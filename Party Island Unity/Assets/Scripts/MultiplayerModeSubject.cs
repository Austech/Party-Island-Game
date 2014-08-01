using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace Assets.Scripts
{
    public class MultiplayerModeSubject: PlayMode
    {
        private Common.Events.GameInput inputEvent;

        public MultiplayerModeSubject(string ip, int port)
        {
            PartyIsland.GlobalVariables.Initialize();
            PartyIsland.GlobalVariables.Client.Connect(ip, port);
        }

        public override void AddObserver(NotificationDelegate callback)
        {
            PartyIsland.GlobalVariables.Client.AddObserver(callback);
        }

        public override void RemoveObserver(NotificationDelegate callback)
        {
            PartyIsland.GlobalVariables.Client.RemoveObserver(callback);
        }

        public override void Notify(GameEvent ge)
        {
            PartyIsland.GlobalVariables.Client.Notify(ge);
        }

        public override void HandleEvent(GameEvent ge)
        {
            PartyIsland.GlobalVariables.Client.HandleEvent(ge);
        }

        public override void Update()
        {
            PartyIsland.GlobalVariables.Client.Update();
        }
    }
}
