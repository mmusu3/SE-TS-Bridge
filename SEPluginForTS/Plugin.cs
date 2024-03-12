using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using SharedPluginClasses;

namespace SEPluginForTS;

public class Plugin
{
    static PluginVersion currentVersion = new(2, 0);

    // NOTE: Must be kept in sync with the project settings.
    static ReadOnlySpan<byte> DLLName => "SEPluginForTS"u8;

    static readonly IntPtr PluginNamePtr = AllocHGlobalUTF8("SE-TS Bridge"u8);
    static readonly IntPtr PluginVersionPtr = AllocVersionStringUTF8();
    static readonly IntPtr PluginAuthorPtr = AllocHGlobalUTF8("Remaarn"u8);
    static readonly IntPtr PluginDescriptionPtr = AllocHGlobalUTF8("This plugin integrates with Space Engineers to enable positional audio."u8);
    static readonly IntPtr CommandKeywordPtr = AllocHGlobalUTF8("setsbridge"u8);

    static Plugin instance = new();

    TS3Functions functions;
    unsafe byte* pluginID = null;

    ulong connHandlerId;
    ushort localClientId;
    ulong currentChannelId;

    Vector3 listenerForward = new() { X = 0, Y = 0, Z = -1 };
    Vector3 listenerUp = new() { X = 0, Y = 1, Z = 0 };

    float minDistance = 1.3f;
    float distanceScale = 0.05f;
    float distanceFalloff = 2f;
    float maxDistance = 150f;

    ulong localSteamID = 0;
    bool isInGameSession;

    bool useAntennaConnections = true;
    bool useLocalMuting = false;

    List<string> pendingConsoleMessages = new();
    bool messageDelayComplete;

    string? remoteComputerName;
    NamedPipeClientStream? pipeStream;
    CancellationTokenSource cancellationTokenSource = null!;
    Task runningTask = null!;

    class Client
    {
        public ulong ServerConnectionHandlerID;
        public ushort ClientID;
        public string? ClientName;
        public PluginVersion PluginVersion;
        public ulong SteamID;
        public Vector3 Position;
        public bool IsLocallyMuted;
        public bool InGameSession;
        public bool HasConnection;
        public bool IsWhispering;
    }

    readonly List<Client> clients = new();

    readonly MemoryPool<byte> memPool = MemoryPool<byte>.Shared;

    static unsafe IntPtr AllocHGlobalUTF8(ReadOnlySpan<byte> s)
    {
        void* mem = NativeMemory.Alloc((uint)s.Length + 1);

        fixed (byte* ptr = s)
            Unsafe.CopyBlockUnaligned(mem, ptr, (uint)s.Length);

        ((byte*)mem)[s.Length] = 0;

        return (IntPtr)mem;
    }

    static unsafe IntPtr AllocVersionStringUTF8()
    {
        Span<byte> buffer = stackalloc byte[16];
        bool formatSuccess = Utf8.TryWrite(buffer, $"1.{currentVersion.Minor}.{currentVersion.Patch}", out int bytesWritten);

        if (!formatSuccess)
            return 0;

        buffer[bytesWritten] = 0;

        void* ptr = NativeMemory.Alloc((uint)bytesWritten + 1);

        buffer.Slice(0, bytesWritten).CopyTo(new Span<byte>(ptr, 32));
        ((byte*)ptr)[bytesWritten] = 0;

        return (IntPtr)ptr;
    }

    [Conditional("DEBUG")]
    static void DebugConsole(string text) => Console.WriteLine(text);

    [Conditional("RELEASE")]
    static void ReleaseConsole(string text) => Console.WriteLine(text);

    unsafe void Init()
    {
        connHandlerId = functions.getCurrentServerConnectionHandlerID();

        int connectionStatus = 0;
        var err = (Ts3ErrorType)functions.getConnectionStatus(connHandlerId, &connectionStatus);

        if (err == Ts3ErrorType.ERROR_ok && connectionStatus == 1 && GetLocalClientAndChannelID())
        {
            RefetchTSClients();
            Set3DSettings(distanceScale, 1);
        }

        LoadSettingsFile();
        CreatePipe();

        runningTask = UpdateLoop(cancellationTokenSource.Token);
    }

    void LoadSettingsFile()
    {
        remoteComputerName = "."; // Default to local computer

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsFolderPath = Path.Combine(appDataPath, "TS3Client", "plugins", "SEPluginForTS");
        var settingsFilePath = Path.Combine(settingsFolderPath, "settings.txt");

        if (!File.Exists(settingsFilePath))
            return;

        string? settingsText;

        try
        {
            settingsText = File.ReadAllText(settingsFilePath);
        }
        catch (Exception ex)
        {
            settingsText = null;
            AddOrPrintConsoleMessage($"[SE-TS Bridge] - Failed to read settings file. {ex}");
        }

        if (settingsText != null)
        {
            var parts = settingsText.Split('=', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 && parts[0] == "RemotePCName")
            {
                remoteComputerName = parts[1];
                AddOrPrintConsoleMessage($"[SE-TS Bridge] - Assigning '{remoteComputerName}' as remote PC name.");
            }
            else
            {
                AddOrPrintConsoleMessage("[SE-TS Bridge] - Invalid settings file.");
            }
        }
    }

    void CreatePipe()
    {
        remoteComputerName ??= "."; // Default to local computer

        const PipeDirection direction = PipeDirection.In;

        cancellationTokenSource = new CancellationTokenSource();
        pipeStream = new NamedPipeClientStream(remoteComputerName, "09C842DD-F683-4798-A95F-88B0981265BE", direction, PipeOptions.Asynchronous);
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

        pipeStream?.Dispose();

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

    async ValueTask ConnectPipe(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                pipeStream!.Connect(timeout: 1/*ms*/);
                return;
            }
            catch (TimeoutException) { }
            catch (IOException ioEx) when ((uint)ioEx.HResult == 0x80070035)
            {
                if (remoteComputerName != ".")
                    AddOrPrintConsoleMessage($"[SE-TS Bridge] - Failed to connect to remote computer '{remoteComputerName}'");
                else
                    AddOrPrintConsoleMessage("[SE-TS Bridge] - Failed to connect pipe.");

                pipeStream!.Dispose();
                pipeStream = null;
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
        }
    }

    void AddOrPrintConsoleMessage(string message)
    {
        Console.WriteLine(message);

        lock (pendingConsoleMessages)
        {
            if (messageDelayComplete)
                PrintMessageToCurrentTab(message);
            else
                pendingConsoleMessages.Add(message);
        }
    }

    void PrintPendingConsoleMessages()
    {
        lock (pendingConsoleMessages)
        {
            foreach (var item in pendingConsoleMessages)
                PrintMessageToCurrentTab(item);

            pendingConsoleMessages.Clear();
            messageDelayComplete = true;
        }
    }

    async Task UpdateLoop(CancellationToken cancellationToken)
    {
        // Messages don't show if done while TS is starting up.
        _ = Task.Delay(5000, cancellationToken).ContinueWith(t => PrintPendingConsoleMessages(), cancellationToken);

        waitForPipe:
        while (pipeStream == null)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                return;
            }
        }

        connect:
        await ConnectPipe(cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return;

        if (pipeStream == null)
            goto waitForPipe;

        AddOrPrintConsoleMessage("[SE-TS Bridge] - Established connection to Space Engineers plugin.");

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
                case UpdateResult.WrongVersion:
                    var msg = $"[SE-TS Bridge] - Plugin communication failed. {result.Error}";
                    Console.WriteLine(msg);
                    PrintMessageToCurrentTab(msg);
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

        isInGameSession = false;

        lock (clients)
        {
            foreach (var item in clients)
            {
                item.Position = default;

                if (item.ClientID != 0)
                    SetClientPos(item.ClientID, default);

                UpdateLocalMutingForClient(item);
            }
        }

        SendLocalInfoToCurrentChannel();

        if (cancellationToken.IsCancellationRequested)
            return;

        await pipeStream.DisposeAsync().ConfigureAwait(false);

        PrintMessageToCurrentTab("[SE-TS Bridge] - Closed connection to Space Engineers.");

        CreatePipe();

        goto connect;
    }

    enum UpdateResult
    {
        OK,
        Canceled,
        Closed,
        WrongVersion,
        Corrupt
    }

    async ValueTask<(UpdateResult Result, string? Error)> Update(CancellationToken cancellationToken)
    {
        using var headerBuffer = memPool.Rent(GameUpdatePacket.Size);
        var headerMemory = headerBuffer.Memory;
        int bytes = await pipeStream!.ReadAsync(headerMemory.Slice(0, GameUpdatePacket.Size), cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return (UpdateResult.Canceled, null);

        if (bytes == 0)
            return (UpdateResult.Closed, "Pipe returned zero bytes.");

        if (bytes < GameUpdatePacketHeader.Size)
            return (UpdateResult.Corrupt, $"Expected at least {GameUpdatePacketHeader.Size} bytes, got {bytes}");

        GameUpdatePacket gameUpdate = default;

        if (bytes != GameUpdatePacket.Size)
            gameUpdate.Header = MemoryMarshal.Read<GameUpdatePacketHeader>(headerMemory.Span);
        else
            gameUpdate = MemoryMarshal.Read<GameUpdatePacket>(headerMemory.Span);

        var header = gameUpdate.Header;

        if (header.Version != currentVersion.Packed)
        {
            var headerVersion = new PluginVersion(header.Version);
            var (m, p) = headerVersion.GetVersionNumbers();

            if (headerVersion.IsValid)
                return (UpdateResult.WrongVersion, $"Mismatched TS and SE plugin versions. Expected:{currentVersion.Minor}.{currentVersion.Patch} but got {m}.{p} from SE.");
            else
                return (UpdateResult.Corrupt, "Invalid header version data.");
        }

        bool inSessionDifferent = isInGameSession != header.InSession;
        isInGameSession = header.InSession;

        if (inSessionDifferent)
        {
            UpdateLocalMutingForAllClients();

            if (!isInGameSession)
                ResetAllClientPositions();
        }

        bool steamIdChanged = header.LocalSteamID != localSteamID;

        localSteamID = header.LocalSteamID;

        if (inSessionDifferent || steamIdChanged)
            SendLocalInfoToCurrentChannel();

        if (bytes != GameUpdatePacket.Size)
            return (UpdateResult.Corrupt, $"Expected {GameUpdatePacket.Size} bytes, got {bytes}");

        listenerForward = gameUpdate.Forward;
        listenerUp = gameUpdate.Up;

        // No idea what the deal is with this coord system. Just ignore it and do the transform on the game side.
        //if (localClientId != 0)
        //    SetListener(updatePacket.Forward, updatePacket.Up);

        if (gameUpdate.PlayerCount == 0 && gameUpdate.RemovedPlayerCount == 0 && gameUpdate.NewPlayerCount == 0)
            return (UpdateResult.OK, null);

        int expectedBytes = gameUpdate.PlayerCount * ClientGameState.Size
            + gameUpdate.RemovedPlayerCount * sizeof(ulong)
            + gameUpdate.NewPlayerByteLength;

        using var memBuffer = memPool.Rent(expectedBytes);
        var memory = memBuffer.Memory.Slice(0, expectedBytes);

        bytes = await pipeStream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
            return (UpdateResult.Canceled, null);

        if (bytes != expectedBytes)
            return (UpdateResult.Corrupt, $"Expected {expectedBytes} bytes, got {bytes}");

        if (gameUpdate.PlayerCount != 0)
        {
            ProcessClientStates(memory.Span);
            memory = memory.Slice(gameUpdate.PlayerCount * ClientGameState.Size);
        }

        if (gameUpdate.RemovedPlayerCount != 0)
        {
            ProcessRemovedPlayers(gameUpdate.RemovedPlayerCount, memory.Span);
            memory = memory.Slice(gameUpdate.RemovedPlayerCount * sizeof(ulong));
        }

        if (gameUpdate.NewPlayerCount != 0)
        {
            ProcessNewPlayers(gameUpdate.NewPlayerCount, memory.Span);
            memory = memory.Slice(gameUpdate.NewPlayerByteLength);

            if (memory.Length != 0)
                return (UpdateResult.Corrupt, "Not all bytes were processed.");
        }
        else
        {
            if (gameUpdate.NewPlayerByteLength != 0)
                return (UpdateResult.Corrupt, $"NewPlayerCount was 0 but NewPlayerByteLength was {gameUpdate.NewPlayerByteLength}");
        }

        return (UpdateResult.OK, null);
    }

    void SendLocalInfoToCurrentChannel()
    {
        //Console.WriteLine("[SE-TS Bridge] - SendLocalInfoToCurrentChannel()");
        SendLocalInfoToClientOrChannel(null);
    }

    void SendLocalInfoToClient(Client client)
    {
        //Console.WriteLine($"[SE-TS Bridge] - SendLocalInfoToClient({client.ClientID})");
        SendLocalInfoToClientOrChannel(client);
    }

    unsafe void SendLocalInfoToClientOrChannel(Client? client)
    {
        byte* commandBuffer = stackalloc byte[64];
        var cbSpan = new Span<byte>(commandBuffer, 63);

        bool formatSuccess;
        int bytesWritten;

        if (client == null || client.PluginVersion.Packed == 0 || client.PluginVersion > new PluginVersion(1, 2))
            formatSuccess = Utf8.TryWrite(cbSpan, $"TSSE[{currentVersion.Packed}],GameInfo:{localSteamID}:{(isInGameSession ? 1 : 0)}", out bytesWritten);
        else if (localSteamID != 0)
            formatSuccess = Utf8.TryWrite(cbSpan, $"TSSE,SteamId:{localSteamID}", out bytesWritten);
        else
            return;

        commandBuffer[bytesWritten] = 0;

        if (!formatSuccess)
            Console.WriteLine("[SE-TS Bridge] - Failed to format GameInfo command, insufficient buffer size.");

        if (client != null)
        {
            uint clientIdAndTerm = client.ClientID; // Upper 16 bits is zero terminator

            functions.sendPluginCommand(connHandlerId, pluginID, commandBuffer, PluginTargetMode.PluginCommandTarget_CLIENT, (ushort*)&clientIdAndTerm, null);
        }
        else
        {
            functions.sendPluginCommand(connHandlerId, pluginID, commandBuffer, PluginTargetMode.PluginCommandTarget_CURRENT_CHANNEL, null, null);
        }
    }

    void ProcessClientStates(ReadOnlySpan<byte> bytes)
    {
        var states = MemoryMarshal.Cast<byte, ClientGameState>(bytes);

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

                    client.InGameSession = false;
                    client.Position = default;

                    SetClientPos(client.ClientID, default);
                    UpdateLocalMutingForClient(client);
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

            var pos = Read<Vector3>(ref bytes);
            bool hasConnection = Read<bool>(ref bytes);
            var client = GetClientBySteamId(id);

            if (client != null)
            {
                DebugConsole($"[SE-TS Bridge] - Matching client found for player SteamID: {id}, SteamName: {name}, ClientID: {client.ClientID}");
                ReleaseConsole($"[SE-TS Bridge] - Matching client found for player SteamID. SteamName: {name}, ClientID: {client.ClientID}");

                client.InGameSession = true;
                client.Position = pos;
                client.HasConnection = hasConnection;

                UpdateLocalMutingForClient(client);
            }
            else
            {
                ReleaseConsole($"[SE-TS Bridge] - Missing client for player SteamID. SteamName: {name}");
                DebugConsole($"[SE-TS Bridge] - Missing client for player SteamID: {id}, SteamName: {name}");
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
            ServerConnectionHandlerID = connHandlerId,
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

    void ResetAllClientPositions()
    {
        lock (clients)
        {
            foreach (var item in clients)
                SetClientPos(item.ClientID, default);
        }
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
        int numClients;

        lock (clients)
        {
            int i = 0;
            ushort id;

            while ((id = clientList[i++]) != 0)
            {
                if (id == localClientId)
                    continue;

                var name = GetClientName(id);

                GetLocalMuteStateForClient(connHandlerId, id, out bool muted);

                var client = AddClient(id, name);
                client.IsLocallyMuted = muted;

                numAdded++;
            }

            numClients = clients.Count;
        }

        if (numAdded != 0)
            Console.WriteLine($"[SE-TS Bridge] - Added {numAdded} clients.");

        Console.WriteLine($"[SE-TS Bridge] - There are {numClients} total clients.");

        err = (Ts3ErrorType)functions.freeMemory(clientList);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"[SE-TS Bridge] - Failed to free client list. Error: {err}");
    }

    [Conditional("DEBUG")]
    void CheckClientLocalMuting(Client client)
    {
        if (GetLocalMuteStateForClient(client.ServerConnectionHandlerID, client.ClientID, out bool actualMuted))
        {
            if (client.IsLocallyMuted != actualMuted)
                Console.WriteLine($"[SE-TS Bridge] - ClientID: {client.ClientID} has mismatched local muting state. Cached:{client.IsLocallyMuted}, Actual:{actualMuted}");
        }
    }

    void UpdateLocalMutingForClient(Client client)
    {
        if (!useLocalMuting)
            return;

        CheckClientLocalMuting(client);

        if (isInGameSession != client.InGameSession)
        {
            if (!client.IsLocallyMuted)
            {
                if (MuteClientLocally(client.ServerConnectionHandlerID, client.ClientID))
                {
                    DebugConsole($"[SE-TS Bridge] - UpdateLocalMutingForClient. Name:{client.ClientName}, LocalInSession:{isInGameSession}, client.InSession:{client.InGameSession}, client.IsLocallyMuted was:false now:true");
                    client.IsLocallyMuted = true;
                }
            }
            else
            {
                DebugConsole($"[SE-TS Bridge] - UpdateLocalMutingForClient. Name:{client.ClientName}, LocalInSession:{isInGameSession}, client.InSession:{client.InGameSession}, client.IsLocallyMuted:{client.IsLocallyMuted}");
            }
        }
        else
        {
            if (client.IsLocallyMuted)
            {
                if (UnmuteClientLocally(client.ServerConnectionHandlerID, client.ClientID))
                {
                    DebugConsole($"[SE-TS Bridge] - UpdateLocalMutingForClient. Name:{client.ClientName}, LocalInSession:{isInGameSession}, client.InSession:{client.InGameSession}, client.IsLocallyMuted was:true now:false");
                    client.IsLocallyMuted = false;
                }
            }
            else
            {
                DebugConsole($"[SE-TS Bridge] - UpdateLocalMutingForClient. Name:{client.ClientName}, LocalInSession:{isInGameSession}, client.InSession:{client.InGameSession}, client.IsLocallyMuted:{client.IsLocallyMuted}");
            }
        }
    }

    void UpdateLocalMutingForAllClients()
    {
        if (!useLocalMuting)
            return;

        lock (clients)
        {
            foreach (var item in clients)
                UpdateLocalMutingForClient(item);
        }
    }

    void UnmuteAllClientsLocally()
    {
        if (!useLocalMuting)
            return;

        lock (clients)
        {
            foreach (var item in clients)
            {
                if (item.IsLocallyMuted && UnmuteClientLocally(item.ServerConnectionHandlerID, item.ClientID))
                    item.IsLocallyMuted = false;
            }
        }
    }

    void HandlePluginCommandEvent(ReadOnlySpan<byte> pluginName, ReadOnlySpan<byte> cmd, Client invokerClient)
    {
        if (!cmd.StartsWith("TSSE"u8))
        {
            var pluginNameStr = Encoding.UTF8.GetString(pluginName);
            var cmdStr = Encoding.UTF8.GetString(cmd);

            Console.WriteLine($"[SE-TS Bridge] - Received unknown plugin command, PluginName: {pluginNameStr}, Command: {cmdStr}, InvokerClientID: {invokerClient.ClientID}, InvokerName: {invokerClient.ClientName}");
            return;
        }

        var origCmd = cmd;
        cmd = cmd.Slice("TSSE"u8.Length);

        int splitIndex = cmd.IndexOf("["u8);

        if (splitIndex == -1) // Old protocol has no version
        {
            if (!cmd.StartsWith(",SteamId:"u8))
            {
                InvalidPCE(invokerClient.ClientID, origCmd);
                return;
            }

            cmd = cmd.Slice(",SteamId:"u8.Length);

            if (!ulong.TryParse(cmd, out ulong steamID))
            {
                Console.WriteLine($"[SE-TS Bridge] - Recieved SteamId PCE with invalid SteamID data from ClientID: {invokerClient.ClientID}.");
                return;
            }

            invokerClient.PluginVersion = new PluginVersion(1, 2);
            invokerClient.SteamID = steamID;

            ReleaseConsole($"[SE-TS Bridge] - Recieved SteamID for ClientID: {invokerClient.ClientID}.");
            DebugConsole($"[SE-TS Bridge] - Recieved SteamID for ClientID: {invokerClient.ClientID}. SteamID: {steamID}");

            // invokerClient will not have recieved our info since it could not
            // parse it. Send it again now that its version is known.
            SendLocalInfoToClient(invokerClient);
        }
        else
        {
            cmd = cmd.Slice(splitIndex + "["u8.Length);
            splitIndex = cmd.IndexOf("],"u8);

            if (splitIndex == -1)
            {
                InvalidPCE(invokerClient.ClientID, origCmd);
                return;
            }

            var part = cmd[..splitIndex];

            if (!uint.TryParse(part, out uint version))
            {
                InvalidPCE2(invokerClient.ClientID, origCmd, part);
                return;
            }

            var cmdVersion = new PluginVersion(version);
            var (m, p) = cmdVersion.GetVersionNumbers();

            if (cmdVersion.IsValid)
            {
                invokerClient.PluginVersion = cmdVersion;
            }
            else
            {
                Console.WriteLine($"[SE-TS Bridge] - Recieved PCE from ClientID: {invokerClient.ClientID} with incorrect version: {version}, Minor:{m}, Patch:{p}");
                InvalidPCE(invokerClient.ClientID, origCmd);
                return;
            }

            cmd = cmd.Slice(splitIndex + "],"u8.Length);

            if (!cmd.StartsWith("GameInfo:"u8))
            {
                InvalidPCE(invokerClient.ClientID, origCmd);
                return;
            }

            cmd = cmd.Slice("GameInfo:"u8.Length);
            splitIndex = cmd.IndexOf(":"u8);

            if (splitIndex == -1)
            {
                Console.WriteLine($"[SE-TS Bridge] - Recieved invalid GameInfo PCE from ClientID: {invokerClient.ClientID}");
                return;
            }

            part = cmd[..splitIndex];

            if (!ulong.TryParse(part, out ulong steamID))
            {
                Console.WriteLine($"[SE-TS Bridge] - Recieved GameInfo PCE with invalid SteamID data from ClientID: {invokerClient.ClientID}.");
                return;
            }

            part = cmd.Slice(splitIndex + ":"u8.Length);

            if (!int.TryParse(part, out int inGameSession))
            {
                Console.WriteLine($"[SE-TS Bridge] - Recieved GameInfo PCE with invalid InGameSession data from ClientID: {invokerClient.ClientID}.");
                return;
            }

            invokerClient.SteamID = steamID;
            invokerClient.InGameSession = inGameSession != 0;

            ReleaseConsole($"[SE-TS Bridge] - Recieved GameInfo for ClientID: {invokerClient.ClientID}. InGameSession: {invokerClient.InGameSession}");
            DebugConsole($"[SE-TS Bridge] - Recieved GameInfo for ClientID: {invokerClient.ClientID}. SteamID: {steamID}, InGameSession: {invokerClient.InGameSession}");

            UpdateLocalMutingForClient(invokerClient);
        }

        static void InvalidPCE(ushort clientID, ReadOnlySpan<byte> cmd)
        {
            var cmdStr = Encoding.UTF8.GetString(cmd);
            Console.WriteLine($"[SE-TS Bridge] - Recieved invalid PCE from ClientID: {clientID}, Cmd: {cmdStr}");
        }

        static void InvalidPCE2(ushort clientID, ReadOnlySpan<byte> cmd, ReadOnlySpan<byte> part)
        {
            var cmdStr = Encoding.UTF8.GetString(cmd);
            var partStr = Encoding.UTF8.GetString(part);

            Console.WriteLine($"[SE-TS Bridge] - Recieved invalid PCE from ClientID: {clientID}, Cmd: {cmdStr}, Part: {partStr}");
        }
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

    unsafe void SetListener(Vector3 forward, Vector3 up)
    {
        //Console.WriteLine($"[SE-TS Bridge] - Setting listener attribs. Forward: {forward}, Up: {up}.");

        Vector3 zeroPos = default;

        var err = (Ts3ErrorType)functions.systemset3DListenerAttributes(connHandlerId, (TS3_VECTOR*)&zeroPos, (TS3_VECTOR*)&forward, (TS3_VECTOR*)&up);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"[SE-TS Bridge] - Failed to set listener attribs. Error: {err}");
    }

    unsafe void SetClientPos(ushort clientId, Vector3 position)
    {
        if (clientId == 0)
        {
            Console.WriteLine($"[SE-TS Bridge] - Tried to set position of client ID 0.");
            return;
        }

        //Console.WriteLine($"[SE-TS Bridge] - Setting position of client {clientId} to {position}.");

        position.Z = -position.Z;

        var err = (Ts3ErrorType)functions.channelset3DAttributes(connHandlerId, clientId, (TS3_VECTOR*)&position);

        if (err != Ts3ErrorType.ERROR_ok)
            Console.WriteLine($"[SE-TS Bridge] - Failed to set client pos to {position}. Error: {err}");
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

    unsafe bool GetLocalMuteStateForClient(ulong serverConnectionHandlerID, ushort clientID, out bool muted)
    {
        int mutedState;
        var err = (Ts3ErrorType)functions.getClientVariableAsInt(serverConnectionHandlerID, clientID, (int)ClientProperties.CLIENT_IS_MUTED, &mutedState);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            muted = false;
            Console.WriteLine($"[SE-TS Bridge] - Failed to get client local muting state. ClientID: {clientID}, Error: {err}");
            return false;
        }

        muted = mutedState != 0;
        return true;
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

    unsafe bool MuteClientLocally(ulong serverConnectionHandlerID, ushort clientID)
    {
        ushort* clientArray = stackalloc ushort[2] { clientID, 0 };

        var err = (Ts3ErrorType)functions.requestMuteClients(serverConnectionHandlerID, clientArray, null);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            Console.WriteLine($"[SE-TS Bridge] - Failed to mute client locally. ClientID: {clientID}, Error: {err}");
            return false;
        }

        return true;
    }

    unsafe bool UnmuteClientLocally(ulong serverConnectionHandlerID, ushort clientID)
    {
        ushort* clientArray = stackalloc ushort[2] { clientID, 0 };

        var err = (Ts3ErrorType)functions.requestUnmuteClients(serverConnectionHandlerID, clientArray, null);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            Console.WriteLine($"[SE-TS Bridge] - Failed to unmute client locally. ClientID: {clientID}, Error: {err}");
            return false;
        }

        return true;
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

    #endregion

    int ProcessCommand(string cmd)
    {
        int spaceIndex = cmd.IndexOf(' ');

        if (spaceIndex < 0)
        {
            PrintMessageToCurrentTab($"Invalid command {cmd}");
            return 0;
        }

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
        case "maxdistance":
            {
                if (float.TryParse(cmd.AsSpan(spaceIndex).Trim(), out float value))
                {
                    maxDistance = value;
                    PrintMessageToCurrentTab($"Setting max distance to {value}");
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
                UpdateLocalMutingForAllClients();
                SendLocalInfoToCurrentChannel();
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

            UpdateLocalMutingForClient(client);
            SendLocalInfoToClient(client);
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

            if (useLocalMuting)
                UnmuteClientLocally(serverConnectionHandlerID, clientID);
        }
        else
        {
            // A client either moved between channels that aren't the current channnel or they left the server.
        }
    }

    // Docs https://teamspeakdocs.github.io/PluginAPI/client_html/index.html

    #region Required functions
#pragma warning disable IDE1006 // Naming Styles

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_name")] public unsafe static byte* ts3plugin_name() => (byte*)PluginNamePtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_version")] public unsafe static byte* ts3plugin_version() => (byte*)PluginVersionPtr;
    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_apiVersion")] public unsafe static int ts3plugin_apiVersion() => 26;
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

        Console.WriteLine("[SE-TS Bridge] - Registered plugin ID: " + Encoding.UTF8.GetString(charSpan));
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
                instance.UpdateLocalMutingForAllClients();
                instance.SendLocalInfoToCurrentChannel();
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

        float minD = instance.minDistance;
        float scale = instance.distanceScale;
        float dist = Vector3.Distance(default, client.Position);
        float vol = float.Min(1f, 1f / float.Pow(dist * scale + 1 - minD * scale, instance.distanceFalloff));

        float limit = dist / instance.maxDistance;
        limit = 1 - limit * limit;

        vol *= limit;
        vol = float.Max(0, vol);

        *volume = vol;
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

        var nameUtf8 = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pluginName);

        if (!nameUtf8.SequenceEqual(DLLName))
            return;

        var client = instance.GetClientByClientId(invokerClientID);
        var invokerNameStr = Marshal.PtrToStringUTF8((nint)invokerName);

        if (client == null)
        {
            Console.WriteLine($"[SE-TS Bridge] - Received plugin command from unregistered client, ClientID: {invokerClientID}, InvokerName: {invokerNameStr}");
            return;
        }

        var cmd = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pluginCommand);

        if (cmd != null)
            instance.HandlePluginCommandEvent(nameUtf8, cmd, client);
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
