using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using SharedPluginClasses;

namespace SETSMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Mod : MySessionComponentBase
    {
        // Picked at random, may conflict with other mods.
        const ushort MPMessageId1 = 42691;
        const ushort MPMessageId2 = 42692;

        AntennaSystemHelper antennaSystemHelper = new AntennaSystemHelper();

        List<IMyPlayer> playerList = new List<IMyPlayer>();
        HashSet<IMyPlayer> playerSet = new HashSet<IMyPlayer>();

        List<PlayerInfo> playerInfos = new List<PlayerInfo>();
        Dictionary<IMyPlayer, PlayerInfo> infosByPlayer = new Dictionary<IMyPlayer, PlayerInfo>();
#if DEBUG_INCLUDE_OFFLINE_PLAYERS
        Dictionary<IMyCharacter, MyTuple<ulong, long>> offlineCharacters = new Dictionary<IMyCharacter, MyTuple<ulong, long>>();
        Dictionary<IMyCharacter, PlayerInfo> infosByCharacter = new Dictionary<IMyCharacter, PlayerInfo>();
#endif

        class PlayerInfo
        {
            public IMyPlayer Player;
            public IMyCharacter Character;
            public ulong SteamId;
            public long IdentityId;
            public PluginVersion PluginVersion;
            public HashSet<MyDataBroadcaster> RelayedBroadcasters;
            public HashSet<MyDataReceiver> RelayedReceivers;
            public HashSet<MyDataBroadcaster> VisibleBroadcasters;

            public PlayerInfo(IMyPlayer player)
            {
                Player = player;
                Character = player.Character;
                SteamId = player.SteamUserId;
                RelayedBroadcasters = new HashSet<MyDataBroadcaster>();
                RelayedReceivers = new HashSet<MyDataReceiver>();
                VisibleBroadcasters = new HashSet<MyDataBroadcaster>();
            }

#if DEBUG_INCLUDE_OFFLINE_PLAYERS
            public PlayerInfo(IMyCharacter character, ulong steamId, long identityId)
            {
                Player = null;
                Character = character;
                SteamId = steamId;
                IdentityId = identityId;
                RelayedBroadcasters = new HashSet<MyDataBroadcaster>();
                RelayedReceivers = new HashSet<MyDataReceiver>();
                VisibleBroadcasters = new HashSet<MyDataBroadcaster>();
            }
#endif
        }

        int playerUpdateIndex;
        byte[] cachedMessageArray;

        public override void LoadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            MyLog.Default.WriteLineAndConsole("[SE-TS Mod] LoadData");

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(MPMessageId2, MPMessageHandler);
        }

        protected override void UnloadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            MyLog.Default.WriteLineAndConsole("[SE-TS Mod] UnloadData");

            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(MPMessageId2, MPMessageHandler);
        }

        void MPMessageHandler(ushort handlerId, byte[] message, ulong steamId, bool fromServer)
        {
            // If the message does not match what is expected there might be another mod using the same handlerId.

            if (!fromServer)
                return; // Should never get messages from clients

            const int headerSize = sizeof(ushort) + sizeof(uint) + sizeof(uint);

            if (message.Length < headerSize)
                return;

            int offset = 0;

            if (ReadUInt16(message, ref offset) != MPMessageId2)
                return;

            uint numBytes = ReadUInt32(message, ref offset);

            if (numBytes != (uint)message.Length)
                return; // Invalid message

            uint playerPluginVersion = ReadUInt32(message, ref offset);
            var player = GetPlayerBySteamId(steamId);

            if (player != null)
                player.PluginVersion = new PluginVersion(playerPluginVersion);
        }

        static ushort ReadUInt16(byte[] bytes, ref int offset)
        {
            ushort value = (ushort)(bytes[offset + 0] | ((uint)bytes[offset + 1] << 8));
            offset += sizeof(ushort);
            return value;
        }

        static uint ReadUInt32(byte[] bytes, ref int offset)
        {
            uint value = bytes[offset + 0] | ((uint)bytes[offset + 1] << 8) | ((uint)bytes[offset + 2] << 16) | ((uint)bytes[offset + 3] << 24);
            offset += sizeof(uint);
            return value;
        }

        PlayerInfo GetPlayerBySteamId(ulong steamId)
        {
            foreach (var info in playerInfos)
            {
                if (info.SteamId == steamId)
                    return info;
            }

            return null;
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
            int livePlayerCount = UpdatePlayerTracking();

            if (livePlayerCount < 1)
                return;

            UpdatePlayerAntennaRelays();

            var message = GetMessageBuffer(playerInfos.Count - 1); // Minus one since players don't need info about themselves

            foreach (var player in playerInfos)
            {
                player.RelayedBroadcasters.RemoveWhere(b => b.Closed);
                player.RelayedReceivers.RemoveWhere(r => r.Entity == null || r.Entity.Closed || r.Entity.MarkedForClose);
                player.VisibleBroadcasters.RemoveWhere(b => b.Closed);

                if (player.Player != null)
                    SendUpdateToPlayer(player, ref message);

                message.Reset();
            }
        }

        int UpdatePlayerTracking()
        {
            MyAPIGateway.Players.GetPlayers(playerList);

            foreach (var player in playerList)
                playerSet.Add(player);

#if DEBUG_INCLUDE_OFFLINE_PLAYERS
            GetOfflineCharacters();
#endif

            List<PlayerInfo> infosToRemove = null;

            foreach (var info in playerInfos)
            {
                if (info.Player != null && playerSet.Contains(info.Player))
                    continue;

#if DEBUG_INCLUDE_OFFLINE_PLAYERS
                if (info.Character != null && offlineCharacters.ContainsKey(info.Character))
                    continue;
#endif

                if (infosToRemove == null)
                    infosToRemove = new List<PlayerInfo>();

                infosToRemove.Add(info);
            }

            if (infosToRemove != null)
            {
                foreach (var info in infosToRemove)
                {
                    playerInfos.Remove(info);

                    if (info.Player != null)
                        infosByPlayer.Remove(info.Player);

#if DEBUG_INCLUDE_OFFLINE_PLAYERS
                    if (info.Character != null)
                        infosByCharacter.Remove(info.Character);
#endif
                }
            }

            foreach (var player in playerList)
            {
                PlayerInfo info;

                if (infosByPlayer.TryGetValue(player, out info))
                {
                    info.Character = player.Character;
                    info.IdentityId = player.IdentityId;
                }
                else
                {
                    info = new PlayerInfo(player);
                    playerInfos.Add(info);
                    infosByPlayer.Add(player, info);
                }
            }

#if DEBUG_INCLUDE_OFFLINE_PLAYERS
            foreach (var item in offlineCharacters)
            {
                var character = item.Key;
                ulong steamId = item.Value.Item1;
                long identityId = item.Value.Item2;

                if (infosByCharacter.ContainsKey(character))
                    continue;

                var playerInfo = new PlayerInfo(character, steamId, identityId);

                playerInfos.Add(playerInfo);
                infosByCharacter.Add(character, playerInfo);
            }

            offlineCharacters.Clear();
#endif

            int livePlayerCount = playerList.Count;

            playerList.Clear();
            playerSet.Clear();

            return livePlayerCount;
        }

        void UpdatePlayerAntennaRelays()
        {
            if (playerUpdateIndex >= playerInfos.Count)
                playerUpdateIndex = 0;

            // Throttle connection checks for performance
            var player = playerInfos[playerUpdateIndex];

            playerUpdateIndex = (playerUpdateIndex + 1) % playerInfos.Count;

            player.RelayedBroadcasters.Clear();
            player.RelayedReceivers.Clear();
            player.VisibleBroadcasters.Clear();

            if (player.Character == null)
                return;

            var receiver = player.Character.Components.Get<MyDataReceiver>();
            var broadcaster = player.Character.Components.Get<MyDataBroadcaster>();

            antennaSystemHelper.GetAllRelayedBroadcasters(broadcaster, player.IdentityId, player.RelayedBroadcasters);
            antennaSystemHelper.GetAllRelayedReceivers(receiver, player.IdentityId, player.RelayedReceivers);

            foreach (var r in player.RelayedReceivers)
            {
                foreach (var b in r.BroadcastersInRange)
                    player.VisibleBroadcasters.Add(b);
            }
        }

        MessageBuffer GetMessageBuffer(int playerCount)
        {
            int numBytes = sizeof(int) + sizeof(int) + playerCount * (sizeof(ulong) + sizeof(bool));
            var array = cachedMessageArray;

            if (array == null || array.Length != numBytes)
                cachedMessageArray = array = new byte[numBytes];

            return new MessageBuffer { Array = array };
        }

        void SendUpdateToPlayer(PlayerInfo player, ref MessageBuffer message)
        {
            message.Write(message.Length);
            message.Write(playerInfos.Count - 1);

            foreach (var other in playerInfos)
            {
                if (other == player)
                    continue;

                // TODO: Count relay hops

                bool hasConnection = false;

                foreach (var b in other.RelayedBroadcasters)
                {
                    if (player.VisibleBroadcasters.Contains(b))
                    {
                        hasConnection = true;
                        break;
                    }
                }

                message.Write(other.SteamId);
                message.Write(hasConnection);
            }

            MyAPIGateway.Multiplayer.SendMessageTo(MPMessageId1, message.Array, player.SteamId, reliable: true);
        }

#if DEBUG_INCLUDE_OFFLINE_PLAYERS
        void GetOfflineCharacters()
        {
            var players = MyAPIGateway.Players;

            foreach (var character in GetCharactersRecursive())
            {
                var player = players.GetPlayerControllingEntity(character);

                if (player != null)
                    continue;

                var owner = character as IMyComponentOwner<MyIDModule>;
                MyIDModule idModule;

                if (owner == null || !owner.GetComponent(out idModule))
                    continue;

                long identityId = idModule.Owner;
                ulong steamId = players.TryGetSteamId(identityId);

                if (steamId == 0)
                    continue;

                offlineCharacters.Add(character, new MyTuple<ulong, long>(steamId, identityId));
            }
        }

        static IEnumerable<IMyCharacter> GetCharactersRecursive()
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities);

            HashSet<IMyEntity> childEntities = null;

            foreach (var item in entities)
            {
                IMyCharacter character;

                if ((character = item as IMyCharacter) != null)
                {
                    yield return character;
                }
                else if (item is IMyCubeGrid)
                {
                    if (childEntities == null)
                        childEntities = new HashSet<IMyEntity>();

                    item.Hierarchy.GetChildrenRecursive(childEntities);

                    foreach (var child in childEntities)
                    {
                        IMyCharacter _char;

                        if ((_char = child as IMyCharacter) != null)
                            yield return _char;
                    }

                    childEntities.Clear();
                }
            }
        }
#endif
    }

    class AntennaSystemHelper
    {
        HashSet<MyDataBroadcaster> tempBroadcasters = new HashSet<MyDataBroadcaster>();

        public bool CheckConnection(IMyCharacter sender, IMyCharacter receiver, long receiverIdentityId)
        {
            if (sender == null || receiver == null)
                return false;

            if (sender == receiver)
                return true;

            var dataReceiver = receiver.Components.Get<MyDataReceiver>();
            var dataBroadcaster = sender.Components.Get<MyDataBroadcaster>();

            return CheckConnection(dataBroadcaster, dataReceiver, receiverIdentityId);
        }

        public bool CheckConnection(MyDataBroadcaster broadcaster, MyDataReceiver receiver, long receiverIdentityId)
        {
            if (broadcaster == null || receiver == null)
                return false;

            GetAllRelayedBroadcasters(broadcaster, receiverIdentityId, tempBroadcasters);

            bool hasConnection = tempBroadcasters.Contains(broadcaster);

            tempBroadcasters.Clear();

            return hasConnection;
        }

        public void GetAllRelayedReceivers(MyDataReceiver receiver, long identityId, HashSet<MyDataReceiver> output)
        {
            output.Add(receiver);

            var broadcaster = receiver.Broadcaster;

            if (broadcaster != null)
                GetAllRelayedReceiversRecursive(broadcaster, identityId, output);
        }

        void GetAllRelayedReceiversRecursive(MyDataBroadcaster broadcaster, long identityId, HashSet<MyDataReceiver> output)
        {
            foreach (var receiver in broadcaster.ReceiversInRange)
            {
                var receiverEntity = receiver.Entity;

                if (receiverEntity == null || receiverEntity.Closed || receiverEntity.MarkedForClose)
                    continue;

                if (output.Contains(receiver) || !CanBeUsedByPlayer(receiverEntity, identityId))
                    continue;

                output.Add(receiver);

                var relayBroadcaster = receiver.Broadcaster;

                if (relayBroadcaster != null)
                    GetAllRelayedReceiversRecursive(relayBroadcaster, identityId, output);
            }
        }

        public void GetAllRelayedBroadcasters(MyDataBroadcaster broadcaster, long identityId, HashSet<MyDataBroadcaster> output)
        {
            output.Add(broadcaster);

            GetAllRelayedBroadcastersRecursive(broadcaster, identityId, output);
        }

        void GetAllRelayedBroadcastersRecursive(MyDataBroadcaster broadcaster, long identityId, HashSet<MyDataBroadcaster> output)
        {
            foreach (var receiver in broadcaster.ReceiversInRange)
            {
                var receiverEntity = receiver.Entity;

                if (receiverEntity == null || receiverEntity.Closed || receiverEntity.MarkedForClose)
                    continue;

                var relayBroadcaster = receiver.Broadcaster;

                if (relayBroadcaster == null || output.Contains(relayBroadcaster) || !CanBeUsedByPlayer(receiverEntity, identityId))
                    continue;

                output.Add(relayBroadcaster);
                GetAllRelayedBroadcastersRecursive(relayBroadcaster, identityId, output);
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

        public static bool IsFriendlyRelation(MyRelationsBetweenPlayers relation)
        {
            switch (relation)
            {
            case MyRelationsBetweenPlayers.Self:
            case MyRelationsBetweenPlayers.Allies:
                return true;

            case MyRelationsBetweenPlayers.Neutral:
            case MyRelationsBetweenPlayers.Enemies:
                break;
            }

            return false;
        }

        public static bool IsFriendlyRelation(MyRelationsBetweenPlayerAndBlock relation)
        {
            switch (relation)
            {
            case MyRelationsBetweenPlayerAndBlock.Owner:
            case MyRelationsBetweenPlayerAndBlock.FactionShare:
            case MyRelationsBetweenPlayerAndBlock.Friends:
                return true;

            case MyRelationsBetweenPlayerAndBlock.NoOwnership:
            case MyRelationsBetweenPlayerAndBlock.Neutral:
            case MyRelationsBetweenPlayerAndBlock.Enemies:
                break;
            }

            return false;
        }

        public static bool HasFriendlyRelation(long owner, long user)
        {
            if (owner == user)
                return true;//MyRelationsBetweenPlayers.Self;

            if (owner == 0 || user == 0)
                return false;//MyRelationsBetweenPlayers.Neutral;

            var factions = MyAPIGateway.Session.Factions;

            var userFaction = factions.TryGetPlayerFaction(user);
            var ownerFaction = factions.TryGetPlayerFaction(owner);

            if (userFaction == null && ownerFaction == null)
                return false;//MyRelationsBetweenPlayers.Enemies;

            if (userFaction == ownerFaction)
                return true;//MyRelationsBetweenPlayers.Allies;

            if (userFaction == null)
                return ownerFaction.IsFriendly(user);

            if (ownerFaction == null)
                return userFaction.IsFriendly(owner);

            var factionRelation = factions.GetRelationBetweenFactions(ownerFaction.FactionId, userFaction.FactionId);

            switch (factionRelation)
            {
            case MyRelationsBetweenFactions.Neutral:
            //case MyRelationsBetweenFactions.Allies:
            case MyRelationsBetweenFactions.Friends:
                return true;
            case MyRelationsBetweenFactions.Enemies:
                break;
            }

            return false;
        }
    }

    static class SerializationHelper
    {
        public static void Write(byte[] bytes, int offset, int value)
        {
            int o = offset;
            bytes[o + 0] = (byte)(value >> 0);
            bytes[o + 1] = (byte)(value >> 8);
            bytes[o + 2] = (byte)(value >> 16);
            bytes[o + 3] = (byte)(value >> 24);
        }

        public static void Write(byte[] bytes, int offset, ulong value)
        {
            int o = offset;
            bytes[o + 0] = (byte)(value >> 0);
            bytes[o + 1] = (byte)(value >> 8);
            bytes[o + 2] = (byte)(value >> 16);
            bytes[o + 3] = (byte)(value >> 24);
            bytes[o + 4] = (byte)(value >> 32);
            bytes[o + 5] = (byte)(value >> 40);
            bytes[o + 6] = (byte)(value >> 48);
            bytes[o + 7] = (byte)(value >> 56);
        }

        public static void Write(byte[] bytes, int offset, bool value)
        {
            bytes[offset] = (byte)(value ? 1 : 0);
        }
    }

    struct MessageBuffer
    {
        public byte[] Array;
        int offset;

        public int Length => Array.Length;

        public void Reset()
        {
            offset = 0;
        }

        public void Write(int value)
        {
            SerializationHelper.Write(Array, offset, value);
            offset += sizeof(int);
        }

        public void Write(ulong value)
        {
            SerializationHelper.Write(Array, offset, value);
            offset += sizeof(ulong);
        }

        public void Write(bool value)
        {
            SerializationHelper.Write(Array, offset, value);
            offset += sizeof(bool);
        }
    }
}
