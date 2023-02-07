using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TSSEPlugin;

public static class Plugin
{
    static readonly string PluginName = "TS-SE Plugin";
    static readonly IntPtr PluginNamePtr = StringToHGlobalUTF8(PluginName);

    static readonly string PluginVersion = "1.0.8";
    static readonly IntPtr PluginVersionPtr = StringToHGlobalUTF8(PluginVersion);

    static readonly string PluginAuthor = "Remaarn";
    static readonly IntPtr PluginAuthorPtr = StringToHGlobalUTF8(PluginAuthor);

    static readonly string PluginDescription = "This plugin integrates with Space Engineers to enable positional audio.";
    static readonly IntPtr PluginDescriptionPtr = StringToHGlobalUTF8(PluginDescription);

    static readonly string CommandKeyword = "setsbridge";
    static readonly IntPtr CommandKeywordPtr = StringToHGlobalUTF8(CommandKeyword);

    static TS3Functions functions;
    unsafe static byte* pluginID = null;

    static ulong connHandlerId;
    static ushort localClientId;
    static ulong currentChannelId;

    static TS3_VECTOR listenerForward = new() { x = 0, y = 0, z = -1 };
    static TS3_VECTOR listenerUp = new() { x = 0, y = 1, z = 0 };

    static float distanceScale = 0.3f;
    static float distanceFalloff = 0.9f;

    static NamedPipeClientStream pipeStream = null!;
    static CancellationTokenSource cancellationTokenSource = null!;
    static Task runningTask = null!;

    class Client
    {
        public ulong SteamID;
        public ushort ClientID;
        public string? ClientName;
        public TS3_VECTOR Position;
        public bool IsWhispering;
    }

    static ulong localSteamId = 0;

    static readonly List<Client> clients = new();

    static readonly MemoryPool<byte> memPool = MemoryPool<byte>.Shared;

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

    static void Init()
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

    static void CreatePipe()
    {
        pipeStream = new NamedPipeClientStream(".", "09C842DD-F683-4798-A95F-88B0981265BE", PipeDirection.In, PipeOptions.Asynchronous);
        cancellationTokenSource = new CancellationTokenSource();
    }

    static void Dispose()
    {
        Console.WriteLine("TS-SE Plugin - Disposing.");

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

        Console.WriteLine("TS-SE Plugin - Disposed.");
    }

    async static Task UpdateLoop(CancellationToken cancellationToken)
    {
        bool fistRun = true;

        connect:
        await pipeStream.ConnectAsync(cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return;

        Console.WriteLine("TS-SE Plugin - Established connection to Space Engineers plugin.");

        if (fistRun)
        {
            // Message doesn't show if done while TS is starting up.
            _ = Task.Delay(5000, cancellationToken).ContinueWith(t => PrintMessageToCurrentTab("TS-SE Plugin - Established connection to Space Engineers plugin."), cancellationToken);
            fistRun = false;
        }
        else
        {
            PrintMessageToCurrentTab("TS-SE Plugin - Established connection to Space Engineers plugin.");
        }

        while (pipeStream.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await Update(cancellationToken).ConfigureAwait(false);

                if (result.Result == UpdateResult.OK)
                    continue;

                else if (result.Result == UpdateResult.Corrupt)
                    Console.WriteLine($"TS-SE Plugin - Update failed with result {result.Result}. {result.Error}");

                else if (result.Result == UpdateResult.Canceled)
                    Console.WriteLine($"TS-SE Plugin - Update was canceled.");

                else if (result.Result == UpdateResult.Closed)
                    Console.WriteLine($"TS-SE Plugin - Connection was closed. {result.Error}");

                break;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Console.WriteLine($"TS-SE Plugin - Update was canceled.");
                    break;
                }
                else
                {
                    var msg = $"TS-SE Plugin - Exception while updating {ex}";
                    Console.WriteLine(msg);
                    LogMessage(msg, LogLevel.LogLevel_ERROR, "TS-SE Plugin");
                }
            }
        }

        if (cancellationToken.IsCancellationRequested)
            return;

        await pipeStream.DisposeAsync().ConfigureAwait(false);

        PrintMessageToCurrentTab("TS-SE Plugin - Closed connection to Space Engineers.");

        RemoveGameOnlyClients();

        CreatePipe();
        goto connect;
    }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    struct Header
    {
        public int CheckValue;
        public ulong LocalSteamId;
        public TS3_VECTOR Forward;
        public TS3_VECTOR Up;
        public int PlayerCount;
        public int RemovedPlayerCount;
        public int NewPlayerCount;
        public int NewPlayerByteLength;

        public unsafe static readonly int Size = sizeof(Header);
    }

    struct ClientState
    {
        public ulong SteamID;
        public TS3_VECTOR Position;

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

    async static ValueTask<(UpdateResult Result, string? Error)> Update(CancellationToken cancellationToken)
    {
        using var headerBuffer = memPool.Rent(Header.Size);
        var headerMemory = headerBuffer.Memory;
        int bytes = await pipeStream.ReadAsync(headerMemory.Slice(0, Header.Size), cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return (UpdateResult.Canceled, null);

        if (bytes == 0)
            return (UpdateResult.Closed, "Pipe returned zero bytes.");

        if (bytes != Header.Size)
            return (UpdateResult.Corrupt, $"Expected {Header.Size} bytes, got {bytes}");

        var header = MemoryMarshal.Read<Header>(headerMemory.Span);

        if (header.CheckValue != 0x12ABCDEF)
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

    unsafe static void SendLocalSteamIdToCurrentChannel()
    {
        IntPtr command = StringToHGlobalUTF8("TSSE,SteamId:" + localSteamId);

        functions.sendPluginCommand(connHandlerId, pluginID, (byte*)command, (int)PluginTargetMode.PluginCommandTarget_CURRENT_CHANNEL, null, null);

        NativeMemory.Free((void*)command);
    }

    unsafe static void SendLocalSteamIdToClient(Client client)
    {
        IntPtr command = StringToHGlobalUTF8("TSSE,SteamId:" + localSteamId);
        ushort* clientIds = stackalloc ushort[2] { client.ClientID, 0 };

        functions.sendPluginCommand(connHandlerId, pluginID, (byte*)command, (int)PluginTargetMode.PluginCommandTarget_CLIENT, clientIds, null);

        NativeMemory.Free((void*)command);
    }

    static void ProcessClientStates(ReadOnlySpan<byte> bytes)
    {
        var states = MemoryMarshal.Cast<byte, ClientState>(bytes);

        //Console.WriteLine($"TS-SE Plugin - Processing {states.Length} client states.");

        for (int i = 0; i < states.Length; i++)
        {
            var state = states[i];
            var client = GetClientBySteamId(state.SteamID);

            if (client != null)
            {
                client.Position = state.Position;

                if (localClientId != 0)
                    UpdateClientPosition(client);
            }
            else
            {
                // TODO: Add client without name
                Console.WriteLine($"TS-SE Plugin - Missing game client for SteamID: {state.SteamID}");
            }
        }
    }

    static void ProcessRemovedPlayers(int numRemovedPlayers, ReadOnlySpan<byte> bytes)
    {
        Console.WriteLine($"TS-SE Plugin - Removing {numRemovedPlayers} game clients.");

        lock (clients)
        {
            for (int i = 0; i < numRemovedPlayers; i++)
            {
                ulong steamId = Read<ulong>(ref bytes);

                for (int j = clients.Count - 1; j >= 0; j--)
                {
                    var client = clients[j];

                    if (client.SteamID != steamId)
                        continue;

                    client.SteamID = 0;
                    client.Position = default;

                    if (client.ClientID != 0)
                        SetClientPos(client.ClientID, default);
                    else
                        clients.RemoveAt(j);

                    break;
                }
            }
        }
    }

    static void ProcessNewPlayers(int numNewPlayers, ReadOnlySpan<byte> bytes)
    {
        Console.WriteLine($"TS-SE Plugin - Received {numNewPlayers} new game clients.");

        for (int i = 0; i < numNewPlayers; i++)
        {
            ulong id = Read<ulong>(ref bytes);
            int nameLength = Read<int>(ref bytes);
            var name = new string(MemoryMarshal.Cast<byte, char>(bytes).Slice(0, nameLength));

            bytes = bytes.Slice(nameLength * sizeof(char));

            var client = GetClientBySteamId(id);

            if (client == null)
            {
                client = new Client { SteamID = id };

                lock (clients)
                    clients.Add(client);

                Console.WriteLine($"TS-SE Plugin - Created client from Steam ID. SteamId: {id}, SteamName:{name}");
            }
            else
            {
                Console.WriteLine($"TS-SE Plugin - Matching client found for Steam ID. SteamId: {id}, SteamName:{name}");
            }

            client.Position = Read<TS3_VECTOR>(ref bytes);
        }
    }

    unsafe static T Read<T>(ref ReadOnlySpan<byte> span) where T : unmanaged
    {
        var value = MemoryMarshal.Read<T>(span);
        span = span.Slice(sizeof(T));
        return value;
    }

    static Client? GetClientByClientId(ushort id)
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

    static Client? GetClientBySteamId(ulong id)
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

    static Client AddClientThreadSafe(ushort id, string? name)
    {
        lock (clients)
            return AddClient(id, name);
    }

    static Client AddClient(ushort id, string? name)
    {
        var client = new Client {
            ClientID = id,
            ClientName = name
        };

        clients.Add(client);

        return client;
    }

    static void RemoveClientThreadSafe(Client client, bool resetPos)
    {
        lock (clients)
            RemoveClient(client, resetPos);
    }

    static void RemoveClient(Client client, bool resetPos)
    {
        if (resetPos)
            SetClientPos(client.ClientID, default);

        bool removed = clients.Remove(client);

        if (!removed)
            Console.WriteLine($"TS-SE Plugin - Failed to unregister client. ClientId: {client.ClientID}");

        client.ClientID = 0;
        client.ClientName = null;
    }

    static void RemoveTSOnlyClients()
    {
        lock (clients)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                if (clients[i].SteamID == 0)
                    clients.RemoveAt(i);
            }
        }
    }

    static void RemoveGameOnlyClients()
    {
        lock (clients)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                if (clients[i].ClientID == 0)
                    clients.RemoveAt(i);
            }
        }
    }

    static void SetClientIsWhispering(ushort clientId, bool isWhispering)
    {
        var client = GetClientByClientId(clientId);

        if (client == null)
            return;

        //Console.WriteLine($"TS-SE Plugin - Setting client {clientId} whispering state to {isWhispering}");

        client.IsWhispering = isWhispering;
        UpdateClientPosition(client);
    }

    static void UpdateClientPosition(Client client)
    {
        SetClientPos(client.ClientID, client.IsWhispering ? default : client.Position);
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

    unsafe static void LogMessage(string message, LogLevel level, string? channel)
    {
        byte nullChar = 0;
        var msgPtr = Marshal.StringToHGlobalAnsi(message);
        var chnPtr = channel != null ? Marshal.StringToHGlobalAnsi(channel) : (IntPtr)(&nullChar);

        try
        {
            var err = (Ts3ErrorType)functions.logMessage((byte*)msgPtr, level, (byte*)chnPtr, connHandlerId);

            if (err != Ts3ErrorType.ERROR_ok)
                Console.WriteLine($"TS-SE Plugin - Failed to log message \"{message}\"");
        }
        finally
        {
            Marshal.FreeHGlobal(msgPtr);

            if (channel != null)
                Marshal.FreeHGlobal(chnPtr);
        }
    }

    static bool Set3DSettings(float distanceFactor, float rolloffScale)
    {
        Console.WriteLine($"TS-SE Plugin - Setting system 3D settings. DistanceFactor: {distanceFactor}, RolloffScale: {rolloffScale}.");

        var err = (Ts3ErrorType)functions.systemset3DSettings(connHandlerId, distanceFactor, rolloffScale);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            Console.WriteLine($"TS-SE Plugin - Failed to set system 3D settings. Error: {err}");
            return false;
        }

        return true;
    }

    unsafe static void SetListener(TS3_VECTOR forward, TS3_VECTOR up)
    {
        //Console.WriteLine($"TS-SE Plugin - Setting listener attribs. Forward: {{{forward.x}, {forward.y}, {forward.z}}}, Up: {{{up.x}, {up.y}, {up.z}}}.");

        TS3_VECTOR zeroPos = default;

        var err = (Ts3ErrorType)functions.systemset3DListenerAttributes(connHandlerId, &zeroPos, &forward, &up);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"TS-SE Plugin - Failed to set listener attribs. Error: {err}");
    }

    unsafe static void SetClientPos(ushort clientId, TS3_VECTOR position)
    {
        if (clientId == 0)
            return;

        //Console.WriteLine($"TS-SE Plugin - Setting position of client {clientId} to {{{position.x}, {position.y}, {position.z}}}.");

        position.z = -position.z;

        var err = (Ts3ErrorType)functions.channelset3DAttributes(connHandlerId, clientId, &position);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"TS-SE Plugin - Failed to set client pos to {{{position.x}, {position.y}, {position.z}}}. Error: {err}");
    }

    unsafe static void RefetchTSClients()
    {
        Console.WriteLine("TS-SE Plugin - Refetching client list.");

        ushort* clientList;
        var err = (Ts3ErrorType)functions.getChannelClientList(connHandlerId, currentChannelId, &clientList);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            Console.WriteLine($"TS-SE Plugin - Failed to get client list. Error: {err}");
            return;
        }

        RemoveTSOnlyClients();

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
            Console.WriteLine($"TS-SE Plugin - Added {numAdded} clients.");

        err = (Ts3ErrorType)functions.freeMemory(clientList);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"TS-SE Plugin - Failed to free client list. Error: {err}");
    }

    unsafe static bool GetLocalClientAndChannelID()
    {
        ushort clientId;
        var err = (Ts3ErrorType)functions.getClientID(connHandlerId, &clientId);

        if (err == Ts3ErrorType.ERROR_ok)
        {
            localClientId = clientId;
            Console.WriteLine($"TS-SE Plugin - Got client ID: {clientId}");
        }
        else
        {
            Console.WriteLine($"TS-SE Plugin - Failed to get client ID. Error: {err}");
            return false;
        }

        ulong channelId;
        err = (Ts3ErrorType)functions.getChannelOfClient(connHandlerId, localClientId, &channelId);

        if (err == Ts3ErrorType.ERROR_ok)
        {
            currentChannelId = channelId;
            Console.WriteLine($"TS-SE Plugin - Got channel ID: {channelId}");
        }
        else
        {
            Console.WriteLine($"TS-SE Plugin - Failed to get channel ID. Error: {err}");
            return false;
        }

        return true;
    }

    unsafe static string? GetClientName(ushort clientId)
    {
        byte* nameBuffer;
        var err = (Ts3ErrorType)functions.getClientVariableAsString(connHandlerId, clientId, (nint)ClientProperties.CLIENT_NICKNAME, &nameBuffer);

        if (err == Ts3ErrorType.ERROR_ok)
        {
            var name = Marshal.PtrToStringUTF8((IntPtr)nameBuffer);
            err = (Ts3ErrorType)functions.freeMemory(nameBuffer);

            if (err != Ts3ErrorType.ERROR_ok)
                Console.WriteLine($"TS-SE Plugin - Failed to free client name. Error: {err}");

            return name;
        }
        else
        {
            Console.WriteLine($"TS-SE Plugin - Failed to get client name. Error: {err}");
            return null;
        }
    }

    unsafe static void SendMessageToClient(ushort clientId, string message)
    {
        IntPtr ptr = Marshal.StringToHGlobalAnsi(message);

        try
        {
            var err = (Ts3ErrorType)functions.requestSendPrivateTextMsg(connHandlerId, (byte*)ptr, clientId, null);

            if (err != Ts3ErrorType.ERROR_ok)
                Console.WriteLine($"TS-SE Plugin - Failed to send private message to clientID {clientId}. Error: {err}");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    unsafe static void PrintMessageToCurrentTab(string message)
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

    #endregion

    // Docs https://teamspeakdocs.github.io/PluginAPI/client_html/index.html

    #region Required functions
#pragma warning disable IDE1006 // Naming Styles

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_name")] public unsafe static byte* ts3plugin_name() => (byte*)PluginNamePtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_version")] public unsafe static byte* ts3plugin_version() => (byte*)PluginVersionPtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_apiVersion")] public unsafe static int ts3plugin_apiVersion() => 23;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_author")] public unsafe static byte* ts3plugin_author() => (byte*)PluginAuthorPtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_description")] public unsafe static byte* ts3plugin_description() => (byte*)PluginDescriptionPtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_setFunctionPointers")] public unsafe static void ts3plugin_setFunctionPointers(TS3Functions funcs) => functions = funcs;

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_init")]
    public static int ts3plugin_init()
    {
        Init();

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
        if (pluginID != null)
        {
            NativeMemory.Free(pluginID);
            pluginID = null;
        }

        Dispose();
    }

    #endregion

    #region Optional functions

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_registerPluginID")]
    public unsafe static void ts3plugin_registerPluginID(/*const */byte* id)
    {
        var charSpan = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(id);
        uint sz = (uint)charSpan.Length + 1;

        pluginID = (byte*)NativeMemory.Alloc(sz);

        // The id buffer will invalidate after exiting this function.
        Unsafe.CopyBlock(pluginID, id, sz);

        Console.WriteLine("TS-SE Plugin - Registered plugin ID: " + System.Text.Encoding.UTF8.GetString(charSpan));
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_commandKeyword")]
    public unsafe static /*const */byte* ts3plugin_commandKeyword()
    {
        return (byte*)CommandKeywordPtr;
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_processCommand")]
    public unsafe static int ts3plugin_processCommand(ulong serverConnectionHandlerID, /*const */byte* command)
    {
        var cmd = Marshal.PtrToStringUTF8((IntPtr)command);

        if (cmd == null)
            return 1;

        if (cmd.StartsWith("distancescale ", StringComparison.OrdinalIgnoreCase))
        {
            if (float.TryParse(cmd.AsSpan()["distancescale ".Length..].Trim(), out float value))
            {
                distanceScale = value;

                if (Set3DSettings(distanceScale, 1))
                    PrintMessageToCurrentTab($"Setting distance scale to {distanceScale}");
                else
                    PrintMessageToCurrentTab($"Error, failed to set distance scale value.");
            }
            else
            {
                PrintMessageToCurrentTab($"Error, failed to parse value.");
            }
        }
        else if (cmd.StartsWith("distancefalloff ", StringComparison.OrdinalIgnoreCase))
        {
            if (float.TryParse(cmd.AsSpan()["distancefalloff ".Length..].Trim(), out float value))
            {
                distanceFalloff = value;
                PrintMessageToCurrentTab($"Setting distance falloff to {distanceFalloff}");
            }
            else
            {
                PrintMessageToCurrentTab($"Error, failed to parse value.");
            }
        }

        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_currentServerConnectionChanged")]
    public unsafe static void ts3plugin_currentServerConnectionChanged(ulong serverConnectionHandlerID)
    {
        connHandlerId = serverConnectionHandlerID;
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onConnectStatusChangeEvent")]
    public unsafe static void ts3plugin_onConnectStatusChangeEvent(ulong serverConnectionHandlerID, int newStatus, uint errorNumber)
    {
        var connStatus = (ConnectStatus)newStatus;

        if (connStatus == ConnectStatus.STATUS_CONNECTION_ESTABLISHED)
        {
            if (GetLocalClientAndChannelID())
            {
                RefetchTSClients();
                Set3DSettings(distanceScale, 1);
            }
        }
        else if (connStatus == ConnectStatus.STATUS_DISCONNECTED)
        {
            localClientId = 0;
            currentChannelId = 0;
            RemoveTSOnlyClients();
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientMoveEvent")]
    public unsafe static void ts3plugin_onClientMoveEvent(ulong serverConnectionHandlerID, ushort clientID,
        ulong oldChannelID, ulong newChannelID, Visibility visibility, byte* moveMessage)
    {
        // Client moved themself
        HandleClientMoved(serverConnectionHandlerID, clientID, oldChannelID, newChannelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientMoveMovedEvent")]
    public unsafe static void ts3plugin_onClientMoveMovedEvent(ulong serverConnectionHandlerID, ushort clientID,
        ulong oldChannelID, ulong newChannelID, Visibility visibility,
        ushort moverID, /*const */byte* moverName, /*const */byte* moverUniqueIdentifier, /*const */byte* moveMessage)
    {
        // Client was moved by another
        HandleClientMoved(serverConnectionHandlerID, clientID, oldChannelID, newChannelID);
    }

    static void HandleClientMoved(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID)
    {
        if (clientID == localClientId)
        {
            // TODO: Check serverConnectionHandlerID against current

            currentChannelId = newChannelID;
            Console.WriteLine($"TS-SE Plugin - Current channel changed. NewChannelID: {newChannelID}");

            if (newChannelID != 0)
                RefetchTSClients();
        }
        else if (newChannelID == currentChannelId)
        {
            var client = GetClientByClientId(clientID);

            if (client != null)
            {
                Console.WriteLine($"TS-SE Plugin - Client joined current channel but was already registered. ClientID: {clientID}, ClientName: {client.ClientName}, NewChannelID: {newChannelID}");
            }
            else
            {
                var name = GetClientName(clientID);
                Console.WriteLine($"TS-SE Plugin - New client joined current channel. ClientID: {clientID}, ClientName: {name}, NewChannelID: {newChannelID}");
                client = AddClientThreadSafe(clientID, name);
            }

            SendLocalSteamIdToClient(client);
        }
        else if (oldChannelID == currentChannelId)
        {
            var client = GetClientByClientId(clientID);

            if (client != null)
            {
                Console.WriteLine($"TS-SE Plugin - Client left current channel. ClientID: {clientID}, ClientName: {client.ClientName}, NewChannelID: {newChannelID}");
                RemoveClientThreadSafe(client, newChannelID != 0);
            }
            else
            {
                Console.WriteLine($"TS-SE Plugin - Unregisterd client left current channel. ClientID: {clientID}, OldChannelID: {oldChannelID}");
            }
        }
        else
        {
            // A client either moved between channels that aren't the current channnel or they left the server.
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onTalkStatusChangeEvent")]
    public static void ts3plugin_onTalkStatusChangeEvent(ulong serverConnectionHandlerID, int status, int isReceivedWhisper, ushort clientID)
    {
        if ((TalkStatus)status == TalkStatus.STATUS_TALKING)
            SetClientIsWhispering(clientID, isReceivedWhisper != 0);
        else
            SetClientIsWhispering(clientID, false);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onCustom3dRolloffCalculationClientEvent")]
    public unsafe static void ts3plugin_onCustom3dRolloffCalculationClientEvent(ulong serverConnectionHandlerID, ushort clientID, float distance, float* volume)
    {
        var client = GetClientByClientId(clientID);

        if (client == null)
            return;

        float dist = Distance(default, client.Position);
        *volume = Math.Clamp(1f / MathF.Pow((dist * distanceScale) + 0.6f, distanceFalloff), 0, 1);

        //Console.WriteLine($"DistScale: {distanceScale}, DistFalloff: {distanceFalloff}, Dist: {dist}, Vol: {*volume}");
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onPluginCommandEvent")]
    public unsafe static void ts3plugin_onPluginCommandEvent(ulong serverConnectionHandlerID, byte* pluginName, byte* pluginCommand,
        ushort invokerClientID, byte* invokerName, byte* invokerUniqueIdentity)
    {
        // Commands can get sent to yourself
        if (invokerClientID == localClientId)
            return;

        var pName = Marshal.PtrToStringUTF8((nint)pluginName);

        if (pName != "SE-TS_Plugin") // Seems to use the dll name
            return;

        var cmd = Marshal.PtrToStringUTF8((nint)pluginCommand);
        var invoker = Marshal.PtrToStringUTF8((nint)invokerName);

        Console.WriteLine($"TS-SE Plugin - Received plugin command, PluginName: {pName}, Command: {cmd}, InvokerClientId: {invokerClientID}, InvokerName: {invoker}");

        var tsClient = GetClientByClientId(invokerClientID);

        if (tsClient == null)
        {
            Console.WriteLine($"TS-SE Plugin - Received plugin command from unregistered client, ClientId:{invokerClientID}, InvokerName:{invoker}");
            return;
        }

        if (cmd != null && cmd.StartsWith("TSSE,SteamId:"))
        {
            if (ulong.TryParse(cmd.AsSpan("TSSE,SteamId:".Length), out ulong steamId))
            {
                tsClient.SteamID = steamId;

                var gameClient = GetClientBySteamId(steamId);

                if (gameClient != null)
                {
                    Console.WriteLine($"TS-SE Plugin - Merging clients, ClientId:{invokerClientID}, SteamId:{steamId}");

                    tsClient.Position = gameClient.Position;

                    lock (clients)
                        clients.Remove(gameClient);
                }
                else
                {
                    Console.WriteLine($"TS-SE Plugin - Recieved SteamId for client, ClientId:{invokerClientID}, SteamId:{steamId}");
                }

                return;
            }
        }

        Console.WriteLine($"TS-SE Plugin - Received invalid plugin command, Command:{cmd}, ClientId:{invokerClientID}");
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientDisplayNameChanged")]
    public unsafe static void ts3plugin_onClientDisplayNameChanged(ulong serverConnectionHandlerID, ushort clientID, /*const */byte* displayName, /*const */byte* uniqueClientIdentifier)
    {
        if (clientID == localClientId)
            return;

        // TODO: Check serverConnectionHandlerID against current

        var name = Marshal.PtrToStringUTF8((IntPtr)displayName);
        var client = GetClientByClientId(clientID);

        if (client != null)
        {
            client.ClientName = name;
        }
        else
        {
            Console.WriteLine($"TS-SE Plugin - Unregisterd client changed display name. ClientID: {clientID}");
        }
    }

#pragma warning restore IDE1006 // Naming Styles
    #endregion
}
