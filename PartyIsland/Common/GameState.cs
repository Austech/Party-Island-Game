using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public abstract class GameState: EventReceiver
    {
        public int PlayerCount
        {
            get;
            set;
        }

        public EventDispatcher EDispatcher
        {
            get;
            set;
        }

        public GameState()
        {
            PlayerCount = 0;
            EDispatcher = null;
        }

        public abstract void Update(float dt);

        public virtual void HandleEvent(Event ev)
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
