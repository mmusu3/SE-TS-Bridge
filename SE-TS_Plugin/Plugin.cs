using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Engine.Networking;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Plugins;
using VRage.Utils;
using VRageMath;

namespace SETSPlugin;

public class Plugin : IPlugin
{
    internal class PlayerInfo
    {
        public IMyPlayer? InternalPlayer;
        public ulong SteamID;
        public string DisplayName;
        public Vector3D Position;
        public bool HasConnection;

        public PlayerInfo(IMyPlayer? internalPlayer, ulong steamID, string displayName, Vector3D position)
        {
            InternalPlayer = internalPlayer;
            SteamID = steamID;
            DisplayName = displayName;
            Position = position;
            HasConnection = true;
        }
    }

    List<IMyPlayer> tempPlayers = new(); // Reused list
    List<PlayerInfo> currentPlayers = new();
    List<PlayerInfo> newPlayers = new();
    List<PlayerInfo> removedPlayers = new();
    NamedPipeServerStream pipeStream;
    CancellationTokenSource pipeCancellation;
    Task? connectTask;

    readonly Vector3 mouthOffset = new Vector3(0, 1.6f, -0.1f); // Approximate mouth offset

    const int minor = 1;
    const int patch = 1;
    const int CurrentVersion = (0xABCD << 16) | (minor << 8) | patch; // 16 bits of magic value, 8 bits of major, 8 bits of minor

    struct PlayerStatesHeader
    {
        public int Version;
        public ulong LocalSteamId;
        public Vector3 Forward;
        public Vector3 Up;
        public int PlayerCount;
        public int RemovedPlayerCount;
        public int NewPlayerCount;
        public int NewPlayerByteLength;

        public unsafe static readonly int Size = sizeof(PlayerStatesHeader);
    }

    struct ClientState
    {
        public ulong SteamID;
        public Vector3 Position;
        public bool HasConnection;

        public unsafe static readonly int Size = sizeof(ClientState);
    }

    const ushort mpMessageId = 42691; // Picked at random, may conflict with other mods.

    Action<ushort, byte[], ulong, bool> mpMessageHandler;

    public Plugin()
    {
        pipeStream = null!;
        pipeCancellation = null!;
        mpMessageHandler = MPMessageHandler;
    }

    public void Init(object gameInstance)
    {
        MyLog.Default.WriteLine("[SE-TS Bridge] Initializing.");

        BeginConnection();

        MySession.OnLoading += MySession_OnLoading;
        MySession.OnUnloading += MySession_OnUnloading;

        MyLog.Default.WriteLine("[SE-TS Bridge] Initialized.");
    }

    void BeginConnection()
    {
        MyLog.Default.WriteLine("[SE-TS Bridge] Beginning connection.");

        pipeStream = new NamedPipeServerStream("09C842DD-F683-4798-A95F-88B0981265BE", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        pipeCancellation = new CancellationTokenSource();
        connectTask = pipeStream.WaitForConnectionAsync(pipeCancellation.Token).ContinueWith(OnPipeConnected);
    }

    void OnPipeConnected(Task task)
    {
        connectTask = null;

        if (task.IsCanceled || task.IsFaulted)
            return;

        MyLog.Default.WriteLine($"[SE-TS Bridge] Established connection with TeamSpeak plugin.");
        MyAPIGateway.Utilities?.InvokeOnGameThread(() => MyAPIGateway.Utilities?.ShowMessage("SE-TS Bridge", "Established connection with TeamSpeak plugin."), "SE-TS Bridge");
    }

    void MySession_OnLoading()
    {
        MyAPIGateway.Multiplayer?.RegisterSecureMessageHandler(mpMessageId, mpMessageHandler);
    }

    void MySession_OnUnloading()
    {
        MyAPIGateway.Multiplayer?.UnregisterSecureMessageHandler(mpMessageId, mpMessageHandler);

        if (!pipeStream.IsConnected)
            return;

        MyLog.Default.WriteLine($"[SE-TS Bridge] Removing {currentPlayers.Count} players due to session unload.");

        var header = new PlayerStatesHeader {
            Version = CurrentVersion,
            LocalSteamId = MyGameService.UserId,
            Forward = Vector3.Forward,
            Up = Vector3.Up,
            PlayerCount = 0,
            RemovedPlayerCount = currentPlayers.Count,
            NewPlayerCount = 0,
            NewPlayerByteLength = 0
        };

        var arrayPool = ArrayPool<byte>.Shared;
        int dataSize = PlayerStatesHeader.Size + sizeof(ulong) * currentPlayers.Count;
        var buffer = arrayPool.Rent(dataSize);
        var span = buffer.AsSpan(0, dataSize);
        Write(ref span, header);

        if (currentPlayers.Count != 0)
        {
            foreach (var item in currentPlayers)
                Write(ref span, item.SteamID);

            currentPlayers.Clear();
        }

        try
        {
            pipeStream.WriteAsync(buffer, 0, dataSize);
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    public void Dispose()
    {
        MyLog.Default.WriteLine("[SE-TS Bridge] Disposing.");

        MySession.OnLoading -= MySession_OnLoading;
        MySession.OnUnloading -= MySession_OnUnloading;

        if (pipeStream.IsConnected)
            pipeStream.Disconnect();
        else
            pipeCancellation.Cancel();

        pipeStream.Dispose();
        pipeCancellation.Dispose();

        currentPlayers.Clear();

        MyLog.Default.WriteLine("[SE-TS Bridge] Disposed.");
    }

    void MPMessageHandler(ushort handlerId, byte[] message, ulong steamId, bool fromServer)
    {
        // If the message does not match what is expected there might be another mod using the same handlerId.

        if (!fromServer)
            return; // Should never get messages from clients.

        if (message.Length < sizeof(int))
            return; // Message must always at least specify the number of bytes.

        ReadOnlySpan<byte> bytes = message;

        int numBytes = Read<int>(ref bytes);

        if (numBytes != message.Length)
            return; // Invalid message.

        int numPlayers = Read<int>(ref bytes);
        int expectedBytes = numPlayers * (sizeof(ulong) + sizeof(bool));

        if (bytes.Length != expectedBytes)
            return; // Invalid message.

        for (int i = 0; i < numPlayers; i++)
        {
            ulong playerId = Read<ulong>(ref bytes);
            bool hasConnection = Read<bool>(ref bytes);
            var player = GetPlayerBySteamId(playerId);

            if (player != null)
                player.HasConnection = hasConnection;
        }
    }

    unsafe static T Read<T>(ref ReadOnlySpan<byte> span) where T : unmanaged
    {
        var value = MemoryMarshal.Read<T>(span);
        span = span.Slice(sizeof(T));
        return value;
    }

    PlayerInfo? GetPlayerBySteamId(ulong steamId)
    {
        foreach (var item in currentPlayers)
        {
            if (item.SteamID == steamId)
                return item;
        }

        return null;
    }

    public void Update()
    {
        if (!pipeStream.IsConnected)
        {
            if (connectTask == null || connectTask.IsCompleted || connectTask.IsFaulted)
            {
                MyLog.Default.WriteLine("[SE-TS Bridge] Restarting connection.");
                MyAPIGateway.Utilities?.ShowMessage("SE-TS Bridge", "Restarting connection.");

                pipeStream.Dispose();
                currentPlayers.Clear();
                BeginConnection();
            }

            return;
        }

        var session = MyAPIGateway.Session;

        if (session == null || !MyAPIGateway.Multiplayer.MultiplayerActive)
            return;

        var pc = MyAPIGateway.Players;

        if (pc == null)
            return;

        if (session.LocalHumanPlayer == null)
            return;

        try
        {
            SendUpdate(pc, session);
        }
        catch (Exception ex)
        {
            if (ex is not System.IO.IOException)
            {
                MyLog.Default.WriteLine("[SE-TS Bridge] Exception while updating.");
                MyLog.Default.WriteLine(ex);
                MyAPIGateway.Utilities?.ShowMessage("SE-TS Bridge", "Exception while updating.");
            }
        }
    }

    void SendUpdate(IMyPlayerCollection playerCollection, IMySession session)
    {
        var localPlayer = session.LocalHumanPlayer;

        AddNewPlayers(playerCollection, localPlayer);

        // Testing code for SE single player
        //AddOfflinePlayers(localPlayer);

        int newPlayersByteLength = 0;

        if (newPlayers.Count != 0)
        {
            foreach (var item in newPlayers)
            {
                newPlayersByteLength += sizeof(ulong);
                newPlayersByteLength += sizeof(int);
                newPlayersByteLength += item.DisplayName.Length * sizeof(char);
                newPlayersByteLength += sizeof(float) * 3; // sizeof(Vector3);
                newPlayersByteLength += sizeof(bool);
            }
        }

        var camera = session.Camera;
        var localPos = camera.WorldMatrix.Translation;
        var localOrient = camera.WorldMatrix.Rotation;
        var inverseLocalOrient = Quaternion.Inverse(Quaternion.CreateFromRotationMatrix(camera.WorldMatrix.GetOrientation()));

        var header = new PlayerStatesHeader {
            Version = CurrentVersion,
            LocalSteamId = localPlayer.SteamUserId,
            Forward = localOrient.Forward,
            Up = localOrient.Up,
            PlayerCount = currentPlayers.Count,
            RemovedPlayerCount = removedPlayers.Count,
            NewPlayerCount = newPlayers.Count,
            NewPlayerByteLength = newPlayersByteLength
        };

        var arrayPool = ArrayPool<byte>.Shared;
        int dataSize = PlayerStatesHeader.Size
            + ClientState.Size * currentPlayers.Count
            + sizeof(ulong) * removedPlayers.Count
            + newPlayersByteLength;

        var buffer = arrayPool.Rent(dataSize);
        var span = buffer.AsSpan(0, dataSize);

        Write(ref span, header);

        if (currentPlayers.Count != 0)
        {
            foreach (var item in currentPlayers)
            {
                var p = item.InternalPlayer;

                Vector3 pos;

                if (p != null)
                {
                    var c = p.Character;

                    if (c != null)
                        pos = c.GetPosition() + Vector3.Transform(mouthOffset, c.WorldMatrix.GetOrientation());
                    else
                        pos = p.GetPosition();

                    item.Position = pos;
                }
                else
                {
                    pos = item.Position;
                }

                var relPos = (Vector3)(pos - localPos);
                relPos = Vector3.Transform(relPos, inverseLocalOrient);

                var state = new ClientState {
                    SteamID = item.SteamID,
                    Position = relPos,
                    HasConnection = item.HasConnection
                };

                Write(ref span, state);
            }
        }

        if (removedPlayers.Count != 0)
        {
            MyLog.Default.WriteLine($"[SE-TS Bridge] Removing {removedPlayers.Count} players.");

            foreach (var item in removedPlayers)
                Write(ref span, item.SteamID);

            removedPlayers.Clear();
        }

        if (newPlayers.Count != 0)
        {
            MyLog.Default.WriteLine($"[SE-TS Bridge] Adding {newPlayers.Count} new players.");

            foreach (var item in newPlayers)
            {
                Write(ref span, item.SteamID);
                Write(ref span, item.DisplayName.Length);

                var nameBytes = MemoryMarshal.AsBytes(item.DisplayName.AsSpan());
                nameBytes.CopyTo(span);
                span = span.Slice(nameBytes.Length);

                var relPos = (Vector3)(item.Position - localPos);
                relPos = Vector3.Transform(relPos, inverseLocalOrient);

                Write(ref span, relPos);
                Write(ref span, item.HasConnection);

                currentPlayers.Add(item);
            }

            newPlayers.Clear();
        }

        try
        {
            pipeStream.Write(buffer, 0, dataSize);
            pipeStream.Flush();
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    unsafe static void Write<T>(ref Span<byte> span, in T value) where T : unmanaged
    {
        MemoryMarshal.Write(span, ref Unsafe.AsRef(in value));
        span = span.Slice(sizeof(T));
    }

    void AddNewPlayers(IMyPlayerCollection playerCollection, IMyPlayer localPlayer)
    {
        playerCollection.GetPlayers(tempPlayers);

        for (int i = 0; i < currentPlayers.Count; i++)
        {
            var item = currentPlayers[i];

            if (item.InternalPlayer != null && !PlayerExists(tempPlayers, item.InternalPlayer))
            {
                removedPlayers.Add(item);
                currentPlayers.RemoveAt(i--);
            }
        }

        foreach (var item in tempPlayers)
        {
            if (item == localPlayer || item.IsBot || PlayerExists(currentPlayers, item))
                continue;

            var c = item.Character;

            Vector3D pos;

            if (c != null)
                pos = c.GetPosition() + Vector3.Transform(mouthOffset, c.WorldMatrix.GetOrientation());
            else
                pos = item.GetPosition();

            newPlayers.Add(new PlayerInfo(item, item.SteamUserId, item.DisplayName, pos));
        }

        tempPlayers.Clear();
    }

    static bool PlayerExists(List<IMyPlayer> players, IMyPlayer player)
    {
        foreach (var item in players)
        {
            if (item.SteamUserId == player.SteamUserId)
                return true;
        }

        return false;
    }

    static bool PlayerExists(List<PlayerInfo> players, IMyPlayer player)
    {
        foreach (PlayerInfo item in players)
        {
            if (item.SteamID == player.SteamUserId)
                return true;
        }

        return false;
    }

    void AddOfflinePlayers(IMyPlayer localPlayer)
    {
        foreach (var character in GetCharactersRecursive())
        {
            character.GetPlayerId(out var playerId);

            if (playerId.SteamId == localPlayer.SteamUserId)
                continue;

            int index = currentPlayers.FindIndex(p => p.SteamID == playerId.SteamId);

            if (index == -1)
            {
                var pos = ((IMyEntity)character).GetPosition();
                pos += Vector3.Transform(mouthOffset, character.WorldMatrix.GetOrientation());

                newPlayers.Add(new PlayerInfo(null, playerId.SteamId, character.GetIdentity().DisplayName, pos));
            }
        }
    }

    static IEnumerable<MyCharacter> GetCharactersRecursive()
    {
        var entities = MyEntities.GetEntities();

        foreach (var item in entities)
        {
            switch (item)
            {
            case MyCharacter character:
                yield return character;
                break;
            case MyCubeGrid:
                foreach (var child in GetCharactersRecursive(item.Hierarchy))
                    yield return child;
                break;
            }
        }
    }

    static IEnumerable<MyCharacter> GetCharactersRecursive(MyHierarchyComponentBase hierarchy)
    {
        foreach (var item in hierarchy.Children)
        {
            var child = item.Container.Entity;

            switch (child)
            {
            case MyCharacter character:
                yield return character;
                break;
            default:
                foreach (var character in GetCharactersRecursive(child.Hierarchy))
                    yield return character;
                break;
            }
        }
    }
}
