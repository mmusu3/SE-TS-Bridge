using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SharedPluginClasses;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Plugins;
using VRage.Utils;
using VRageMath;

namespace TSPluginForSE;

public class Plugin : IPlugin
{
    static PluginVersion currentVersion = new(4, 0);

    internal class PlayerInfo
    {
        public IMyPlayer? InternalPlayer;
        public ulong SteamID;
        public string DisplayName;
        public Vector3D Position;
        public PlayerStateFlags Flags;

        public bool HasConnection
        {
            get => (Flags & PlayerStateFlags.HasConnection) != 0;
            set => SetFlag(value, PlayerStateFlags.HasConnection);
        }

        public bool InCockpit
        {
            get => (Flags & PlayerStateFlags.InCockpit) != 0;
            set => SetFlag(value, PlayerStateFlags.InCockpit);
        }
#if BI_DIRECTIONAL
        public bool IsWhispering
        {
            get => (Flags & PlayerStateFlags.Whispering) != 0;
            set => SetFlag(value, PlayerStateFlags.Whispering);
        }
#endif

        public PlayerInfo(IMyPlayer? internalPlayer, ulong steamID, string displayName, Vector3D position)
        {
            InternalPlayer = internalPlayer;
            SteamID = steamID;
            DisplayName = displayName;
            Position = position;
            HasConnection = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetFlag(bool value, PlayerStateFlags flag)
        {
            if (value)
                Flags |= flag;
            else
                Flags &= ~flag;
        }
    }

    List<IMyPlayer> tempPlayers = []; // Reused list
    List<PlayerInfo> currentPlayers = [];
    List<PlayerInfo> newPlayers = [];
    List<PlayerInfo> removedPlayers = [];
    NamedPipeServerStream? pipeStream;
    CancellationTokenSource pipeCancellation;
    Task? connectTask;

    readonly Vector3 mouthOffset = new Vector3(0, 1.6f, -0.1f); // Approximate mouth offset

    // Picked at random, may conflict with other mods.
    const ushort MPMessageId1 = 42691;
    const ushort MPMessageId2 = 42692;

    Action<ushort, byte[], ulong, bool> mpMessageHandler;

    public Plugin()
    {
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

        pipeCancellation = new CancellationTokenSource();

        const PipeDirection direction = PipeDirection.Out;

        try
        {
            pipeStream = new NamedPipeServerStream("09C842DD-F683-4798-A95F-88B0981265BE", direction, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }
        catch (IOException)
        {
            pipeStream = null;
        }

        if (pipeStream != null)
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
        MySession.Static.OnReady += Session_OnReady;

        MyAPIGateway.Multiplayer?.RegisterSecureMessageHandler(MPMessageId1, mpMessageHandler);
    }

    void Session_OnReady()
    {
        if (MyAPIGateway.Multiplayer == null)
            return;

        int messageSize = sizeof(ushort) + sizeof(uint) + sizeof(uint);
        var message = new byte[messageSize];
        var span = message.AsSpan();

        Write(ref span, MPMessageId2);
        Write(ref span, (uint)message.Length);
        Write(ref span, currentVersion.Packed);

        MyAPIGateway.Multiplayer.SendMessageToServer(MPMessageId2, message, reliable: true);
    }

    void MySession_OnUnloading()
    {
        MySession.Static.OnReady -= Session_OnReady;

        MyAPIGateway.Multiplayer?.UnregisterSecureMessageHandler(MPMessageId1, mpMessageHandler);

        if (pipeStream == null || !pipeStream.IsConnected)
            return;

        MyLog.Default.WriteLine($"[SE-TS Bridge] Clearing {currentPlayers.Count} players due to session unload.");

        currentPlayers.Clear();

        var updatePacket = new GameUpdatePacket {
            Header = new GameUpdatePacketHeader {
                Version = currentVersion.Packed,
                InSession = false,
                LocalSteamID = Sync.MyId,
            },
            Forward = Vector3.Forward,
            Up = Vector3.Up
        };

        var arrayPool = ArrayPool<byte>.Shared;
        int dataSize = GameUpdatePacket.Size;
        var buffer = arrayPool.Rent(dataSize);
        var span = buffer.AsSpan(0, dataSize);

        Write(ref span, updatePacket);

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

        if (pipeStream != null && pipeStream.IsConnected)
            pipeStream.Disconnect();
        else
            pipeCancellation.Cancel();

        pipeStream?.Dispose();
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

    unsafe static T Read<T>(byte[] bytes, ref int offset) where T : unmanaged
    {
        var value = MemoryMarshal.Read<T>(bytes.AsSpan(offset));
        offset += sizeof(T);
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
        if (pipeStream == null || !pipeStream.IsConnected)
        {
            if (pipeStream != null && (connectTask == null || connectTask.IsCompleted || connectTask.IsFaulted))
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
        var players = MyAPIGateway.Players;

        try
        {
            if (session != null
                && MyAPIGateway.Multiplayer.MultiplayerActive
                && players != null
                && session.LocalHumanPlayer != null)
            {
                SendUpdate(players, session);
            }
            else
            {
                SendUpdate();
            }
        }
        catch (Exception ex)
        {
            if (ex is IOException)
            {
                MyLog.Default.WriteLine("[SE-TS Bridge] Connection closed.");
                MyAPIGateway.Utilities?.ShowMessage("SE-TS Bridge", "Connection closed.");
            }
            else
            {
                MyLog.Default.WriteLine("[SE-TS Bridge] Exception while updating.");
                MyLog.Default.WriteLine(ex);
                MyAPIGateway.Utilities?.ShowMessage("SE-TS Bridge", "Exception while updating.");
            }
        }
    }

    void SendUpdate()
    {
        var updatePacket = new GameUpdatePacket {
            Header = new GameUpdatePacketHeader {
                Version = currentVersion.Packed,
                InSession = false,
                LocalSteamID = Sync.MyId,
            },
            Forward = Vector3.Forward,
            Up = Vector3.Up
        };

        var arrayPool = ArrayPool<byte>.Shared;
        int dataSize = GameUpdatePacket.Size;
        var buffer = arrayPool.Rent(dataSize);
        var span = buffer.AsSpan(0, dataSize);

        Write(ref span, updatePacket);

        try
        {
            pipeStream!.Write(buffer, 0, dataSize);
            pipeStream.Flush();
        }
        finally
        {
            arrayPool.Return(buffer);
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
                newPlayersByteLength += sizeof(int);
            }
        }

        var camera = session.Camera;
        var localPos = camera.WorldMatrix.Translation;
        var localOrient = camera.WorldMatrix.Rotation;
        var inverseLocalOrient = Quaternion.Inverse(Quaternion.CreateFromRotationMatrix(camera.WorldMatrix.GetOrientation()));

        var updatePacket = new GameUpdatePacket {
            Header = new GameUpdatePacketHeader {
                Version = currentVersion.Packed,
                InSession = true,
                LocalSteamID = localPlayer.SteamUserId,
            },
            Forward = localOrient.Forward,
            Up = localOrient.Up,
            PlayerCount = currentPlayers.Count,
            RemovedPlayerCount = removedPlayers.Count,
            NewPlayerCount = newPlayers.Count,
            NewPlayerByteLength = newPlayersByteLength
        };

        var arrayPool = ArrayPool<byte>.Shared;
        int dataSize = GameUpdatePacket.Size
            + ClientGameState.Size * currentPlayers.Count
            + sizeof(ulong) * removedPlayers.Count
            + newPlayersByteLength;

        var buffer = arrayPool.Rent(dataSize);
        var span = buffer.AsSpan(0, dataSize);

        Write(ref span, updatePacket);

        if (currentPlayers.Count != 0)
        {
            foreach (var item in currentPlayers)
            {
                var p = item.InternalPlayer;

                Vector3 pos;
                bool inCockpit = false;

                if (p != null)
                {
                    var c = p.Character;

                    if (c != null)
                    {
                        pos = c.GetPosition() + Vector3.Transform(mouthOffset, c.WorldMatrix.GetOrientation());

                        if (c.Parent is IMyShipController cockpit)
                            inCockpit = cockpit.CanControlShip;
                    }
                    else
                    {
                        pos = p.GetPosition();
                    }

                    item.Position = pos;
                }
                else
                {
                    pos = item.Position;
                }

                item.InCockpit = inCockpit;

                var relPos = (Vector3)(pos - localPos);
                relPos = Vector3.Transform(relPos, inverseLocalOrient);

                PlayerStateFlags flags = 0;

                if (item.HasConnection)
                    flags |= PlayerStateFlags.HasConnection;

                if (item.InCockpit)
                    flags |= PlayerStateFlags.InCockpit;

                var state = new ClientGameState {
                    SteamID = item.SteamID,
                    Position = relPos,
                    Flags = flags
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

                PlayerStateFlags flags = 0;

                if (item.HasConnection)
                    flags |= PlayerStateFlags.HasConnection;

                if (item.InCockpit)
                    flags |= PlayerStateFlags.InCockpit;

                Write(ref span, relPos);
                Write(ref span, (int)flags);

                currentPlayers.Add(item);
            }

            newPlayers.Clear();
        }

        try
        {
            pipeStream!.Write(buffer, 0, dataSize);
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
            bool inCockpit = false;

            if (c != null)
            {
                pos = c.GetPosition() + Vector3.Transform(mouthOffset, c.WorldMatrix.GetOrientation());

                if (c.Parent is IMyShipController cockpit)
                    inCockpit = cockpit.CanControlShip;
            }
            else
            {
                pos = item.GetPosition();
            }

            newPlayers.Add(new PlayerInfo(item, item.SteamUserId, item.DisplayName, pos) { InCockpit = inCockpit });
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

            int index = -1;

            for (int i = 0; i < currentPlayers.Count; i++)
            {
                if (currentPlayers[i].SteamID == playerId.SteamId)
                {
                    index = i;
                    break;
                }
            }

            var pos = ((IMyEntity)character).GetPosition();
            pos += Vector3.Transform(mouthOffset, character.WorldMatrix.GetOrientation());

            if (index == -1)
            {
                newPlayers.Add(new PlayerInfo(null, playerId.SteamId, character.GetIdentity().DisplayName, pos));
            }
            else if (currentPlayers[index].InternalPlayer == null)
            {
                currentPlayers[index].Position = pos;
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
