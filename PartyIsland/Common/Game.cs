using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Game: EventReceiver
    {
        EventDispatcher eDispatcher;
        GameState currentState;

        public int PlayerCount
        {
            get;
            private set;
        }

        public EventDispatcher EDispatcher
        {
            get
            {
                return eDispatcher;
            }
            private set
            {
                eDispatcher = value;
            }
        }

        public Game()
        {
            eDispatcher = new EventDispatcher();
            eDispatcher.RegisterReceiver(this);

            PlayerCount = 0;
        }

        public void Update(float dt)
        {
            if (currentState != null)
            {
                currentState.Update(dt);
            }
        }

        public void SetGameState(GameState gs)
        {
            if (currentState != null)
            {
                eDispatcher.UnRegisterReceiver(currentState);
            }

            currentState = gs;
            currentState.EDispatcher = eDispatcher;
            eDispatcher.RegisterReceiver(currentState);
            
            currentState.PlayerCount = PlayerCount;
        }

        public void HandleEvent(Event ev)
        {
            switch (ev.Type)
            {
                case Event.EventTypes.PLAYER_JOINED:
                    PlayerCount++;
                    break;
            }
        }
    }
}
