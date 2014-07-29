using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace Assets.Scripts
{
    public abstract class PlayMode: IGameObserver, IGameSubject
    {
        public abstract void Update();
        public abstract void AddObserver(NotificationDelegate callback);
        public abstract void RemoveObserver(NotificationDelegate callback);
        public abstract void Notify(GameEvent ge);
        public abstract void HandleEvent(GameEvent ge);
    }
}
