using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Lidgren.Network;
using System.Diagnostics;

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

        private DefaultGameSubject defaultGameSubject;

        /// <summary>
        /// Milliseconds until next tick.
        /// The server sends 'snapshots' of key elements of the game every tick.
        /// </summary>
        public int TickRate
        {
            get;
            set;
        }

        Stopwatch tickWatch;

        public GameServer(int tickrate)
        {
            defaultGameSubject = new DefaultGameSubject();

            TickRate = tickrate;
            tickWatch = new Stopwatch();

            NetPeerConfiguration config = new NetPeerConfiguration("PARTY_ISLAND");
            config.Port = 1337;
            server = new NetServer(config);

            connectedIds = new List<int>();
        }

        public void Start()
        {
            tickWatch.Start();
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

                                server.SendMessage(MessageFromEvent(new GameEvent(GameEvent.EventTypes.PLAYER_ID_RESPONSE, new byte[1] { (byte)uniqueId })), msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

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
                        }
                        break;
                    default:
                        break;
                }
            }

            if (tickWatch.ElapsedMilliseconds >= TickRate)
            {
                tickWatch.Reset();
                tickWatch.Start();

                Notify(new GameEvent(GameEvent.EventTypes.CHARACTERSELECT_ENCODE_REQUEST, new byte[0]));
                Notify(new GameEvent(GameEvent.EventTypes.BOARD_ENCODE_REQUEST, new byte[0]));
            }
        }

        public void AddObserver(NotificationDelegate callback)
        {
            defaultGameSubject.AddObserver(callback);
        }

        public void RemoveObserver(NotificationDelegate callback)
        {
            defaultGameSubject.RemoveObserver(callback);
        }

        public void Notify(GameEvent ge)
        {
            defaultGameSubject.Notify(ge);
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
