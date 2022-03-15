using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Plugins;
using VRage.Utils;
using VRageMath;

namespace SETSPlugin;

public class Plugin : IPlugin
{
    class Player
    {
        public IMyPlayer InternalPlayer;
        public ulong SteamID;
        public string DisplayName;
        public Vector3D Position;
    }

    List<IMyPlayer> tempPlayers = new();
    List<Player> currentPlayers = new();
    List<Player> newPlayers = new();
    List<Player> removedPlayers = new();
    NamedPipeServerStream pipeStream;
    IAsyncResult connectResult;

    struct Header
    {
        public int CheckValue;
        public Vector3 Forward;
        public Vector3 Up;
        public int PlayerCount;
        public int RemovedPlayerCount;
        public int NewPlayerCount;
        public int NewPlayerByteLength;

        public unsafe static readonly int Size = sizeof(Header);
    }

    struct ClientState
    {
        public ulong SteamID;
        public Vector3 Position;

        public unsafe static readonly int Size = sizeof(ClientState);
    }

    public void Init(object gameInstance)
    {
        MyLog.Default.WriteLine("[SE-TS Bridge] Initializing.");

        BeginConnection();

        MySession.OnUnloading += MySession_OnUnloading;

        MyLog.Default.WriteLine("[SE-TS Bridge] Initialized.");
    }

    void BeginConnection()
    {
        MyLog.Default.WriteLine("[SE-TS Bridge] Beginning connection.");

        pipeStream = new NamedPipeServerStream("09C842DD-F683-4798-A95F-88B0981265BE", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        connectResult = pipeStream.BeginWaitForConnection(OnPipeConnected, null);
    }

    void OnPipeConnected(IAsyncResult result)
    {
        try
        {
            pipeStream.EndWaitForConnection(result);
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        finally
        {
            connectResult = null;
        }

        MyLog.Default.WriteLine($"[SE-TS Bridge] Established connection with TeamSpeak plugin.");
        MyAPIGateway.Utilities?.InvokeOnGameThread(() => MyAPIGateway.Utilities?.ShowMessage("SE-TS Bridge", "Established connection with TeamSpeak plugin."), "SE-TS Bridge");
    }

    void MySession_OnUnloading()
    {
        if (!pipeStream.IsConnected)
            return;

        MyLog.Default.WriteLine($"[SE-TS Bridge] Removing {currentPlayers.Count} players due to session unload.");

        var header = new Header {
            CheckValue = 0x12ABCDEF,
            Forward = Vector3.Forward,
            Up = Vector3.Up,
            PlayerCount = 0,
            RemovedPlayerCount = currentPlayers.Count,
            NewPlayerCount = 0,
            NewPlayerByteLength = 0
        };

        var arrayPool = ArrayPool<byte>.Shared;
        int dataSize = Header.Size + sizeof(ulong) * currentPlayers.Count;
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

        MySession.OnUnloading -= MySession_OnUnloading;

        if (pipeStream.IsConnected)
            pipeStream.Disconnect();

        pipeStream.Dispose();
        currentPlayers.Clear();

        MyLog.Default.WriteLine("[SE-TS Bridge] Disposed.");
    }

    public void Update()
    {
        var session = MyAPIGateway.Session;

        if (session == null || !MyAPIGateway.Multiplayer.MultiplayerActive)
            return;

        var pc = MyAPIGateway.Players;

        if (pc == null)
            return;

        if (!pipeStream.IsConnected)
        {
            if (connectResult == null)
            {
                MyLog.Default.WriteLine("[SE-TS Bridge] Restarting connection.");
                MyAPIGateway.Utilities?.ShowMessage("SE-TS Bridge", "Restarting connection.");

                pipeStream.Dispose();
                currentPlayers.Clear();
                BeginConnection();
            }

            return;
        }

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

    void SendUpdate(IMyPlayerCollection pc, IMySession session)
    {
        var localPlayer = session.LocalHumanPlayer;

        pc.GetPlayers(tempPlayers);

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
            if (item == localPlayer || PlayerExists(currentPlayers, item))
                continue;

            var pos = item.GetPosition();

            if (item.Character != null)
                // Add approximate head height
                pos += Vector3.Transform(new Vector3(0, 1.7f, 0), item.Character.WorldMatrix.GetOrientation());

            newPlayers.Add(new Player {
                InternalPlayer = item,
                SteamID = item.SteamUserId,
                DisplayName = item.DisplayName,
                Position = pos
            });
        }

        tempPlayers.Clear();

        // Testing code for SE single player
        //var entities = new HashSet<IMyEntity>();
        //MyAPIGateway.Entities.GetEntities(entities, e => e is MyCharacter);

        //foreach (var item in entities)
        //{
        //    var character = (MyCharacter)item;
        //    character.GetPlayerId(out var playerId);

        //    if (playerId.SteamId == localPlayer.SteamUserId)
        //        continue;

        //    int index = currentPlayers.FindIndex(p => p.SteamID == playerId.SteamId);

        //    if (index == -1)
        //    {
        //        newPlayers.Add(new Player {
        //            SteamID = playerId.SteamId,
        //            DisplayName = character.GetIdentity().DisplayName,
        //            Position = item.GetPosition()
        //        });
        //    }
        //}

        int newPlayersByteLength = 0;

        if (newPlayers.Count != 0)
        {
            foreach (var item in newPlayers)
            {
                newPlayersByteLength += sizeof(ulong);
                newPlayersByteLength += sizeof(int);
                newPlayersByteLength += item.DisplayName.Length * sizeof(char);
                newPlayersByteLength += sizeof(float) * 3; // sizeof(Vector3);
            }
        }

        var camera = session.Camera;
        var localPos = camera.WorldMatrix.Translation;
        var localOrient = camera.WorldMatrix.Rotation;
        var inverseLocalOrient = Quaternion.Inverse(Quaternion.CreateFromRotationMatrix(camera.WorldMatrix.GetOrientation()));

        var header = new Header {
            CheckValue = 0x12ABCDEF,
            Forward = localOrient.Forward,
            Up = localOrient.Up,
            PlayerCount = currentPlayers.Count,
            RemovedPlayerCount = removedPlayers.Count,
            NewPlayerCount = newPlayers.Count,
            NewPlayerByteLength = newPlayersByteLength
        };

        var arrayPool = ArrayPool<byte>.Shared;
        int dataSize = Header.Size
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
                Vector3 pos;

                if (item.InternalPlayer != null)
                {
                    item.Position = item.InternalPlayer.GetPosition();
                    pos = item.Position;

                    if (item.InternalPlayer.Character != null)
                        // Add approximate head height
                        pos += Vector3.Transform(new Vector3(0, 1.7f, 0), item.InternalPlayer.Character.WorldMatrix.GetOrientation());
                }
                else
                {
                    pos = item.Position;
                }

                var relPos = (Vector3)(pos - localPos);
                relPos = Vector3.Transform(relPos, inverseLocalOrient);

                var state = new ClientState {
                    SteamID = item.SteamID,
                    Position = relPos
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

                currentPlayers.Add(item);
            }

            newPlayers.Clear();
        }

        try
        {
            pipeStream.WriteAsync(buffer, 0, dataSize);
            pipeStream.FlushAsync();
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

    static bool PlayerExists(List<IMyPlayer> players, IMyPlayer player)
    {
        foreach (var item in players)
        {
            if (item.SteamUserId == player.SteamUserId)
                return true;
        }

        return false;
    }

    static bool PlayerExists(List<Player> players, IMyPlayer player)
    {
        foreach (Player item in players)
        {
            if (item.SteamID == player.SteamUserId)
                return true;
        }

        return false;
    }
}
