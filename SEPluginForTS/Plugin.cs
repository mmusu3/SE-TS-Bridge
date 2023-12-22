using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SEPluginForTS;

public class Plugin
{
    const int minor = 1;
    const int patch = 1;
    const int CurrentVersion = (0xABCD << 16) | (minor << 8) | patch; // 16 bits of magic value, 8 bits of major, 8 bits of minor

    static readonly string PluginName = "SE-TS Bridge";
    static readonly IntPtr PluginNamePtr = StringToHGlobalUTF8(PluginName);

    static readonly string PluginVersion = $"1.{minor}.{patch}";
    static readonly IntPtr PluginVersionPtr = StringToHGlobalUTF8(PluginVersion);

    static readonly string PluginAuthor = "Remaarn";
    static readonly IntPtr PluginAuthorPtr = StringToHGlobalUTF8(PluginAuthor);

    static readonly string PluginDescription = "This plugin integrates with Space Engineers to enable positional audio.";
    static readonly IntPtr PluginDescriptionPtr = StringToHGlobalUTF8(PluginDescription);

    static readonly string CommandKeyword = "setsbridge";
    static readonly IntPtr CommandKeywordPtr = StringToHGlobalUTF8(CommandKeyword);

    static Plugin instance = new();

    TS3Functions functions;
    unsafe byte* pluginID = null;

    ulong connHandlerId;
    ushort localClientId;
    ulong currentChannelId;

    TS3_VECTOR listenerForward = new() { x = 0, y = 0, z = -1 };
    TS3_VECTOR listenerUp = new() { x = 0, y = 1, z = 0 };

    float distanceScale = 0.3f;
    float distanceFalloff = 0.9f;

    ulong localSteamId = 0;
    bool useAntennaConnections = true;

    NamedPipeClientStream pipeStream = null!;
    CancellationTokenSource cancellationTokenSource = null!;
    Task runningTask = null!;

    class Client
    {
        public ulong SteamID;
        public ushort ClientID;
        public string? ClientName;
        public TS3_VECTOR Position;
        public bool HasConnection;
        public bool IsWhispering;
    }

    readonly List<Client> clients = new();

    readonly MemoryPool<byte> memPool = MemoryPool<byte>.Shared;

    static unsafe IntPtr StringToHGlobalUTF8(string? s)
    {
        if (s is null)
            return IntPtr.Zero;

        int nb = System.Text.Encoding.UTF8.GetMaxByteCount(s.Length);
        void* ptr = NativeMemory.Alloc((uint)nb + 1);

        int nbWritten;
        byte* pbMem = (byte*)ptr;

        fixed (char* firstChar = s)
            nbWritten = System.Text.Encoding.UTF8.GetBytes(firstChar, s.Length, pbMem, nb);

        pbMem[nbWritten] = 0;

        return (IntPtr)ptr;
    }

    void Init()
    {
        connHandlerId = functions.getCurrentServerConnectionHandlerID();

        if (connHandlerId != 0 && GetLocalClientAndChannelID())
        {
            RefetchTSClients();
            Set3DSettings(distanceScale, 1);
        }

        CreatePipe();
        runningTask = UpdateLoop(cancellationTokenSource.Token);
    }

    void CreatePipe()
    {
        // TODO: Allow remote computer
        pipeStream = new NamedPipeClientStream(".", "09C842DD-F683-4798-A95F-88B0981265BE", PipeDirection.In, PipeOptions.Asynchronous);
        cancellationTokenSource = new CancellationTokenSource();
    }

    void Dispose()
    {
        Console.WriteLine("[SE-TS Bridge] - Disposing.");

        cancellationTokenSource.Cancel();

        try
        {
            runningTask.Wait(1000);
        }
        catch (AggregateException ex) when (ex.InnerException is TaskCanceledException) { }

        pipeStream.Dispose();

        lock (clients)
        {
            foreach (var item in clients)
                SetClientPos(item.ClientID, default);

            clients.Clear();
        }

        connHandlerId = 0;
        localClientId = 0;
        currentChannelId = 0;

        Console.WriteLine("[SE-TS Bridge] - Disposed.");
    }

    async Task UpdateLoop(CancellationToken cancellationToken)
    {
        bool fistRun = true;

        connect:
        await pipeStream.ConnectAsync(cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return;

        Console.WriteLine("[SE-TS Bridge] - Established connection to Space Engineers plugin.");

        if (fistRun)
        {
            // Message doesn't show if done while TS is starting up.
            _ = Task.Delay(5000, cancellationToken).ContinueWith(t => PrintMessageToCurrentTab("[SE-TS Bridge] - Established connection to Space Engineers plugin."), cancellationToken);
            fistRun = false;
        }
        else
        {
            PrintMessageToCurrentTab("[SE-TS Bridge] - Established connection to Space Engineers plugin.");
        }

        while (pipeStream.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await Update(cancellationToken).ConfigureAwait(false);

                if (result.Result == UpdateResult.OK)
                    continue;

                switch (result.Result)
                {
                case UpdateResult.Canceled:
                    Console.WriteLine($"[SE-TS Bridge] - Update was canceled.");
                    break;
                case UpdateResult.Closed:
                    Console.WriteLine($"[SE-TS Bridge] - Connection was closed. {result.Error}");
                    break;
                case UpdateResult.Corrupt:
                    Console.WriteLine($"[SE-TS Bridge] - Update failed with result {result.Result}. {result.Error}");
                    break;
                }

                break;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Console.WriteLine($"[SE-TS Bridge] - Update was canceled.");
                    break;
                }
                else
                {
                    var msg = $"[SE-TS Bridge] - Exception while updating {ex}";
                    Console.WriteLine(msg);
                    LogMessage(msg, LogLevel.LogLevel_ERROR, "[SE-TS Bridge]");
                }
            }
        }

        if (cancellationToken.IsCancellationRequested)
            return;

        await pipeStream.DisposeAsync().ConfigureAwait(false);

        PrintMessageToCurrentTab("[SE-TS Bridge] - Closed connection to Space Engineers.");

        lock (clients)
        {
            foreach (var item in clients)
            {
                item.Position = default;

                if (item.ClientID != 0)
                    SetClientPos(item.ClientID, default);
            }
        }

        CreatePipe();

        goto connect;
    }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    struct PlayerStatesHeader
    {
        public int Version;
        public ulong LocalSteamId;
        public TS3_VECTOR Forward;
        public TS3_VECTOR Up;
        public int PlayerCount;
        public int RemovedPlayerCount;
        public int NewPlayerCount;
        public int NewPlayerByteLength;

        public unsafe static readonly int Size = sizeof(PlayerStatesHeader);
    }

    struct ClientState
    {
        public ulong SteamID;
        public TS3_VECTOR Position;
        public bool HasConnection;

        public unsafe static readonly int Size = sizeof(ClientState);
    }
#pragma warning restore CS0649

    enum UpdateResult
    {
        OK,
        Canceled,
        Closed,
        Corrupt
    }

    async ValueTask<(UpdateResult Result, string? Error)> Update(CancellationToken cancellationToken)
    {
        using var headerBuffer = memPool.Rent(PlayerStatesHeader.Size);
        var headerMemory = headerBuffer.Memory;
        int bytes = await pipeStream.ReadAsync(headerMemory.Slice(0, PlayerStatesHeader.Size), cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return (UpdateResult.Canceled, null);

        if (bytes == 0)
            return (UpdateResult.Closed, "Pipe returned zero bytes.");

        if (bytes != PlayerStatesHeader.Size)
            return (UpdateResult.Corrupt, $"Expected {PlayerStatesHeader.Size} bytes, got {bytes}");

        var header = MemoryMarshal.Read<PlayerStatesHeader>(headerMemory.Span);

        if (header.Version != CurrentVersion)
            return (UpdateResult.Corrupt, "Invalid data");

        bool steamIdChanged = header.LocalSteamId != localSteamId;

        localSteamId = header.LocalSteamId;

        if (steamIdChanged)
            SendLocalSteamIdToCurrentChannel();

        listenerForward = header.Forward;
        listenerUp = header.Up;

        // No idea what the deal is with this coord system. Just ignore it and do the transform on the game side.
        //if (localClientId != 0)
        //    SetListener(header.Forward, header.Up);

        if (header.PlayerCount == 0 && header.RemovedPlayerCount == 0 && header.NewPlayerCount == 0)
            return (UpdateResult.OK, null);

        int expectedBytes = header.PlayerCount * ClientState.Size
            + header.RemovedPlayerCount * sizeof(ulong)
            + header.NewPlayerByteLength;

        using var memBuffer = memPool.Rent(expectedBytes);
        var memory = memBuffer.Memory.Slice(0, expectedBytes);

        bytes = await pipeStream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return (UpdateResult.Canceled, null);

        if (bytes != expectedBytes)
            return (UpdateResult.Corrupt, $"Expected {expectedBytes} bytes, got {bytes}");

        if (header.PlayerCount != 0)
        {
            ProcessClientStates(memory.Span);
            memory = memory.Slice(header.PlayerCount * ClientState.Size);
        }

        if (header.RemovedPlayerCount != 0)
        {
            ProcessRemovedPlayers(header.RemovedPlayerCount, memory.Span);
            memory = memory.Slice(header.RemovedPlayerCount * sizeof(ulong));
        }

        if (header.NewPlayerCount != 0)
        {
            ProcessNewPlayers(header.NewPlayerCount, memory.Span);
            memory = memory.Slice(header.NewPlayerByteLength);

            if (memory.Length != 0)
                return (UpdateResult.Corrupt, "Not all bytes were processed.");
        }
        else
        {
            if (header.NewPlayerByteLength != 0)
                return (UpdateResult.Corrupt, $"NewPlayerCount was 0 but NewPlayerByteLength was {header.NewPlayerByteLength}");
        }

        return (UpdateResult.OK, null);
    }

    unsafe void SendLocalSteamIdToCurrentChannel()
    {
        if (localSteamId == 0)
            return;

        //Console.WriteLine("[SE-TS Bridge] - SendLocalSteamIdToCurrentChannel()");

        IntPtr command = StringToHGlobalUTF8("TSSE,SteamId:" + localSteamId);

        functions.sendPluginCommand(connHandlerId, pluginID, (byte*)command, (int)PluginTargetMode.PluginCommandTarget_CURRENT_CHANNEL, null, null);

        NativeMemory.Free((void*)command);
    }

    unsafe void SendLocalSteamIdToClient(Client client)
    {
        if (localSteamId == 0)
            return;

        //Console.WriteLine($"[SE-TS Bridge] - SendLocalSteamIdToClient({client.ClientID})");

        IntPtr command = StringToHGlobalUTF8("TSSE,SteamId:" + localSteamId);
        ushort* clientIds = stackalloc ushort[2] { client.ClientID, 0 };

        functions.sendPluginCommand(connHandlerId, pluginID, (byte*)command, (int)PluginTargetMode.PluginCommandTarget_CLIENT, clientIds, null);

        NativeMemory.Free((void*)command);
    }

    void ProcessClientStates(ReadOnlySpan<byte> bytes)
    {
        var states = MemoryMarshal.Cast<byte, ClientState>(bytes);

        //Console.WriteLine($"[SE-TS Bridge] - Processing {states.Length} client states.");

        for (int i = 0; i < states.Length; i++)
        {
            var state = states[i];
            var client = GetClientBySteamId(state.SteamID);

            if (client != null)
            {
                client.Position = state.Position;
                client.HasConnection = state.HasConnection;

                if (localClientId != 0)
                    UpdateClientPosition(client);
            }
            //else
            //{
            //    Console.WriteLine($"[SE-TS Bridge] - Missing game client for SteamID: {state.SteamID}");
            //}
        }
    }

    void ProcessRemovedPlayers(int numRemovedPlayers, ReadOnlySpan<byte> bytes)
    {
        Console.WriteLine($"[SE-TS Bridge] - Removing {numRemovedPlayers} players.");

        lock (clients)
        {
            for (int i = 0; i < numRemovedPlayers; i++)
            {
                ulong steamId = Read<ulong>(ref bytes);
                var client = GetClientBySteamId(steamId);

                if (client != null && client.ClientID != 0)
                {
                    //Console.WriteLine($"[SE-TS Bridge] - Resetting client position for ID: {client.ClientID}.");

                    client.Position = default;
                    SetClientPos(client.ClientID, default);
                }
            }
        }
    }

    void ProcessNewPlayers(int numNewPlayers, ReadOnlySpan<byte> bytes)
    {
        Console.WriteLine($"[SE-TS Bridge] - Received {numNewPlayers} new players.");

        for (int i = 0; i < numNewPlayers; i++)
        {
            ulong id = Read<ulong>(ref bytes);
            int nameLength = Read<int>(ref bytes);
            var name = new string(MemoryMarshal.Cast<byte, char>(bytes).Slice(0, nameLength));

            bytes = bytes.Slice(nameLength * sizeof(char));

            var pos = Read<TS3_VECTOR>(ref bytes);
            bool hasConnection = Read<bool>(ref bytes);
            var client = GetClientBySteamId(id);

            if (client != null)
            {
                Console.WriteLine($"[SE-TS Bridge] - Matching client found for Steam ID. SteamId: {id}, SteamName: {name}");

                client.Position = pos;
                client.HasConnection = hasConnection;
            }
        }
    }

    unsafe static T Read<T>(ref ReadOnlySpan<byte> span) where T : unmanaged
    {
        var value = MemoryMarshal.Read<T>(span);
        span = span.Slice(sizeof(T));
        return value;
    }

    Client? GetClientByClientId(ushort id)
    {
        lock (clients)
        {
            foreach (var item in clients)
            {
                if (item.ClientID == id)
                    return item;
            }
        }

        return null;
    }

    Client? GetClientBySteamId(ulong id)
    {
        lock (clients)
        {
            foreach (var item in clients)
            {
                if (item.SteamID == id)
                    return item;
            }
        }

        return null;
    }

    Client AddClientThreadSafe(ushort id, string? name)
    {
        lock (clients)
            return AddClient(id, name);
    }

    Client AddClient(ushort id, string? name)
    {
        var client = new Client {
            ClientID = id,
            ClientName = name
        };

        clients.Add(client);

        return client;
    }

    void RemoveClientThreadSafe(Client client, bool resetPos)
    {
        lock (clients)
            RemoveClient(client, resetPos);
    }

    void RemoveClient(Client client, bool resetPos)
    {
        if (resetPos)
            SetClientPos(client.ClientID, default);

        bool removed = clients.Remove(client);

        if (!removed)
            Console.WriteLine($"[SE-TS Bridge] - Failed to unregister client. ClientId: {client.ClientID}");
    }

    void RemoveAllClients()
    {
        lock (clients)
        {
            if (clients.Count == 0)
            {
                Console.WriteLine($"[SE-TS Bridge] - Zero clients to remove.");
                return;
            }

            Console.WriteLine($"[SE-TS Bridge] - Removing all {clients.Count} clients.");

            clients.Clear();
            // TODO: Does this need to SetClientPos?
        }
    }

    void SetClientIsWhispering(ushort clientId, bool isWhispering)
    {
        var client = GetClientByClientId(clientId);

        if (client == null)
            return;

        //Console.WriteLine($"[SE-TS Bridge] - Setting client {clientId} whispering state to {isWhispering}");

        if (isWhispering != client.IsWhispering)
        {
            client.IsWhispering = isWhispering;
            UpdateClientPosition(client);
        }
    }

    void UpdateClientPosition(Client client)
    {
        SetClientPos(client.ClientID, client.IsWhispering && (client.HasConnection || !useAntennaConnections) ? default : client.Position);
    }

    unsafe void RefetchTSClients()
    {
        Console.WriteLine("[SE-TS Bridge] - Refetching client list.");

        ushort* clientList;
        var err = (Ts3ErrorType)functions.getChannelClientList(connHandlerId, currentChannelId, &clientList);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            Console.WriteLine($"[SE-TS Bridge] - Failed to get client list. Error: {err}");
            return;
        }

        RemoveAllClients();

        int numAdded = 0;

        lock (clients)
        {
            int i = 0;
            ushort id;

            while ((id = clientList[i++]) != 0)
            {
                if (id == localClientId)
                    continue;

                var name = GetClientName(id);
                AddClient(id, name);
                numAdded++;
            }
        }

        if (numAdded != 0)
            Console.WriteLine($"[SE-TS Bridge] - Added {numAdded} clients.");

        Console.WriteLine($"[SE-TS Bridge] - There are {clients.Count} total clients.");

        err = (Ts3ErrorType)functions.freeMemory(clientList);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"[SE-TS Bridge] - Failed to free client list. Error: {err}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float DistanceSquared(TS3_VECTOR a, TS3_VECTOR b)
    {
        float x = b.x - a.x;
        float y = b.y - a.y;
        float z = b.z - a.z;
        return x * x + y * y + z * z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float Distance(TS3_VECTOR a, TS3_VECTOR b)
    {
        return MathF.Sqrt(DistanceSquared(a, b));
    }

    #region Wrapper Methods

    unsafe void LogMessage(string message, LogLevel level, string? channel)
    {
        byte nullChar = 0;
        var msgPtr = Marshal.StringToHGlobalAnsi(message);
        var chnPtr = channel != null ? Marshal.StringToHGlobalAnsi(channel) : (IntPtr)(&nullChar);

        try
        {
            var err = (Ts3ErrorType)functions.logMessage((byte*)msgPtr, level, (byte*)chnPtr, connHandlerId);

            if (err != Ts3ErrorType.ERROR_ok)
                Console.WriteLine($"[SE-TS Bridge] - Failed to log message \"{message}\"");
        }
        finally
        {
            Marshal.FreeHGlobal(msgPtr);

            if (channel != null)
                Marshal.FreeHGlobal(chnPtr);
        }
    }

    bool Set3DSettings(float distanceFactor, float rolloffScale)
    {
        Console.WriteLine($"[SE-TS Bridge] - Setting system 3D settings. DistanceFactor: {distanceFactor}, RolloffScale: {rolloffScale}.");

        var err = (Ts3ErrorType)functions.systemset3DSettings(connHandlerId, distanceFactor, rolloffScale);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            Console.WriteLine($"[SE-TS Bridge] - Failed to set system 3D settings. Error: {err}");
            return false;
        }

        return true;
    }

    unsafe void SetListener(TS3_VECTOR forward, TS3_VECTOR up)
    {
        //Console.WriteLine($"[SE-TS Bridge] - Setting listener attribs. Forward: {{{forward.x}, {forward.y}, {forward.z}}}, Up: {{{up.x}, {up.y}, {up.z}}}.");

        TS3_VECTOR zeroPos = default;

        var err = (Ts3ErrorType)functions.systemset3DListenerAttributes(connHandlerId, &zeroPos, &forward, &up);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"[SE-TS Bridge] - Failed to set listener attribs. Error: {err}");
    }

    unsafe void SetClientPos(ushort clientId, TS3_VECTOR position)
    {
        if (clientId == 0)
        {
            Console.WriteLine($"[SE-TS Bridge] - Tried to set position of client ID 0.");
            return;
        }

        //Console.WriteLine($"[SE-TS Bridge] - Setting position of client {clientId} to {{{position.x}, {position.y}, {position.z}}}.");

        position.z = -position.z;

        var err = (Ts3ErrorType)functions.channelset3DAttributes(connHandlerId, clientId, &position);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"[SE-TS Bridge] - Failed to set client pos to {{{position.x}, {position.y}, {position.z}}}. Error: {err}");
    }

    unsafe bool GetLocalClientAndChannelID()
    {
        ushort clientId;
        var err = (Ts3ErrorType)functions.getClientID(connHandlerId, &clientId);

        if (err == Ts3ErrorType.ERROR_ok)
        {
            localClientId = clientId;
            Console.WriteLine($"[SE-TS Bridge] - Got client ID: {clientId}");
        }
        else
        {
            Console.WriteLine($"[SE-TS Bridge] - Failed to get client ID. Error: {err}");
            return false;
        }

        ulong channelId;
        err = (Ts3ErrorType)functions.getChannelOfClient(connHandlerId, localClientId, &channelId);

        if (err == Ts3ErrorType.ERROR_ok)
        {
            currentChannelId = channelId;
            Console.WriteLine($"[SE-TS Bridge] - Got channel ID: {channelId}");
        }
        else
        {
            Console.WriteLine($"[SE-TS Bridge] - Failed to get channel ID. Error: {err}");
            return false;
        }

        return true;
    }

    unsafe string? GetClientName(ushort clientId)
    {
        byte* nameBuffer;
        var err = (Ts3ErrorType)functions.getClientVariableAsString(connHandlerId, clientId, (nint)ClientProperties.CLIENT_NICKNAME, &nameBuffer);

        if (err == Ts3ErrorType.ERROR_ok)
        {
            var name = Marshal.PtrToStringUTF8((IntPtr)nameBuffer);
            err = (Ts3ErrorType)functions.freeMemory(nameBuffer);

            if (err != Ts3ErrorType.ERROR_ok)
                Console.WriteLine($"[SE-TS Bridge] - Failed to free client name. Error: {err}");

            return name;
        }
        else
        {
            Console.WriteLine($"[SE-TS Bridge] - Failed to get client name. Error: {err}");
            return null;
        }
    }

    unsafe void SendMessageToClient(ushort clientId, string message)
    {
        IntPtr ptr = Marshal.StringToHGlobalAnsi(message);

        try
        {
            var err = (Ts3ErrorType)functions.requestSendPrivateTextMsg(connHandlerId, (byte*)ptr, clientId, null);

            if (err != Ts3ErrorType.ERROR_ok)
                Console.WriteLine($"[SE-TS Bridge] - Failed to send private message to clientID {clientId}. Error: {err}");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    unsafe void PrintMessageToCurrentTab(string message)
    {
        IntPtr ptr = Marshal.StringToHGlobalAnsi(message);

        try
        {
            functions.printMessageToCurrentTab((byte*)ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    int ProcessCommand(string cmd)
    {
        int spaceIndex = cmd.IndexOf(' ');

        switch (cmd.Substring(0, spaceIndex).ToLowerInvariant())
        {
        case "distancescale":
            {
                if (float.TryParse(cmd.AsSpan(spaceIndex).Trim(), out float value))
                {
                    distanceScale = value;

                    if (Set3DSettings(distanceScale, 1))
                        PrintMessageToCurrentTab($"Setting distance scale to {value}");
                    else
                        PrintMessageToCurrentTab($"Error, failed to set distance scale value.");
                }
                else
                {
                    PrintMessageToCurrentTab($"Error, failed to parse value.");
                }
                break;
            }
        case "distancefalloff":
            {
                if (float.TryParse(cmd.AsSpan(spaceIndex).Trim(), out float value))
                {
                    distanceFalloff = value;
                    PrintMessageToCurrentTab($"Setting distance falloff to {value}");
                }
                else
                {
                    PrintMessageToCurrentTab($"Error, failed to parse value.");
                }
                break;
            }
        case "useantennas":
            {
                if (bool.TryParse(cmd.AsSpan(spaceIndex).Trim(), out bool value))
                {
                    useAntennaConnections = value;
                    PrintMessageToCurrentTab($"Setting use antennas to {value}");
                }
                else
                {
                    PrintMessageToCurrentTab($"Error, failed to parse value.");
                }
                break;
            }
        default:
            PrintMessageToCurrentTab($"Invalid command {cmd}");
            break;
        }

        return 0;
    }

    void HandleClientMoved(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID)
    {
        if (serverConnectionHandlerID != connHandlerId)
            return;

        if (clientID == localClientId)
        {
            currentChannelId = newChannelID;
            Console.WriteLine($"[SE-TS Bridge] - Current channel changed. NewChannelID: {newChannelID}");

            if (newChannelID != 0)
            {
                RefetchTSClients();
                SendLocalSteamIdToCurrentChannel();
            }
        }
        else if (newChannelID == currentChannelId)
        {
            var client = GetClientByClientId(clientID);

            if (client != null)
            {
                Console.WriteLine($"[SE-TS Bridge] - Client joined current channel but was already registered. ClientID: {clientID}, ClientName: {client.ClientName}, NewChannelID: {newChannelID}");
            }
            else
            {
                var name = GetClientName(clientID);
                Console.WriteLine($"[SE-TS Bridge] - New client joined current channel. ClientID: {clientID}, ClientName: {name}, NewChannelID: {newChannelID}");
                client = AddClientThreadSafe(clientID, name);
            }

            SendLocalSteamIdToClient(client);
        }
        else if (oldChannelID == currentChannelId)
        {
            var client = GetClientByClientId(clientID);

            if (client != null)
            {
                Console.WriteLine($"[SE-TS Bridge] - Client left current channel. ClientID: {clientID}, ClientName: {client.ClientName}, NewChannelID: {newChannelID}");
                RemoveClientThreadSafe(client, newChannelID != 0);
            }
            else
            {
                Console.WriteLine($"[SE-TS Bridge] - Unregisterd client left current channel. ClientID: {clientID}, OldChannelID: {oldChannelID}");
            }
        }
        else
        {
            // A client either moved between channels that aren't the current channnel or they left the server.
        }
    }

    #endregion

    // Docs https://teamspeakdocs.github.io/PluginAPI/client_html/index.html

    #region Required functions
#pragma warning disable IDE1006 // Naming Styles

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_name")] public unsafe static byte* ts3plugin_name() => (byte*)PluginNamePtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_version")] public unsafe static byte* ts3plugin_version() => (byte*)PluginVersionPtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_apiVersion")] public unsafe static int ts3plugin_apiVersion() => 23;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_author")] public unsafe static byte* ts3plugin_author() => (byte*)PluginAuthorPtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_description")] public unsafe static byte* ts3plugin_description() => (byte*)PluginDescriptionPtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_setFunctionPointers")] public unsafe static void ts3plugin_setFunctionPointers(TS3Functions funcs) => instance.functions = funcs;

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_init")]
    public static int ts3plugin_init()
    {
        instance.Init();

        // 0 = success, 1 = failure, -2 = failure but client will not show a "failed to load" warning
        // -2 is a very special case and should only be used if a plugin displays a dialog (e.g. overlay) asking the user to disable
        // the plugin again, avoiding the show another dialog by the client telling the user the plugin failed to load.
        // For normal case, if a plugin really failed to load because of an error, the correct return value is 1.

        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_shutdown")]
    public unsafe static void ts3plugin_shutdown()
    {
        // Note:
        // If your plugin implements a settings dialog, it must be closed and deleted here, else the
        // TeamSpeak client will most likely crash (DLL removed but dialog from DLL code still open).

        // Free pluginID if we registered it
        if (instance.pluginID != null)
        {
            NativeMemory.Free(instance.pluginID);
            instance.pluginID = null;
        }

        instance.Dispose();
    }

    #endregion

    #region Optional functions

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_registerPluginID")]
    public unsafe static void ts3plugin_registerPluginID(/*const */byte* id)
    {
        var charSpan = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(id);
        uint sz = (uint)charSpan.Length + 1;

        instance.pluginID = (byte*)NativeMemory.Alloc(sz);

        // The id buffer will invalidate after exiting this function.
        Unsafe.CopyBlock(instance.pluginID, id, sz);

        Console.WriteLine("[SE-TS Bridge] - Registered plugin ID: " + System.Text.Encoding.UTF8.GetString(charSpan));
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_commandKeyword")]
    public unsafe static /*const */byte* ts3plugin_commandKeyword()
    {
        return (byte*)CommandKeywordPtr;
    }

    // Plugin processes console command. Return 0 if plugin handled the command, 1 if not handled.
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_processCommand")]
    public unsafe static int ts3plugin_processCommand(ulong serverConnectionHandlerID, /*const */byte* command)
    {
        var cmd = Marshal.PtrToStringUTF8((IntPtr)command);

        if (cmd == null)
            return 1;

        return instance.ProcessCommand(cmd);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_currentServerConnectionChanged")]
    public unsafe static void ts3plugin_currentServerConnectionChanged(ulong serverConnectionHandlerID)
    {
        instance.connHandlerId = serverConnectionHandlerID;
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onConnectStatusChangeEvent")]
    public unsafe static void ts3plugin_onConnectStatusChangeEvent(ulong serverConnectionHandlerID, int newStatus, uint errorNumber)
    {
        //Console.WriteLine($"[SE-TS Bridge] - ConnectStatusChangeEvent. NewStatus: {newStatus}");

        var connStatus = (ConnectStatus)newStatus;

        if (connStatus == ConnectStatus.STATUS_CONNECTION_ESTABLISHED)
        {
            if (instance.GetLocalClientAndChannelID())
            {
                instance.RefetchTSClients();
                instance.Set3DSettings(instance.distanceScale, 1);
            }
        }
        else if (connStatus == ConnectStatus.STATUS_DISCONNECTED)
        {
            instance.localClientId = 0;
            instance.currentChannelId = 0;
            instance.RemoveAllClients();
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientMoveEvent")]
    public unsafe static void ts3plugin_onClientMoveEvent(ulong serverConnectionHandlerID, ushort clientID,
        ulong oldChannelID, ulong newChannelID, Visibility visibility, byte* moveMessage)
    {
        // Client moved themself
        instance.HandleClientMoved(serverConnectionHandlerID, clientID, oldChannelID, newChannelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientMoveTimeoutEvent")]
    public unsafe static void ts3plugin_onClientMoveTimeoutEvent(ulong serverConnectionHandlerID, ushort clientID,
        ulong oldChannelID, ulong newChannelID, Visibility visibility, /*const */byte* timeoutMessage)
    {
        instance.HandleClientMoved(serverConnectionHandlerID, clientID, oldChannelID, newChannelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientMoveMovedEvent")]
    public unsafe static void ts3plugin_onClientMoveMovedEvent(ulong serverConnectionHandlerID, ushort clientID,
        ulong oldChannelID, ulong newChannelID, Visibility visibility,
        ushort moverID, /*const */byte* moverName, /*const */byte* moverUniqueIdentifier, /*const */byte* moveMessage)
    {
        // Client was moved by another
        instance.HandleClientMoved(serverConnectionHandlerID, clientID, oldChannelID, newChannelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientKickFromChannelEvent")]
    public unsafe static void ts3plugin_onClientKickFromChannelEvent(ulong serverConnectionHandlerID, ushort clientID,
        ulong oldChannelID, ulong newChannelID, Visibility visibility,
        ushort kickerID, /*const */byte* kickerName, /*const */byte* kickerUniqueIdentifier, /*const */byte* kickMessage)
    {
        instance.HandleClientMoved(serverConnectionHandlerID, clientID, oldChannelID, newChannelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientKickFromServerEvent")]
    public unsafe static void ts3plugin_onClientKickFromServerEvent(ulong serverConnectionHandlerID, ushort clientID,
        ulong oldChannelID, ulong newChannelID, Visibility visibility,
        ushort kickerID, /*const */byte* kickerName, /*const */byte* kickerUniqueIdentifier, /*const */byte* kickMessage)
    {
        instance.HandleClientMoved(serverConnectionHandlerID, clientID, oldChannelID, newChannelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onTalkStatusChangeEvent")]
    public static void ts3plugin_onTalkStatusChangeEvent(ulong serverConnectionHandlerID, int status, int isReceivedWhisper, ushort clientID)
    {
        if (serverConnectionHandlerID != instance.connHandlerId)
            return;

        if (isReceivedWhisper == 1) // Event caused by whisper
            instance.SetClientIsWhispering(clientID, (TalkStatus)status == TalkStatus.STATUS_TALKING);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onCustom3dRolloffCalculationClientEvent")]
    public unsafe static void ts3plugin_onCustom3dRolloffCalculationClientEvent(ulong serverConnectionHandlerID, ushort clientID, float distance, float* volume)
    {
        if (serverConnectionHandlerID != instance.connHandlerId)
            return;

        var client = instance.GetClientByClientId(clientID);

        if (client == null)
            return;

        // TODO: Scale volume down when client is facing away

        float dist = Distance(default, client.Position);
        *volume = Math.Clamp(1f / MathF.Pow((dist * instance.distanceScale) + 0.6f, instance.distanceFalloff), 0, 1);

        //Console.WriteLine($"DistScale: {instance.distanceScale}, DistFalloff: {instance.distanceFalloff}, Dist: {dist}, Vol: {*volume}");
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onPluginCommandEvent")]
    public unsafe static void ts3plugin_onPluginCommandEvent(ulong serverConnectionHandlerID, byte* pluginName, byte* pluginCommand,
        ushort invokerClientID, byte* invokerName, byte* invokerUniqueIdentity)
    {
        if (serverConnectionHandlerID != instance.connHandlerId)
            return;

        // Commands can get sent to yourself
        if (invokerClientID == instance.localClientId)
            return;

        var pName = Marshal.PtrToStringUTF8((nint)pluginName);

        if (pName != "TS-SE_Plugin") // Seems to use the dll name
            return;

        var cmd = Marshal.PtrToStringUTF8((nint)pluginCommand);
        var invoker = Marshal.PtrToStringUTF8((nint)invokerName);

        var client = instance.GetClientByClientId(invokerClientID);

        if (client == null)
        {
            Console.WriteLine($"[SE-TS Bridge] - Received plugin command from unregistered client, ClientId: {invokerClientID}, InvokerName: {invoker}");
            return;
        }

        Console.WriteLine($"[SE-TS Bridge] - Received plugin command, PluginName: {pName}, Command: {cmd}, InvokerClientId: {invokerClientID}, InvokerName: {invoker}");

        if (cmd != null && cmd.StartsWith("TSSE,SteamId:"))
        {
            if (ulong.TryParse(cmd.AsSpan("TSSE,SteamId:".Length), out ulong steamId))
            {
                //Console.WriteLine($"[SE-TS Bridge] - Recieved SteamId for client, ClientId: {invokerClientID}, SteamId: {steamId}");
                client.SteamID = steamId;
                return;
            }
        }

        Console.WriteLine($"[SE-TS Bridge] - Received invalid plugin command, Command: {cmd}, ClientId: {invokerClientID}");
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientDisplayNameChanged")]
    public unsafe static void ts3plugin_onClientDisplayNameChanged(ulong serverConnectionHandlerID, ushort clientID, /*const */byte* displayName, /*const */byte* uniqueClientIdentifier)
    {
        if (serverConnectionHandlerID != instance.connHandlerId)
            return;

        if (clientID == instance.localClientId)
            return;

        var name = Marshal.PtrToStringUTF8((IntPtr)displayName);
        var client = instance.GetClientByClientId(clientID);

        if (client != null)
        {
            client.ClientName = name;
        }
        else
        {
            //Console.WriteLine($"[SE-TS Bridge] - Unregisterd client changed display name. ClientID: {clientID}");
        }
    }

#pragma warning restore IDE1006 // Naming Styles
    #endregion
}
