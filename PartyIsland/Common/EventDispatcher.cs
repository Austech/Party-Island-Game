using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class EventDispatcher
    {
        public class EventObserver
        {
            public EventReceiver Receiver;
            public List<String> BlacklistedSenders;

            public EventObserver(EventReceiver rec)
            {
                Receiver = rec;
                BlacklistedSenders = new List<String>();
            }
        }
        private List<EventObserver> handlers;

        public EventDispatcher()
        {
            handlers = new List<EventObserver>();
        }

        public void Dispatch(Event e)
        {
            for (var i = 0; i < handlers.Count; i++)
            {
                bool canHandleEvent = true;
                for (var j = 0; j < handlers[i].BlacklistedSenders.Count; j++)
                {
                    if (e.Sender == handlers[i].BlacklistedSenders[j])
                    {
                        canHandleEvent = false;
                        break;
                    }
                }
                if(canHandleEvent)
                    handlers[i].Receiver.HandleEvent(e);
            }
        }

        public EventObserver RegisterReceiver(EventReceiver er)
        {
            var observer = new EventObserver(er);
            handlers.Add(observer);
            return observer;
        }

        public void UnRegisterReceiver(EventReceiver er)
        {
            for (var i = 0; i < handlers.Count; i++)
            {
                if (handlers[i].Receiver == er)
                {
                    handlers.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
