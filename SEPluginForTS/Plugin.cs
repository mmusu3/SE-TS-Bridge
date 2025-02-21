using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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
    static PluginVersion currentVersion = new(4, 0);

    // NOTE: Must be kept in sync with the project settings.
    static ReadOnlySpan<byte> DLLName => "SEPluginForTS"u8;

    static readonly IntPtr PluginNamePtr = AllocHGlobalUTF8("SE-TS Bridge"u8);
    static readonly IntPtr PluginVersionPtr = AllocVersionStringUTF8();
    static readonly IntPtr PluginAuthorPtr = AllocHGlobalUTF8("Remaarn"u8);
    static readonly IntPtr PluginDescriptionPtr = AllocHGlobalUTF8("This plugin enables the use of TeamSpeak's positional audio feature with the game Space Engineers."u8);
    static readonly IntPtr CommandKeywordPtr = AllocHGlobalUTF8("setsbridge"u8);

    static Plugin instance = new();

    TS3Functions functions;
    unsafe byte* pluginID = null;

    ulong currentServerId;
    ushort localClientId;
    Dictionary<ulong, Channel> serverChannels = [];
    Channel? currentChannel;

    Vector3 listenerForward = new(0, 0, -1);
    Vector3 listenerUp = new(0, 1, 0);

    float minDistance = 1.3f;
    float distanceScale = 0.05f;
    float distanceFalloff = 2f;
    float maxDistance = 150f;
    float extendRangeFactor = 2f;

    ulong localSteamID = 0;
    bool isInGameSession;
    bool forceIngame = false;

    bool useAntennaConnections = true;

    List<string> pendingConsoleMessages = [];
    bool messageDelayComplete;

    string? remoteComputerName;
    NamedPipeClientStream? pipeStream;
    CancellationTokenSource cancellationTokenSource = null!;
    Task runningTask = null!;

    class Channel
    {
        public required ulong ServerID;
        public required ulong ID;
        public required string Name = null!;

        public string? Topic => topic;
        string? topic;

        public Channel? Parent;
        public List<Channel> Children = [];
        public bool IsSubscribed;
        public int ClientCount;
        public bool IsIngame;

        public void SetTopic(string? topic)
        {
            this.topic = topic;
            IsIngame = topic == "sets-ingame";
        }

        public IEnumerable<Channel> Descendants
        {
            get
            {
                foreach (var child in Children)
                {
                    yield return child;

                    foreach (var item in child.Descendants)
                        yield return item;
                }
            }
        }

        public bool HasAncestor(Channel channel)
        {
            var p = Parent;

            while (p != null)
            {
                if (p == channel)
                    return true;

                p = p.Parent;
            }

            return false;
        }
    }

    [Flags]
    enum ClientStateFlags
    {
        None = 0,
        LocallyMuted = 1,
        InGameSession = 2,
        HasConnection = 4,
        ExtendRange = 8,
        Whispering = 16
    }

    class Client
    {
        public ushort ClientID;
        public string? ClientName;
        public Channel? Channel;
        public PluginVersion PluginVersion;
        public ulong SteamID;
        public Vector3 Position;
        public ClientStateFlags Flags;

        public ulong ServerID => Channel?.ServerID ?? 0;

        public bool IsLocallyMuted
        {
            get => (Flags & ClientStateFlags.LocallyMuted) != 0;
            set => SetFlag(value, ClientStateFlags.LocallyMuted);
        }

        public bool InGameSession
        {
            get => (Flags & ClientStateFlags.InGameSession) != 0;
            set => SetFlag(value, ClientStateFlags.InGameSession);
        }

        public bool HasConnection
        {
            get => (Flags & ClientStateFlags.HasConnection) != 0;
            set => SetFlag(value, ClientStateFlags.HasConnection);
        }

        public bool ExtendRange
        {
            get => (Flags & ClientStateFlags.ExtendRange) != 0;
            set => SetFlag(value, ClientStateFlags.ExtendRange);
        }

        public bool IsWhispering
        {
            get => (Flags & ClientStateFlags.Whispering) != 0;
            set => SetFlag(value, ClientStateFlags.Whispering);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetFlag(bool value, ClientStateFlags flag)
        {
            if (value)
                Flags |= flag;
            else
                Flags &= ~flag;
        }
    }

    readonly List<Client> clients = [];

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

    static void WriteConsole(string text) => Console.WriteLine("[SE-TS Bridge] - " + text);

    [Conditional("DEBUG")]
    static void DebugConsole(string text) => Console.WriteLine("[SE-TS Bridge] - " + text);

    [Conditional("RELEASE")]
    static void ReleaseConsole(string text) => Console.WriteLine("[SE-TS Bridge] - " + text);

    void WriteConsoleAndLog(LogLevel logLevel, string text)
    {
        Console.WriteLine("[SE-TS Bridge] - " + text);
        LogMessage(text, logLevel, "SE-TS Bridge");
    }

    unsafe void Init()
    {
        AddOrPrintConsoleMessage("Initializing.");

        currentServerId = functions.getCurrentServerConnectionHandlerID();

        int connectionStatus = 0;
        var err = (Ts3ErrorType)functions.getConnectionStatus(currentServerId, &connectionStatus);

        if (err == Ts3ErrorType.ERROR_ok && connectionStatus == 1 && UpdateLocalClientIdAndChannel(currentServerId))
        {
            Set3DSettings(distanceScale, 1);
            RefetchTSClients();

            if (instance.currentChannel is { IsIngame: true })
                instance.SendLocalInfoToCurrentChannel();
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
            AddOrPrintConsoleMessage($"Failed to read settings file. {ex}");
        }

        if (settingsText != null)
        {
            var parts = settingsText.Split('=', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 && parts[0] == "RemotePCName")
            {
                remoteComputerName = parts[1];
                AddOrPrintConsoleMessage($"Assigning '{remoteComputerName}' as remote PC name.");
            }
            else
            {
                AddOrPrintConsoleMessage("Invalid settings file.");
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
        WriteConsoleAndLog(LogLevel.LogLevel_INFO, "Disposing.");

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

        lock (serverChannels)
            serverChannels.Clear();

        currentServerId = 0;
        localClientId = 0;
        currentChannel = null;

        WriteConsoleAndLog(LogLevel.LogLevel_INFO, "Disposed.");
    }

    async ValueTask ConnectPipe(CancellationToken cancellationToken)
    {
        bool printWaitMessage = true;

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
                    AddOrPrintConsoleMessage($"Failed to connect to remote computer '{remoteComputerName}'");
                else
                    AddOrPrintConsoleMessage("Failed to connect pipe.");

                pipeStream!.Dispose();
                pipeStream = null;
                return;
            }

            if (printWaitMessage)
            {
                AddOrPrintConsoleMessage("Waiting for game connection.");
                printWaitMessage = false;
            }

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
        }
    }

    void AddOrPrintConsoleMessage(string message)
    {
        message = "[SE-TS Bridge] - " + message;

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

        AddOrPrintConsoleMessage("Established connection to Space Engineers plugin.");

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
                    WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Update was canceled.");
                    break;
                case UpdateResult.Closed:
                    WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Connection was closed. {result.Error}");
                    break;
                case UpdateResult.WrongVersion:
                    var msg = $"Plugin communication failed. {result.Error}";
                    WriteConsoleAndLog(LogLevel.LogLevel_WARNING, msg);
                    PrintMessageToCurrentTab("[SE-TS Bridge] - " + msg);
                    break;
                case UpdateResult.Corrupt:
                    WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Update failed with result {result.Result}. {result.Error}");
                    break;
                }

                break;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Update was canceled.");
                    break;
                }
                else
                {
                    WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Exception while updating {ex}");
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
            }
        }

        if (currentChannel is { IsIngame: true })
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

        if (inSessionDifferent && !isInGameSession)
            ResetAllClientPositions();

        bool steamIdChanged = header.LocalSteamID != localSteamID;

        localSteamID = header.LocalSteamID;

        if (currentChannel is { IsIngame: true } && (inSessionDifferent || steamIdChanged))
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
            int numBytes = gameUpdate.PlayerCount * ClientGameState.Size;
            ProcessClientStates(gameUpdate.PlayerCount, memory.Span.Slice(0, numBytes));
            memory = memory.Slice(numBytes);
        }

        if (gameUpdate.RemovedPlayerCount != 0)
        {
            int numBytes = gameUpdate.RemovedPlayerCount * sizeof(ulong);
            ProcessRemovedPlayers(gameUpdate.RemovedPlayerCount, memory.Span.Slice(0, numBytes));
            memory = memory.Slice(numBytes);
        }

        if (gameUpdate.NewPlayerCount != 0)
        {
            ProcessNewPlayers(gameUpdate.NewPlayerCount, memory.Span.Slice(0, gameUpdate.NewPlayerByteLength));
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

    bool WriteLocalInfoCommand(Span<byte> commandBuffer)
    {
        int bytesWritten;
        bool formatSuccess = Utf8.TryWrite(commandBuffer[0..^1], $"TSSE[{currentVersion.Packed}],GameInfo:{localSteamID}:{((isInGameSession || forceIngame) ? 1 : 0)}", out bytesWritten);

        if (!formatSuccess)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, "Failed to format GameInfo command, insufficient buffer size.");
            return false;
        }

        commandBuffer[bytesWritten] = 0;
        return true;
    }

    bool WriteLegacyLocalInfoCommand(Span<byte> commandBuffer)
    {
        int bytesWritten;
        bool formatSuccess = Utf8.TryWrite(commandBuffer[0..^1], $"TSSE,SteamId:{localSteamID}", out bytesWritten);

        if (!formatSuccess)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, "Failed to format GameInfo command, insufficient buffer size.");
            return false;
        }

        commandBuffer[bytesWritten] = 0;
        return true;
    }

    void SendLocalInfoToCurrentChannel()
    {
        //Console.WriteLine("[SE-TS Bridge] - SendLocalInfoToCurrentChannel()");
        SendLocalInfoToClientOrCurrentChannel(null);
    }

    void SendLocalInfoToClient(Client client)
    {
        //Console.WriteLine($"[SE-TS Bridge] - SendLocalInfoToClient({client.ClientID})");
        SendLocalInfoToClientOrCurrentChannel(client);
    }

    unsafe void SendLocalInfoToClientOrCurrentChannel(Client? client)
    {
        const int commandBufferSize = 64;
        byte* commandBuffer = stackalloc byte[commandBufferSize];
        var cbSpan = new Span<byte>(commandBuffer, commandBufferSize);

        if (client == null || client.PluginVersion.Packed == 0 || client.PluginVersion > new PluginVersion(1, 2))
        {
            WriteLocalInfoCommand(cbSpan);
        }
        else if (localSteamID != 0)
        {
            WriteLegacyLocalInfoCommand(cbSpan);
        }
        else
        {
            return;
        }

        if (client != null)
        {
            uint clientIdAndTerm = client.ClientID; // Upper 16 bits is zero terminator

            functions.sendPluginCommand(client.ServerID, pluginID, commandBuffer, PluginTargetMode.PluginCommandTarget_CLIENT, (ushort*)&clientIdAndTerm, null);
        }
        else
        {
            functions.sendPluginCommand(currentServerId, pluginID, commandBuffer, PluginTargetMode.PluginCommandTarget_CURRENT_CHANNEL, null, null);
        }
    }

    void ProcessClientStates(int numPlayers, ReadOnlySpan<byte> bytes)
    {
        var numStates = bytes.Length / ClientGameState.Size;

        if (numStates < numPlayers)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Error processing client states. Expected {numPlayers} states but only had {numStates}.");
            return;
        }

        //WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Processing {states.Length} client states.");

        for (int i = 0; i < numPlayers; i++)
        {
            var state = MemoryMarshal.Read<ClientGameState>(bytes.Slice(i * ClientGameState.Size));
            var client = GetClientBySteamId(state.SteamID);

            if (client != null)
            {
                client.Position = state.Position;
                client.HasConnection = (state.Flags & PlayerStateFlags.HasConnection) != 0;
                client.ExtendRange = (state.Flags & PlayerStateFlags.InCockpit) != 0;

                if (localClientId != 0)
                    UpdateClientPosition(client);
            }
            //else
            //{
            //    WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Missing game client for SteamID: {state.SteamID}");
            //}
        }
    }

    void ProcessRemovedPlayers(int numRemovedPlayers, ReadOnlySpan<byte> bytes)
    {
        WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Removing {numRemovedPlayers} players.");

        lock (clients)
        {
            for (int i = 0; i < numRemovedPlayers; i++)
            {
                ulong steamId = Read<ulong>(ref bytes);
                var client = GetClientBySteamId(steamId);

                if (client != null && client.ClientID != 0)
                {
                    //WriteConsole($"Resetting client position for ID: {client.ClientID}.");

                    client.InGameSession = false;
                    client.Position = default;

                    SetClientPos(client.ClientID, default);
                }
            }
        }
    }

    void ProcessNewPlayers(int numNewPlayers, ReadOnlySpan<byte> bytes)
    {
        WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Received {numNewPlayers} new players.");

        for (int i = 0; i < numNewPlayers; i++)
        {
            ulong id = Read<ulong>(ref bytes);
            int nameLength = Read<int>(ref bytes);
            var name = new string(MemoryMarshal.Cast<byte, char>(bytes).Slice(0, nameLength));

            bytes = bytes.Slice(nameLength * sizeof(char));

            var pos = Read<Vector3>(ref bytes);
            var flags = (PlayerStateFlags)Read<int>(ref bytes);
            var client = GetClientBySteamId(id);

            if (client != null)
            {
#if DEBUG
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Matching client found for player SteamID: {id}, SteamName: {name}, ClientID: {client.ClientID}");
#else
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Matching client found for player SteamID. SteamName: {name}, ClientID: {client.ClientID}");
#endif

                client.Position = pos;
                client.InGameSession = true;
                client.HasConnection = (flags & PlayerStateFlags.HasConnection) != 0;
                client.ExtendRange = (flags & PlayerStateFlags.InCockpit) != 0;
            }
            else
            {
#if DEBUG
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Missing client for player SteamID: {id}, SteamName: {name}");
#else
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Missing client for player SteamID. SteamName: {name}");
#endif
            }
        }
    }

    unsafe static T Read<T>(ref ReadOnlySpan<byte> span) where T : unmanaged
    {
        var value = MemoryMarshal.Read<T>(span);
        span = span.Slice(sizeof(T));
        return value;
    }

    unsafe static void Write<T>(ref Span<byte> span, in T value) where T : unmanaged
    {
        MemoryMarshal.Write(span, in value);
        span = span.Slice(sizeof(T));
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

    Client AddClientThreadSafe(ushort id, Channel channel, string? name)
    {
        lock (clients)
            return AddClient(id, channel, name);
    }

    Client AddClient(ushort id, Channel channel, string? name)
    {
        var client = new Client {
            ClientID = id,
            ClientName = name,
            Channel = channel
        };

        clients.Add(client);

        channel.ClientCount++;

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

        if (client.Channel != null)
            client.Channel.ClientCount--;

        bool removed = clients.Remove(client);

        if (!removed)
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to unregister client. ClientId: {client.ClientID}");
    }

    void RemoveAllClients()
    {
        lock (clients)
        {
            if (clients.Count == 0)
            {
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Zero clients to remove.");
                return;
            }

            WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Removing all {clients.Count} clients.");

            foreach (var client in clients)
            {
                if (client.Channel != null)
                    client.Channel.ClientCount--;
            }

            clients.Clear();
            // TODO: Does this need to SetClientPos?
        }
    }

    void UpdateClientPosition(Client client)
    {
        bool noPosition = currentChannel is not { IsIngame: true } || (client.IsWhispering && (client.HasConnection || !useAntennaConnections));

        SetClientPos(client.ClientID, noPosition ? default : client.Position);
    }

    void ResetAllClientPositions()
    {
        lock (clients)
        {
            foreach (var item in clients)
                SetClientPos(item.ClientID, default);
        }
    }

    void RefetchTSClients()
    {
        WriteConsoleAndLog(LogLevel.LogLevel_INFO, "Refetching client list.");

        RemoveAllClients();

        if (currentChannel == null)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, "Failed to refetch client list. Current channel is null.");
            return;
        }

        lock (serverChannels)
        {
            foreach (var channel in serverChannels.Values)
            {
                if (channel.IsSubscribed)
                    AddClientsFromChannel(channel);
            }
        }

        WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"There are {clients.Count} registered clients.");
    }

    unsafe void AddClientsFromChannel(Channel channel)
    {
        WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Adding clients from channel {channel.ID} '{channel.Name}'.");

        ushort* clientList;
        var err = (Ts3ErrorType)functions.getChannelClientList(channel.ServerID, channel.ID, &clientList);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get client list for channel {channel.ID}. Error: {err}");
            return;
        }

        int numAdded = 0;

        lock (clients)
        {
            ushort id;

            for (int i = 0; (id = clientList[i]) != 0; i++)
            {
                if (id == localClientId)
                    continue;

                var name = GetClientName(channel.ServerID, id);
                var client = AddClient(id, channel, name);

                GetLocalMuteStateForClient(channel.ServerID, id, out bool muted);
                client.IsLocallyMuted = muted;

                numAdded++;
            }
        }

        err = (Ts3ErrorType)functions.freeMemory(clientList);

        if (err != Ts3ErrorType.ERROR_ok)
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to free client list pointer. Error: {err}");

        if (numAdded != 0)
            WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Added {numAdded} clients.");
    }

    void HandlePluginCommandEvent(ReadOnlySpan<byte> pluginName, ReadOnlySpan<byte> cmd, Client invokerClient)
    {
        if (!cmd.StartsWith("TSSE"u8))
        {
            var pluginNameStr = Encoding.UTF8.GetString(pluginName);
            var cmdStr = Encoding.UTF8.GetString(cmd);

            WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Received unknown plugin command, PluginName: {pluginNameStr}, Command: {cmdStr}, InvokerClientID: {invokerClient.ClientID}, InvokerName: {invokerClient.ClientName}");
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
                WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Recieved SteamId PCE with invalid SteamID data from ClientID: {invokerClient.ClientID}.");
                return;
            }

            invokerClient.PluginVersion = new PluginVersion(1, 2);
            invokerClient.SteamID = steamID;

            ReleaseConsole($"Recieved SteamID for ClientID: {invokerClient.ClientID}.");
            DebugConsole($"Recieved SteamID for ClientID: {invokerClient.ClientID}. SteamID: {steamID}");

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

            if (!uint.TryParse(part, out uint versionPart))
            {
                InvalidPCE2(invokerClient.ClientID, origCmd, part);
                return;
            }

            var version = new PluginVersion(versionPart);
            var (m, p) = version.GetVersionNumbers();

            if (version.IsValid)
            {
                invokerClient.PluginVersion = version;
            }
            else
            {
                WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Recieved PCE from ClientID: {invokerClient.ClientID} with incorrect version: {versionPart}, Minor:{m}, Patch:{p}");
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
                WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Recieved invalid GameInfo PCE from ClientID: {invokerClient.ClientID}");
                return;
            }

            part = cmd[..splitIndex];

            if (!ulong.TryParse(part, out ulong steamID))
            {
                WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Recieved GameInfo PCE with invalid SteamID data from ClientID: {invokerClient.ClientID}.");
                return;
            }

            part = cmd.Slice(splitIndex + ":"u8.Length);

            if (!int.TryParse(part, out int inGameSession))
            {
                WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Recieved GameInfo PCE with invalid InGameSession data from ClientID: {invokerClient.ClientID}.");
                return;
            }

            invokerClient.SteamID = steamID;
            invokerClient.InGameSession = inGameSession != 0;

            ReleaseConsole($"Recieved GameInfo for ClientID: {invokerClient.ClientID}. Version: {version}, InGameSession: {invokerClient.InGameSession}");
            DebugConsole($"Recieved GameInfo for ClientID: {invokerClient.ClientID}. SteamID: {steamID}, Version: {version}, InGameSession: {invokerClient.InGameSession}");
        }

        void InvalidPCE(ushort clientID, ReadOnlySpan<byte> cmd)
        {
            var cmdStr = Encoding.UTF8.GetString(cmd);
            WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Recieved invalid PCE from ClientID: {clientID}, Cmd: {cmdStr}");
        }

        void InvalidPCE2(ushort clientID, ReadOnlySpan<byte> cmd, ReadOnlySpan<byte> part)
        {
            var cmdStr = Encoding.UTF8.GetString(cmd);
            var partStr = Encoding.UTF8.GetString(part);

            WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Recieved invalid PCE from ClientID: {clientID}, Cmd: {cmdStr}, Part: {partStr}");
        }
    }

    int ProcessCommand(string cmd)
    {
        int spaceIndex = cmd.IndexOf(' ');

        if (spaceIndex < 0)
        {
            switch (cmd.ToLowerInvariant())
            {
#if DEBUG
            case "toggleforceingame":
                {
                    forceIngame = !forceIngame;
                    PrintMessageToCurrentTab($"Setting force ingame status to {forceIngame}");
                    SendLocalInfoToCurrentChannel();
                    break;
                }
#endif
            default:
                PrintMessageToCurrentTab($"Invalid command {cmd}");
                break;
            }

            return 0;
        }

        var argSpan = cmd.AsSpan(spaceIndex).Trim();

        switch (cmd.Substring(0, spaceIndex).ToLowerInvariant())
        {
        case "distancescale":
            {
                if (TryParseArgValue(argSpan, out float value))
                {
                    distanceScale = value;

                    if (Set3DSettings(distanceScale, 1))
                        PrintMessageToCurrentTab($"Setting distance scale to {value}");
                    else
                        PrintMessageToCurrentTab($"Error, failed to set distance scale value.");
                }
                break;
            }
        case "distancefalloff":
            {
                if (TryParseArgValue(argSpan, out float value))
                {
                    distanceFalloff = value;
                    PrintMessageToCurrentTab($"Setting distance falloff to {value}");
                }
                break;
            }
        case "maxdistance":
            {
                if (TryParseArgValue(argSpan, out float value))
                {
                    maxDistance = value;
                    PrintMessageToCurrentTab($"Setting max distance to {value}");
                }
                break;
            }
        case "useantennas":
            {
                if (TryParseArgValue(argSpan, out bool value))
                {
                    useAntennaConnections = value;
                    PrintMessageToCurrentTab($"Setting use antennas to {value}");
                }
                break;
            }
        default:
            PrintMessageToCurrentTab($"Invalid command {cmd}");
            break;
        }

        return 0;

        bool TryParseArgValue<T>(ReadOnlySpan<char> arg, [NotNullWhen(true)] out T? value)
            where T : ISpanParsable<T>
        {
            if (T.TryParse(arg, null, out value))
                return true;

            PrintMessageToCurrentTab($"Error, failed to parse value.");
            return false;
        }
    }

    [MemberNotNullWhen(true, nameof(currentChannel))]
    bool UpdateLocalClientIdAndChannel(ulong serverId)
    {
        if (!GetLocalClientAndChannelIds(serverId, out localClientId, out ulong channelId))
            return false;

        lock (serverChannels)
        {
            if (!serverChannels.TryGetValue(channelId, out currentChannel))
            {
                WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to find current channel with ID {channelId}.");
                return false;
            }
        }

        return true;
    }

    void OnServerDisconnected()
    {
        localClientId = 0;
        RemoveAllClients();
        currentChannel = null;

        lock (serverChannels)
            serverChannels.Clear();
    }

    void OnNewChannel(ulong serverId, ulong channelId, ulong channelParentId)
    {
        if (serverId != currentServerId)
            return;

        lock (serverChannels)
            RegisterChannel(serverId, channelId, channelParentId);
    }

    unsafe void OnNewChannelCreated(ulong serverId, ulong channelId, ulong channelParentId,
        ushort invokerId, byte* invokerName, byte* invokerUniqueIdentifier)
    {
        if (serverId != currentServerId)
            return;

        lock (serverChannels)
            RegisterChannel(serverId, channelId, channelParentId);
    }

    Channel? RegisterChannel(ulong serverId, ulong channelId, ulong channelParentId)
    {
        string? name;

        if (!GetChannelName(serverId, channelId, out name))
            return null;

        string? topic;

        if (!GetChannelTopic(serverId, channelId, out topic))
            return null;

        if (GetChannelIsSubscribed(serverId, channelId) is not { } isSubscribed)
            return null;

        Channel? parent = null;

        if (channelParentId != 0 && !serverChannels.TryGetValue(channelParentId, out parent))
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to find parent channel {channelParentId}.");

        var channel = new Channel {
            ServerID = serverId,
            ID = channelId,
            Name = name,
            Parent = parent,
            IsSubscribed = isSubscribed
        };

        channel.SetTopic(topic);

        serverChannels.Add(channelId, channel);
        parent?.Children.Add(channel);

        return channel;
    }

    unsafe void OnChannelDeleted(ulong serverId, ulong channelId,
        ushort invokerId, byte* invokerName, byte* invokerUniqueIdentifier)
    {
        if (serverId != currentServerId)
            return;

        lock (serverChannels)
        {
            if (!serverChannels.Remove(channelId, out var channel))
            {
                WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Channel {channelId} was missing when removing.");
                return;
            }

            channel.Parent?.Children.Remove(channel);
        }
    }

    unsafe void OnChannelMoved(ulong serverId, ulong channelId, ulong newChannelParentId,
        ushort invokerId, byte* invokerName, byte* invokerUniqueIdentifier)
    {
        if (serverId != currentServerId)
            return;

        lock (serverChannels)
        {
            if (!serverChannels.TryGetValue(channelId, out var channel))
            {
                WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Channel {channelId} was missing when removing.");
                return;
            }

            var oldParent = channel.Parent;

            oldParent?.Children.Remove(channel);
            channel.Parent = null;

            if (!serverChannels.TryGetValue(newChannelParentId, out var newParent))
            {
                WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"New parent channel {newChannelParentId} of channel {channelId} was missing.");
                return;
            }

            channel.Parent = newParent;
            newParent.Children.Add(channel);
        }
    }

    unsafe void OnChannelEdited(ulong serverId, ulong channelId,
        ushort invokerId, byte* invokerName, byte* invokerUniqueIdentifier)
    {
        if (serverId != currentServerId)
            return;

        Channel? channel;

        lock (serverChannels)
        {
            if (!serverChannels.TryGetValue(channelId, out channel))
                return;
        }

        var wasIngame = channel.IsIngame;

        channel.SetTopic(GetChannelTopic(serverId, channelId));

        if (channel == currentChannel && channel.IsIngame && !wasIngame)
            SendLocalInfoToCurrentChannel();
    }

    void OnLocalChannelChanged(ulong oldChannelId)
    {
        if (oldChannelId != 0)
        {
            Channel? oldChannel;

            lock (serverChannels)
            {
                serverChannels.TryGetValue(oldChannelId, out oldChannel);

                if (oldChannel != null && !DoesChannelExist(oldChannel.ServerID, oldChannel.ID))
                    serverChannels.Remove(oldChannel.ID); // May have moved out of a temporary channel
            }

            if (oldChannel == null)
                WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to find old channel {oldChannelId}");
        }

        if (currentChannel != null)
        {
            if (!currentChannel.IsSubscribed)
                RefetchTSClients();

            if (currentChannel.IsIngame)
                SendLocalInfoToCurrentChannel();
        }
    }

    void OnChannelSubscribed(ulong serverId, ulong channelId)
    {
        if (serverId != currentServerId)
            return;

        Channel? channel;
        bool wasSubscribed = false;

        lock (serverChannels)
        {
            if (serverChannels.TryGetValue(channelId, out channel))
            {
                wasSubscribed = channel.IsSubscribed;
                channel.IsSubscribed = true;
            }
        }

        if (channel != null && channel.IsSubscribed != wasSubscribed)
            AddClientsFromChannel(channel);
    }

    void OnChannelSubscribeFinished(ulong serverId, ulong channelId)
    {
    }

    void OnChannelUnsubscribed(ulong serverId, ulong channelId)
    {
        if (serverId != currentServerId)
            return;

        lock (serverChannels)
        {
            if (serverChannels.TryGetValue(channelId, out var channel))
                channel.IsSubscribed = false;
        }
    }

    void OnChannelUnsubscribeFinished(ulong serverId, ulong channelId)
    {
    }

    void OnClientMoveSubscriptionEvent(ulong serverId, ushort clientId,
        ulong oldChannelId, ulong newChannelId, Visibility visibility)
    {
    }

    void HandleClientMoved(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID)
    {
        if (serverConnectionHandlerID != currentServerId)
            return;

        Channel? oldChannel, newChannel;

        lock (serverChannels)
        {
            serverChannels.TryGetValue(oldChannelID, out oldChannel);
            serverChannels.TryGetValue(newChannelID, out newChannel);
        }

        if (clientID == localClientId)
        {
            currentChannel = newChannel;

            WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Current channel changed. NewChannelID: {newChannelID}");

            if (newChannelID != 0)
            {
                if (currentChannel != null)
                    OnLocalChannelChanged(oldChannelID);
                else
                    WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to find new channel with ID {newChannelID}.");
            }

            return;
        }

        var client = GetClientByClientId(clientID);

        if (newChannel == null)
        {
            if (client != null)
            {
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Client left server. ClientID: {clientID}, ClientName: {client.ClientName}, OldChannelID: {oldChannelID}, NewChannelID: {newChannelID}");

                RemoveClientThreadSafe(client, resetPos: false);
            }
            else
            {
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Unregistered client left server. ClientID: {clientID}, OldChannelID: {oldChannelID}, NewChannelID: {newChannelID}");
            }

            return;
        }

        if (newChannel == currentChannel || newChannel.IsSubscribed)
        {
            if (client != null)
            {
                client.Channel = newChannel;
                newChannel.ClientCount++;

                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Client joined current/subscribed channel. ClientID: {clientID}, ClientName: {client.ClientName}, OldChannelID: {oldChannelID}, NewChannelID: {newChannelID}");
            }
            else
            {
                var name = GetClientName(serverConnectionHandlerID, clientID);

                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"New client joined current/subscribed channel. ClientID: {clientID}, ClientName: {name}, OldChannelID: {oldChannelID}, NewChannelID: {newChannelID}");

                client = AddClientThreadSafe(clientID, newChannel, name);
            }

            if (oldChannel != null)
                oldChannel.ClientCount--;

            if (newChannel.IsIngame)
                SendLocalInfoToClient(client);

            UpdateClientPosition(client);
        }
        else if (oldChannel != null && (oldChannel == currentChannel || oldChannel.IsSubscribed))
        {
            if (client != null)
            {
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Client left current/subscribed channel. ClientID: {clientID}, ClientName: {client.ClientName}, OldChannelID: {oldChannelID}, NewChannelID: {newChannelID}");

                RemoveClientThreadSafe(client, resetPos: true);
            }
            else
            {
                WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Unregistered client left current/subscribed channel. ClientID: {clientID}, OldChannelID: {oldChannelID}, NewChannelID: {newChannelID}");
            }
        }
        else
        {
            if (oldChannel != null)
                oldChannel.ClientCount--;

            if (newChannel != null)
                newChannel.ClientCount++;
        }

        // Else the client moved between channels that aren't current/subscribed.
    }

    void OnTalkStatusChanged(ulong serverId, ushort clientId, TalkStatus status, bool isReceivedWhisper)
    {
        if (serverId != currentServerId)
            return;

        var client = GetClientByClientId(clientId);

        if (client == null)
            return;

        if (isReceivedWhisper) // Event caused by whisper
        {
            bool isWhispering = status == TalkStatus.STATUS_TALKING;

            //WriteConsole($"Setting client {clientId} whispering state to {isWhispering}");

            if (isWhispering != client.IsWhispering)
            {
                client.IsWhispering = isWhispering;
                UpdateClientPosition(client);
            }
        }
    }

    void CalculateClientVolume(ushort clientId, ref float volume)
    {
        var client = GetClientByClientId(clientId);

        if (client == null || client.Position == default)
            return;

        // TODO: Scale volume down when client is facing away

        float minD = minDistance;
        float maxDist = maxDistance;
        float scale = distanceScale;
        float falloff = distanceFalloff;

        if (client.ExtendRange)
        {
            maxDist *= extendRangeFactor;
            falloff /= extendRangeFactor;
        }

        float dist = Vector3.Distance(default, client.Position);
        float vol = float.Min(1f, 1f / float.Pow(dist * scale + 1 - minD * scale, (float)falloff));

        float limit = dist / maxDist;
        limit = 1 - limit * limit;

        vol *= limit;
        vol = float.Max(0, vol);

        volume = vol;
    }

    #region Wrapper Methods

    unsafe void LogMessage(string message, LogLevel level, string? channel)
    {
        byte nullChar = 0;
        var msgPtr = Marshal.StringToHGlobalAnsi(message);
        var chnPtr = channel != null ? Marshal.StringToHGlobalAnsi(channel) : (IntPtr)(&nullChar);

        try
        {
            var err = (Ts3ErrorType)functions.logMessage((byte*)msgPtr, level, (byte*)chnPtr, logID: 0);

            if (err != Ts3ErrorType.ERROR_ok)
                WriteConsole($"Failed to log message \"{message}\"");
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
        WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Setting system 3D settings. DistanceFactor: {distanceFactor}, RolloffScale: {rolloffScale}.");

        var err = (Ts3ErrorType)functions.systemset3DSettings(currentServerId, distanceFactor, rolloffScale);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to set system 3D settings. Error: {err}");
            return false;
        }

        return true;
    }

    unsafe void SetListener(Vector3 forward, Vector3 up)
    {
        //WriteConsole($"Setting listener attribs. Forward: {forward}, Up: {up}.");

        Vector3 zeroPos = default;

        var err = (Ts3ErrorType)functions.systemset3DListenerAttributes(currentServerId, (TS3_VECTOR*)&zeroPos, (TS3_VECTOR*)&forward, (TS3_VECTOR*)&up);

        if (err != Ts3ErrorType.ERROR_ok)
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to set listener attribs. Error: {err}");
    }

    unsafe void SetClientPos(ushort clientId, Vector3 position)
    {
        if (clientId == 0)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Tried to set position of client ID 0.");
            return;
        }

        //WriteConsole($"Setting position of client {clientId} to {position}.");

        position.Z = -position.Z;

        var err = (Ts3ErrorType)functions.channelset3DAttributes(currentServerId, clientId, (TS3_VECTOR*)&position);

        if (err != Ts3ErrorType.ERROR_ok)
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to set client pos to {position}. Error: {err}");
    }

    unsafe bool GetLocalClientAndChannelIds(ulong serverId, out ushort clientId, out ulong channelId)
    {
        clientId = 0;
        channelId = 0;

        ushort clientID;
        var err = (Ts3ErrorType)functions.getClientID(serverId, &clientID);

        if (err == Ts3ErrorType.ERROR_ok)
        {
            clientId = clientID;
            WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Got client ID: {clientID}");
        }
        else
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get client ID. Error: {err}");
            return false;
        }

        ulong channelID;
        err = (Ts3ErrorType)functions.getChannelOfClient(serverId, clientID, &channelID);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get channel ID. Error: {err}");
            return false;
        }

        channelId = channelID;
        WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"Got channel ID: {channelID}");

        return true;
    }

    unsafe bool DoesChannelExist(ulong serverId, ulong channelId)
    {
        ulong parentId;
        var err = (Ts3ErrorType)functions.getParentChannelOfChannel(serverId, channelId, &parentId);

        if (err == Ts3ErrorType.ERROR_ok)
            return true;

        if (err != Ts3ErrorType.ERROR_channel_invalid_id)
            WriteConsole($"Unexpected error when checkng channel exists. Error: {err}");

        return false;
    }

    unsafe Ts3ErrorType GetChannelStringProperty(ulong serverId, ulong channelId, ChannelProperties property, out string? value)
    {
        value = null;

        byte* strPtr;
        var err = (Ts3ErrorType)functions.getChannelVariableAsString(serverId, channelId, (nint)property, &strPtr);

        if (err != Ts3ErrorType.ERROR_ok)
            return err;

        value = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(strPtr));
        err = (Ts3ErrorType)functions.freeMemory(strPtr);

        if (err != Ts3ErrorType.ERROR_ok)
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to free channel string value pointer. Error: {err}");

        return err;
    }

    unsafe bool GetChannelName(ulong serverId, ulong channelId, [NotNullWhen(true)] out string? name)
    {
        var err = GetChannelStringProperty(serverId, channelId, ChannelProperties.CHANNEL_NAME, out name);

        if (err == Ts3ErrorType.ERROR_ok && name != null)
            return true;

        WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get channel name. Error: {err}");

        return false;
    }

    unsafe bool GetChannelTopic(ulong serverId, ulong channelId, [NotNullWhen(true)] out string? topic)
    {
        var err = GetChannelStringProperty(serverId, channelId, ChannelProperties.CHANNEL_TOPIC, out topic);

        if (err == Ts3ErrorType.ERROR_ok && topic != null)
            return true;

        WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get channel topic. Error: {err}");

        return false;
    }

    unsafe bool? GetChannelIsSubscribed(ulong serverId, ulong channelId)
    {
        int isSubscribed;
        var err = (Ts3ErrorType)functions.getChannelVariableAsInt(serverId, channelId, (nint)ChannelProperties.CHANNEL_FLAG_ARE_SUBSCRIBED, &isSubscribed);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get if channel is subscribed. Error: {err}");
            return null;
        }

        return isSubscribed != 0;
    }

    unsafe string? GetClientName(ulong serverId, ushort clientId)
    {
        byte* nameBuffer;
        var err = (Ts3ErrorType)functions.getClientVariableAsString(serverId, clientId, (nint)ClientProperties.CLIENT_NICKNAME, &nameBuffer);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get client name. Error: {err}");
            return null;
        }

        var name = Marshal.PtrToStringUTF8((IntPtr)nameBuffer);
        err = (Ts3ErrorType)functions.freeMemory(nameBuffer);

        if (err != Ts3ErrorType.ERROR_ok)
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to free client name pointer. Error: {err}");

        return name;
    }

    unsafe bool GetLocalMuteStateForClient(ulong serverId, ushort clientId, out bool muted)
    {
        int mutedState;
        var err = (Ts3ErrorType)functions.getClientVariableAsInt(serverId, clientId, (int)ClientProperties.CLIENT_IS_MUTED, &mutedState);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            muted = false;
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get client local muting state. ClientID: {clientId}, Error: {err}");
            return false;
        }

        muted = mutedState != 0;
        return true;
    }

    unsafe void SendMessageToClient(ulong serverId, ushort clientId, string message)
    {
        IntPtr ptr = Marshal.StringToHGlobalAnsi(message);

        try
        {
            var err = (Ts3ErrorType)functions.requestSendPrivateTextMsg(serverId, (byte*)ptr, clientId, null);

            if (err != Ts3ErrorType.ERROR_ok)
                WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to send private message to clientID {clientId}. Error: {err}");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    unsafe bool MuteClientLocally(ulong serverId, ushort clientId)
    {
        ushort* clientArray = stackalloc ushort[2] { clientId, 0 };

        var err = (Ts3ErrorType)functions.requestMuteClients(serverId, clientArray, null);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to mute client locally. ClientID: {clientId}, Error: {err}");
            return false;
        }

        return true;
    }

    unsafe bool UnmuteClientLocally(ulong serverId, ushort clientId)
    {
        ushort* clientArray = stackalloc ushort[2] { clientId, 0 };

        var err = (Ts3ErrorType)functions.requestUnmuteClients(serverId, clientArray, null);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to unmute client locally. ClientID: {clientId}, Error: {err}");
            return false;
        }

        return true;
    }

    unsafe ulong? GetParentChannel(ulong serverId, ulong channelId)
    {
        ulong parentChannel;
        var err = (Ts3ErrorType)functions.getParentChannelOfChannel(serverId, channelId, &parentChannel);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get parent channel. Error: {err}");
            return null;
        }

        return parentChannel;
    }

    unsafe string? GetChannelTopic(ulong serverId, ulong channelId)
    {
        byte* strPtr;
        var err = (Ts3ErrorType)functions.getChannelVariableAsString(serverId, channelId, (nint)ChannelProperties.CHANNEL_TOPIC, &strPtr);

        if (err != Ts3ErrorType.ERROR_ok)
        {
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to get channel topic. Error: {err}");
            return null;
        }

        var topic = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(strPtr));

        err = (Ts3ErrorType)functions.freeMemory(strPtr);

        if (err != Ts3ErrorType.ERROR_ok)
            WriteConsoleAndLog(LogLevel.LogLevel_ERROR, $"Failed to free channel topic pointer. Error: {err}");

        return topic;
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

        instance.WriteConsoleAndLog(LogLevel.LogLevel_INFO, "Registered plugin ID: " + Encoding.UTF8.GetString(charSpan));
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
        instance.currentServerId = serverConnectionHandlerID;
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onConnectStatusChangeEvent")]
    public unsafe static void ts3plugin_onConnectStatusChangeEvent(ulong serverConnectionHandlerID, int newStatus, uint errorNumber)
    {
        //WriteConsoleAndLog(LogLevel.LogLevel_INFO, $"ConnectStatusChangeEvent. NewStatus: {newStatus}");

        var connStatus = (ConnectStatus)newStatus;

        if (connStatus == ConnectStatus.STATUS_DISCONNECTED)
        {
            instance.OnServerDisconnected();
        }
        else if (connStatus == ConnectStatus.STATUS_CONNECTION_ESTABLISHED)
        {
            if (instance.UpdateLocalClientIdAndChannel(instance.currentServerId))
            {
                instance.Set3DSettings(instance.distanceScale, 1);
                instance.RefetchTSClients();

                if (instance.currentChannel is { IsIngame: true })
                    instance.SendLocalInfoToCurrentChannel();
            }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onNewChannelEvent")]
    public unsafe static void ts3plugin_onNewChannelEvent(ulong serverConnectionHandlerID, ulong channelID, ulong channelParentID)
    {
        instance.OnNewChannel(serverConnectionHandlerID, channelID, channelParentID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onNewChannelCreatedEvent")]
    public unsafe static void ts3plugin_onNewChannelCreatedEvent(ulong serverConnectionHandlerID, ulong channelID, ulong channelParentID, ushort invokerID, /*const */byte* invokerName, /*const */byte* invokerUniqueIdentifier)
    {
        instance.OnNewChannelCreated(serverConnectionHandlerID, channelID, channelParentID, invokerID, invokerName, invokerUniqueIdentifier);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onDelChannelEvent")]
    public unsafe static void ts3plugin_onDelChannelEvent(ulong serverConnectionHandlerID, ulong channelID, ushort invokerID, /*const */byte* invokerName, /*const */byte* invokerUniqueIdentifier)
    {
        instance.OnChannelDeleted(serverConnectionHandlerID, channelID, invokerID, invokerName, invokerUniqueIdentifier);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onChannelMoveEvent")]
    public unsafe static void ts3plugin_onChannelMoveEvent(ulong serverConnectionHandlerID, ulong channelID, ulong newChannelParentID, ushort invokerID, /*const */byte* invokerName, /*const */byte* invokerUniqueIdentifier)
    {
        instance.OnChannelMoved(serverConnectionHandlerID, channelID, newChannelParentID, invokerID, invokerName, invokerUniqueIdentifier);
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
        instance.OnTalkStatusChanged(serverConnectionHandlerID, clientID, (TalkStatus)status, isReceivedWhisper != 0);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onCustom3dRolloffCalculationClientEvent")]
    public unsafe static void ts3plugin_onCustom3dRolloffCalculationClientEvent(ulong serverConnectionHandlerID, ushort clientID, float distance, float* volume)
    {
        if (serverConnectionHandlerID != instance.currentServerId)
            return;

        instance.CalculateClientVolume(clientID, ref *volume);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onPluginCommandEvent")]
    public unsafe static void ts3plugin_onPluginCommandEvent(ulong serverConnectionHandlerID, byte* pluginName, byte* pluginCommand,
        ushort invokerClientID, byte* invokerName, byte* invokerUniqueIdentity)
    {
        if (serverConnectionHandlerID != instance.currentServerId)
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
            instance.WriteConsoleAndLog(LogLevel.LogLevel_WARNING, $"Received plugin command from unregistered client, ClientID: {invokerClientID}, InvokerName: {invokerNameStr}");
            return;
        }

        var cmd = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pluginCommand);

        if (cmd.Length != 0)
            instance.HandlePluginCommandEvent(nameUtf8, cmd, client);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientDisplayNameChanged")]
    public unsafe static void ts3plugin_onClientDisplayNameChanged(ulong serverConnectionHandlerID, ushort clientID, /*const */byte* displayName, /*const */byte* uniqueClientIdentifier)
    {
        if (serverConnectionHandlerID != instance.currentServerId)
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
            //WriteConsole($"Unregistered client changed display name. ClientID: {clientID}");
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onUpdateChannelEditedEvent")]
    public unsafe static void ts3plugin_onUpdateChannelEditedEvent(ulong serverConnectionHandlerID, ulong channelID, ushort invokerID, /*const */byte* invokerName, /*const */byte* invokerUniqueIdentifier)
    {
        instance.OnChannelEdited(serverConnectionHandlerID, channelID, invokerID, invokerName, invokerUniqueIdentifier);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onChannelSubscribeEvent")]
    public unsafe static void ts3plugin_onChannelSubscribeEvent(ulong serverConnectionHandlerID, ulong channelID)
    {
        instance.OnChannelSubscribed(serverConnectionHandlerID, channelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onChannelSubscribeFinishedEvent")]
    public unsafe static void ts3plugin_onChannelSubscribeFinishedEvent(ulong serverConnectionHandlerID, ulong channelID)
    {
        instance.OnChannelSubscribeFinished(serverConnectionHandlerID, channelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onChannelUnsubscribeEvent")]
    public unsafe static void ts3plugin_onChannelUnsubscribeEvent(ulong serverConnectionHandlerID, ulong channelID)
    {
        instance.OnChannelUnsubscribed(serverConnectionHandlerID, channelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onChannelUnsubscribeFinishedEvent")]
    public unsafe static void ts3plugin_onChannelUnsubscribeFinishedEvent(ulong serverConnectionHandlerID, ulong channelID)
    {
        instance.OnChannelUnsubscribeFinished(serverConnectionHandlerID, channelID);
    }

    [UnmanagedCallersOnly(EntryPoint = "ts3plugin_onClientMoveSubscriptionEvent")]
    public unsafe static void ts3plugin_onClientMoveSubscriptionEvent(ulong serverConnectionHandlerID, ushort clientID, ulong oldChannelID, ulong newChannelID, Visibility visibility)
    {
        instance.OnClientMoveSubscriptionEvent(serverConnectionHandlerID, clientID, oldChannelID, newChannelID, visibility);
    }
#pragma warning restore IDE1006 // Naming Styles
    #endregion
}
