using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Lidgren.Network;

namespace Server
{
    class GameServer : IGameObserver, IGameSubject
    {
        private NetServer server;

        /// <summary>
        /// Used ids for connected clients
        /// This is used for assinging a unique id to a client
        /// </summary>
        private List<int> connectedIds; //used ids for connected clients

        private event Common.NotificationDelegate observers;

        public GameServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("PARTY_ISLAND");
            config.Port = 1337;
            server = new NetServer(config);

            connectedIds = new List<int>();
        }

        public void Start()
        {
            server.Start();
        }

        /// <summary>
        /// Get the smallest unique ID
        /// </summary>
        /// <returns>Returns an id</returns>
        public int GetUniqueId()
        {
            var loop = true;
            for (var i = 0; loop; i++)
            {
                loop = false;
                for (var j = 0; j < connectedIds.Count; j++)
                {
                    if (connectedIds[j] == i)
                    {
                        loop = true;
                    }
                }
                if (loop == false)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Update()
        {
            NetIncomingMessage msg = null;

            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        switch (msg.SenderConnection.Status)
                        {
                            case NetConnectionStatus.Connected:
                                Notify(new GameEvent(GameEvent.EventTypes.PLAYER_JOINED, new byte[0] { }));
                                var uniqueId = GetUniqueId();
                                connectedIds.Add(uniqueId);
                                msg.SenderConnection.Tag = uniqueId;

                                Notify(new GameEvent(GameEvent.EventTypes.PLAYER_COUNT, new byte[1] { (byte)connectedIds.Count }));
                                break;
                            case NetConnectionStatus.Disconnected:
                                connectedIds.Remove((int)msg.SenderConnection.Tag);
                                break;
                            default:
                                break;
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        var incomingEvent = EventFromMessage(msg);
                        if (incomingEvent != null)
                        {
                            if (incomingEvent.Type == GameEvent.EventTypes.INPUT)
                            {
                                var inputEvent = GameEvent.GetDetailedEvent<Common.Events.GameInput>(incomingEvent);
                                inputEvent.PlayerId = (byte)((int)msg.SenderConnection.Tag);
                                inputEvent.UpdateByteArray();
                                Notify(inputEvent);
                            }
                            else if(incomingEvent.Sender != "Network")
                            {
                                Notify(incomingEvent);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void Subscribe(NotificationDelegate callback)
        {
            observers += callback;
        }

        public void Unsubscribe(NotificationDelegate callback)
        {
            observers -= callback;
        }

        public void Notify(GameEvent ge)
        {
            observers(ge);
        }

        public void HandleEvent(GameEvent ev)
        {
            server.SendToAll(MessageFromEvent(ev), NetDeliveryMethod.ReliableOrdered);
        }

        private GameEvent EventFromMessage(NetIncomingMessage msg)
        {
            GameEvent.EventTypes type = (GameEvent.EventTypes)msg.ReadByte();
            var data = msg.ReadBytes(msg.LengthBytes - 1);

            return new GameEvent(type, data, "Network");
        }

        private NetOutgoingMessage MessageFromEvent(GameEvent ev)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((byte)ev.Type);
            msg.Write(ev.Data);

            return msg;
        }
    }
}
