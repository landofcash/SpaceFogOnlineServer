namespace SpaceFogOnlineServer
{
    using NLog;
    using Riptide;
    using Riptide.Transports;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using XSpaceFogOnline.Networking.Entities;

    public enum ClientToServerId : ushort
    {
        PlayerInit = 1,
        PlayerUpdate,
    }

    public enum ServerToClientId : ushort
    {
        PlayerInit = 1,
        Spawn,
        SpawnMultiple,
        SpawnOtherPlayers,
        SyncDeleteOtherPlayers,
        Movement,
    }
    internal static class MessageProcessor
    {
        public static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static SpaceFogOnlineServer SFServer = SpaceFogOnlineServer.SFServer;

        [MessageHandler((ushort)ClientToServerId.PlayerInit)]
        private static void PlayerInit(ushort clientId, Message message)
        {
            LOGGER.Debug($"PlayerInit: {clientId}");
            //1. send player 
            var player = new NetworkChunkedEntity()
            {
                Position = new Vector3(0, 10, 0),
                Chunk = new Chunk(0, 0, 0),
                Name = "Player",
                Rotation = Quaternion.Zero
            };
            SFServer.World.Players.Remove(clientId);
            SFServer.World.Players.Add(clientId, new NetworkOtherPlayer()
            {
                Position = player.Position, 
                Chunk = player.Chunk, 
                Name = "OtherPlayer", 
                Rotation = player.Rotation, 
                SessionId = clientId
            });

            var playerInitMessage = Message.Create(MessageSendMode.Reliable, ServerToClientId.PlayerInit).AddSerializable(player);
            LOGGER.Debug($"PlayerInit: {clientId} Sending Player info back size:{playerInitMessage.WrittenLength}");
            SFServer.Server.Send(playerInitMessage, clientId);

            //2. send chunks back to player
            foreach (var chunk in SFServer.World.WorldChunks)
            {
                SendChunk(clientId, chunk);
            }
            //2. send otherPlayers 
            SendOtherPlayers(clientId);

            //3. send spawn to all others 
            NetworkOtherPlayer otherPlayer = new NetworkOtherPlayer()
            {
                Chunk = player.Chunk,
                Name = "OtherPlayer",
                Position = player.Position,
                Rotation = player.Rotation,
                SessionId = clientId
            };
            var playerMessage = Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnOtherPlayers).AddSerializables(new []{otherPlayer});
            LOGGER.Debug($"PlayerInit: {clientId} Sending Player to all. size:{playerMessage.WrittenLength}");
            SFServer.Server.SendToAll(playerMessage, clientId);
        }

        private static void SendChunk(ushort clientId, KeyValuePair<Chunk, List<NetworkChunkedEntity>> chunk)
        {
            List<NetworkChunkedEntity> toSend = new List<NetworkChunkedEntity>();
            int bytesToSend = 0;
            Message chunkDataMessage;
            foreach (var entity in chunk.Value)
            {
                if (entity.SizeInBytes() >= Message.MaxPayloadSize - sizeof(int)) // when the item can't fit one message
                {
                    throw new Exception($"Size of the {entity.GetType()} can't fit the network message. " +
                                        $"Size in bytes:{entity.SizeInBytes()} max payload:{Message.MaxPayloadSize - sizeof(int)}");
                }

                bytesToSend += entity.SizeInBytes();
                if (bytesToSend <= Message.MaxPayloadSize)
                {
                    toSend.Add(entity);
                }
                else
                {
                    chunkDataMessage = Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnMultiple).AddSerializables(toSend.ToArray());
                    LOGGER.Debug($"PlayerInit: {clientId} Sending Chunk size:{chunkDataMessage.WrittenLength}");
                    SFServer.Server.Send(chunkDataMessage, clientId);

                    bytesToSend = entity.SizeInBytes();
                    toSend = new List<NetworkChunkedEntity>();
                    toSend.Add(entity);
                }
            }

            chunkDataMessage = Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnMultiple).AddSerializables(toSend.ToArray());
            LOGGER.Debug($"PlayerInit: {clientId} Sending Chunk size:{chunkDataMessage.WrittenLength}");
            SFServer.Server.Send(chunkDataMessage, clientId);
        }

        private static void SendOtherPlayers(ushort clientId)
        {
            Message spawnOtherPlayerMessage;
            var otherPlayersToSend = new List<NetworkOtherPlayer>();
            foreach (var otherPlayer in SFServer.World.Players.Values)
            {
                if (otherPlayer.SizeInBytes() >= Message.MaxPayloadSize - sizeof(int)) // when the item can't fit one message
                {
                    throw new Exception($"Size of the {otherPlayer.GetType()} can't fit the network message. " +
                                        $"Size in bytes:{otherPlayer.SizeInBytes()} max payload:{Message.MaxPayloadSize - sizeof(int)}");
                }

                int bytesToSend = 0;
                bytesToSend += otherPlayer.SizeInBytes();
                if (bytesToSend <= Message.MaxPayloadSize)
                {
                    otherPlayersToSend.Add(otherPlayer);
                }
                else
                {
                    spawnOtherPlayerMessage = Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnOtherPlayers).AddSerializables(otherPlayersToSend.ToArray());
                    LOGGER.Debug($"PlayerInit: {clientId} Sending Other Players size:{spawnOtherPlayerMessage.WrittenLength}");
                    SFServer.Server.Send(spawnOtherPlayerMessage, clientId);
                }
            }

            spawnOtherPlayerMessage = Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnOtherPlayers).AddSerializables(otherPlayersToSend.ToArray());
            LOGGER.Debug($"PlayerInit: {clientId} Sending Other Players size:{spawnOtherPlayerMessage.WrittenLength}");
            SFServer.Server.Send(spawnOtherPlayerMessage, clientId);
        }
        
        [MessageHandler((ushort)ClientToServerId.PlayerUpdate)]
        private static void PlayerUpdate(ushort clientId, Message message)
        {
            var playerInput = message.GetSerializable<NetworkPlayerMovementInput>();
            if (!SFServer.World.Players.ContainsKey(clientId))
            {
                throw new Exception($"Player movement for player not in game");
            }

            var player = SFServer.World.Players[clientId];
            player.Chunk = playerInput.Chunk;
            player.Position = playerInput.Position;
            player.Rotation = playerInput.Rotation;

            //todo more validation here note that player may switch chunk
            playerInput.SessionId = clientId;

            Message toAllMessage = Message.Create(MessageSendMode.Unreliable,ServerToClientId.Movement);
            toAllMessage.AddSerializable(playerInput);
            LOGGER.Debug($"PlayerUpdate: {clientId} {playerInput.ToString()}");
            SFServer.Server.SendToAll(toAllMessage, clientId);

        }
    }
}
