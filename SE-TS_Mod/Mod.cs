using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace SETSMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Mod : MySessionComponentBase
    {
        const ushort mpMessageId = 42691; // Picked at random, may conflict with other mods.

        AntennaSystemHelper antennaSystemHelper = new AntennaSystemHelper();
        SerializationHelper serializationHelper = new SerializationHelper();

        List<IMyPlayer> playerList = new List<IMyPlayer>();

        public override void LoadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            MyLog.Default.WriteLineAndConsole("[SE-TS Mod] LoadData");
        }

        protected override void UnloadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            MyLog.Default.WriteLineAndConsole("[SE-TS Mod] UnloadData");
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            //try
            {
                Update();
            }
            //catch (Exception ex)
            //{
            //    MyLog.Default.Error($"[SE-TS Mod] Exception in update: {ex}");
            //}
        }

        void Update()
        {
            if (MyAPIGateway.Players.Count < 2)
                return;

            MyAPIGateway.Players.GetPlayers(playerList);

            int playerCount = playerList.Count - 1;
            int numBytes = sizeof(int) + sizeof(int) + playerCount * (sizeof(ulong) + sizeof(bool));
            var message = new byte[numBytes]; // TODO: Array pooling

            foreach (var player in playerList)
                SendUpdateToPlayer(player, playerList, message);

            playerList.Clear();
        }

        void SendUpdateToPlayer(IMyPlayer player, List<IMyPlayer> otherPlayers, byte[] message)
        {
            int offset = 0;
            int playerCount = otherPlayers.Count - 1;

            serializationHelper.Write(message, ref offset, message.Length);
            serializationHelper.Write(message, ref offset, playerCount);

            foreach (var other in otherPlayers)
            {
                if (other == player)
                    continue;

                MyIDModule idModule;

                bool hasConnection = OwnershipHelper.TryGetIDModule(other.Character, out idModule)
                    && OwnershipHelper.IsFriendlyRelation(idModule.GetUserRelationToOwner(player.IdentityId));

                if (hasConnection)
                {
                    // TODO: Improve perf by removing redundant checks with shared antennas
                    // TODO: Count relay hops
                    hasConnection = antennaSystemHelper.CheckConnection(other.Character, player.Character, player.IdentityId);
                }

                serializationHelper.Write(message, ref offset, other.SteamUserId);
                serializationHelper.Write(message, ref offset, hasConnection);
            }

            MyAPIGateway.Multiplayer.SendMessageTo(mpMessageId, message, player.SteamUserId, reliable: true);
        }
    }

    class AntennaSystemHelper
    {
        HashSet<MyDataBroadcaster> relayedBroadcasters = new HashSet<MyDataBroadcaster>();
        HashSet<long> nearbyBroadcastersSearchExceptions = new HashSet<long>();

        public bool CheckConnection(IMyCharacter sender, IMyCharacter receiver, long receiverIdentityId)
        {
            if (sender == null || receiver == null)
                return false;

            if (sender == receiver)
                return true;

            var r = receiver.Components.Get<MyDataReceiver>();
            var b = sender.Components.Get<MyDataBroadcaster>();

            return CheckConnection(b, r, receiverIdentityId, false);
        }

        public bool CheckConnection(MyDataBroadcaster broadcaster, MyDataReceiver receiver, long receiverIdentityId, bool mutual)
        {
            if (broadcaster == null || receiver == null)
                return false;

            GetAllRelayedBroadcasters(receiver, receiverIdentityId, mutual, relayedBroadcasters);

            bool hasConnection = relayedBroadcasters.Contains(broadcaster);

            relayedBroadcasters.Clear();

            return hasConnection;
        }

        public void GetAllRelayedBroadcasters(MyDataReceiver receiver, long identityId, bool mutual, HashSet<MyDataBroadcaster> output)
        {
            nearbyBroadcastersSearchExceptions.Clear();
            nearbyBroadcastersSearchExceptions.Add(receiver.Entity.EntityId);

            GetAllRelayedBroadcastersRecursive(receiver, identityId, mutual, output);

            nearbyBroadcastersSearchExceptions.Clear();
        }

        void GetAllRelayedBroadcastersRecursive(MyDataReceiver receiver, long identityId, bool mutual, HashSet<MyDataBroadcaster> output)
        {
            var selfBroadcaster = receiver.Broadcaster;

            foreach (var broadcaster in receiver.BroadcastersInRange)
            {
                if (broadcaster.Closed || output.Contains(broadcaster))
                    continue;

                var relayReceiver = broadcaster.Receiver;

                if (mutual && (relayReceiver == null || selfBroadcaster == null || !relayReceiver.BroadcastersInRange.Contains(selfBroadcaster)))
                    continue;

                output.Add(broadcaster);

                if (relayReceiver != null && CanBeUsedByPlayer(broadcaster.Entity, identityId))
                {
                    if (nearbyBroadcastersSearchExceptions.Add(broadcaster.Entity.EntityId))
                        GetAllRelayedBroadcastersRecursive(relayReceiver, identityId, mutual, output);
                }
            }
        }

        static bool CanBeUsedByPlayer(IMyEntity entity, long playerId)
        {
            // UseAllTerminals should not allow voice comms
            //if (entity is IMyTerminalBlock && HasAdminUseTerminals(playerId))
            //    return true;

            MyIDModule idModule;

            if (OwnershipHelper.TryGetIDModule(entity, out idModule))
            {
                // TODO: May want to re-implement for perf
                var relation = idModule.GetUserRelationToOwner(playerId);

                return OwnershipHelper.IsFriendlyRelation(relation);
            }

            return true;
        }
    }

    static class OwnershipHelper
    {
        public static bool TryGetIDModule(IMyEntity entity, out MyIDModule idModule)
        {
            idModule = null;

            var owner = entity as IMyComponentOwner<MyIDModule>;

            return owner != null && owner.GetComponent(out idModule);
        }

        public static bool IsFriendlyRelation(MyRelationsBetweenPlayerAndBlock relation)
        {
            switch (relation)
            {
            case MyRelationsBetweenPlayerAndBlock.NoOwnership:
            case MyRelationsBetweenPlayerAndBlock.Neutral:
            case MyRelationsBetweenPlayerAndBlock.Enemies:
                break;
            case MyRelationsBetweenPlayerAndBlock.Owner:
            case MyRelationsBetweenPlayerAndBlock.FactionShare:
            case MyRelationsBetweenPlayerAndBlock.Friends:
                return true;
            }

            return false;
        }
    }

    class SerializationHelper
    {
        public void Write(byte[] bytes, ref int offset, int value)
        {
            bytes[offset + 0] = (byte)((value >> 0) & 0xFF);
            bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
            bytes[offset + 2] = (byte)((value >> 16) & 0xFF);
            bytes[offset + 3] = (byte)((value >> 24) & 0xFF);
            offset += sizeof(int);
        }

        public void Write(byte[] bytes, ref int offset, ulong value)
        {
            bytes[offset + 0] = (byte)((value >> 0) & 0xFF);
            bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
            bytes[offset + 2] = (byte)((value >> 16) & 0xFF);
            bytes[offset + 3] = (byte)((value >> 24) & 0xFF);
            bytes[offset + 4] = (byte)((value >> 32) & 0xFF);
            bytes[offset + 5] = (byte)((value >> 40) & 0xFF);
            bytes[offset + 6] = (byte)((value >> 48) & 0xFF);
            bytes[offset + 7] = (byte)((value >> 56) & 0xFF);
            offset += sizeof(ulong);
        }

        public void Write(byte[] bytes, ref int offset, bool value)
        {
            bytes[offset] = (byte)(value ? 1 : 0);
            offset += sizeof(bool);
        }
    }
}
