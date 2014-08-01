using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class DefaultGameSubject: IGameSubject
    {
        private event NotificationDelegate observers;

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
    }
}
