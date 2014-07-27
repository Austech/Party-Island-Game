using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Lidgren.Network;

namespace PartyIsland
{
    public class GameClient : IGameObserver, IGameSubject
    {
        NetClient client;

        private event Common.NotificationDelegate observers;

        public GameClient()
        {
            client = new NetClient(new NetPeerConfiguration("PARTY_ISLAND"));
        }

        public void Connect(String ip, int port)
        {
            client.Start();
            client.Connect(ip, port);
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
                        {
                            if (eventFromMessage.Type == GameEvent.EventTypes.INPUT)
                            {
                                Notify(GameEvent.GetDetailedEvent<Common.Events.GameInput>(eventFromMessage));
                            }
                            else
                            {
                                Notify(eventFromMessage);
                            }
                        }
                        break;
                }
            }
        }

        public void AddObserver(NotificationDelegate callback)
        {
            observers += callback;
        }

        public void RemoveObserver(NotificationDelegate callback)
        {
            observers -= callback;
        }

        public void Notify(GameEvent ge)
        {
            if (observers != null)
                observers(ge);
        }

        public void HandleEvent(GameEvent ev)
        {
            switch (ev.Type)
            {
                case GameEvent.EventTypes.INPUT:
                    if (ev.Sender == "Network")  //Dont send inputs from the network back to the network
                        break;

                    var outMsg = MessageFromEvent(ev);
                    client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);
                    
                    break;
            }
        }

        private GameEvent EventFromMessage(NetIncomingMessage msg)
        {
            GameEvent.EventTypes type = (GameEvent.EventTypes)msg.ReadByte();
            var data = msg.ReadBytes(msg.LengthBytes - 1);

            return new GameEvent(type, data, "Network");
        }

        private NetOutgoingMessage MessageFromEvent(GameEvent ev)
        {
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((byte)ev.Type);
            msg.Write(ev.Data);

            return msg;
        }
    }
}
