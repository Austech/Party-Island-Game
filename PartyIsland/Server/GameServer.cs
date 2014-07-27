using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Lidgren.Network;

namespace Server
{
    class GameServer : EventReceiver
    {
        NetServer server;
        EventDispatcher eDispatcher;

        /// <summary>
        /// Used ids for connected clients
        /// This is used for assinging a unique id to a client
        /// </summary>
        List<int> connectedIds; //used ids for connected clients 

        public GameServer(EventDispatcher dispatcher)
        {
            eDispatcher = dispatcher;
            eDispatcher.RegisterReceiver(this);

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
                                eDispatcher.Dispatch(new Event(Event.EventTypes.PLAYER_JOINED, new byte[0] { }));
                                var uniqueId = GetUniqueId();
                                connectedIds.Add(uniqueId);
                                msg.SenderConnection.Tag = uniqueId;
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
                            if (incomingEvent.Type == Event.EventTypes.INPUT)
                            {
                                var inputEvent = Event.GetDetailedEvent<Common.Events.GameInput>(incomingEvent);
                                inputEvent.PlayerId = (byte)((int)msg.SenderConnection.Tag);
                                inputEvent.UpdateByteArray();
                                eDispatcher.Dispatch(inputEvent);
                            }
                            else if(incomingEvent.Sender != "Network")
                            {
                                eDispatcher.Dispatch(incomingEvent);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void HandleEvent(Event ev)
        {
            server.SendToAll(MessageFromEvent(ev), NetDeliveryMethod.ReliableOrdered);
        }

        private Event EventFromMessage(NetIncomingMessage msg)
        {
            Event.EventTypes type = (Event.EventTypes)msg.ReadByte();
            var data = msg.ReadBytes(msg.LengthBytes - 1);

            return new Event(type, data, "Network");
        }

        private NetOutgoingMessage MessageFromEvent(Event ev)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((byte)ev.Type);
            msg.Write(ev.Data);

            return msg;
        }
    }
}
