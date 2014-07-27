using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Lidgren.Network;

namespace PartyIsland
{
    public class GameClient : EventReceiver
    {
        NetClient client;
        public EventDispatcher EDispatcher
        {
            get;
            private set;
        }

        public GameClient()
        {
            client = new NetClient(new NetPeerConfiguration("PARTY_ISLAND"));
        }

        public void Connect(String ip, int port)
        {
            client.Start();
            client.Connect(ip, port);
        }

        public void SetDispatcher(EventDispatcher edispatcher)
        {
            EDispatcher = edispatcher;
            EDispatcher.RegisterReceiver(this);
        }

        public void Update()
        {
            NetIncomingMessage msg = null;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        var eventFromMessage = EventFromMessage(msg);
                        if (eventFromMessage != null)
                            EDispatcher.Dispatch(eventFromMessage);
                        break;
                }
            }
        }

        public void HandleEvent(Event ev)
        {
            UnityEngine.Debug.Log(ev.Type);
            switch (ev.Type)
            {
                case Event.EventTypes.INPUT:
                    if (ev.Sender == "Network")  //Dont send inputs from the network back to the network
                        break;

                    var outMsg = MessageFromEvent(ev);
                    client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
                    
                    break;
            }
        }

        private Event EventFromMessage(NetIncomingMessage msg)
        {
            Event.EventTypes type = (Event.EventTypes)msg.ReadByte();
            var data = msg.ReadBytes(msg.LengthBytes - 1);

            return new Event(type, data, "Network");
        }

        private NetOutgoingMessage MessageFromEvent(Event ev)
        {
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)ev.Type);
            msg.Write(ev.Data);

            return msg;
        }
    }
}
