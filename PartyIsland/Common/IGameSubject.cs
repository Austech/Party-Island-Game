using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public delegate void NotificationDelegate(GameEvent ge);
    public interface IGameSubject
    {
        void AddObserver(NotificationDelegate callback);
        void RemoveObserver(NotificationDelegate callback);

        void Notify(GameEvent ge);
    }
}
