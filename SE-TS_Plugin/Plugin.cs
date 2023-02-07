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

    readonly Vector3 mouthOffset = new Vector3(0, 1.6f, -0.1f); // Approximate mouth offset

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
        if (!pipeStream.IsConnected)
        {
            if (connectResult == null || connectResult.IsCompleted)
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
            if (item == localPlayer || item.IsBot || PlayerExists(currentPlayers, item))
                continue;

            var c = item.Character;

            Vector3D pos;

            if (c != null)
                pos = c.GetPosition() + Vector3.Transform(mouthOffset, c.WorldMatrix.GetOrientation());
            else
                pos = item.GetPosition();

            newPlayers.Add(new Player {
                InternalPlayer = item,
                SteamID = item.SteamUserId,
                DisplayName = item.DisplayName,
                Position = pos
            });
        }

        tempPlayers.Clear();

        // Testing code for SE single player
        {
            //static IEnumerable<MyCharacter> GetCharacters()
            //{
            //    var entities = new HashSet<IMyEntity>();
            //    MyAPIGateway.Entities.GetEntities(entities);

            //    HashSet<IMyEntity> childEntities = null;

            //    foreach (var item in entities)
            //    {
            //        switch (item)
            //        {
            //        case MyCharacter character:
            //            yield return character;
            //            break;
            //        case IMyCubeGrid:
            //            childEntities ??= new HashSet<IMyEntity>();
            //            item.Hierarchy.GetChildrenRecursive(childEntities);

            //            foreach (var child in childEntities)
            //            {
            //                if (child is MyCharacter _char)
            //                    yield return _char;
            //            }

            //            childEntities.Clear();
            //            break;
            //        }
            //    }
            //}

            //foreach (var character in GetCharacters())
            //{
            //    character.GetPlayerId(out var playerId);

            //    if (playerId.SteamId == localPlayer.SteamUserId)
            //        continue;

            //    int index = currentPlayers.FindIndex(p => p.SteamID == playerId.SteamId);

            //    if (index == -1)
            //    {
            //        var pos = ((IMyEntity)character).GetPosition();
            //        pos += Vector3.Transform(mouthOffset, character.WorldMatrix.GetOrientation());

            //        newPlayers.Add(new Player {
            //            SteamID = playerId.SteamId,
            //            DisplayName = character.GetIdentity().DisplayName,
            //            Position = pos
            //        });
            //    }
            //}
        }

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
