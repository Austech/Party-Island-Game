using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class GameEvent
    {
        public enum EventTypes: byte
        {
            /// <summary>
            /// Game Input
            /// Contains 7 bytes of data
            /// 0: Player ID of inputter
            /// 1-6: Input values
            /// </summary>
            INPUT,

            /// <summary>
            /// Called when a player joins
            /// </summary>
            PLAYER_JOINED,

            /// <summary>
            /// Number of players in game
            /// 1 byte of data
            /// 0: Number of players (0-255)
            /// </summary>
            PLAYER_COUNT,

            /// <summary>
            /// 1 byte determining the minigame id (0-255)
            /// </summary>
            MINIGAME_SELECTED,

            /// <summary>
            /// 1 byte representing the new selection
            /// </summary>
            CHARACTERSELECT_PLAYER_CHANGED_SELECTION,
            
            /// <summary>
            /// 1 byte representing the new selection
            /// </summary>
            CHARACTERSELECT_PLAYER_CONFIRMED_SELECTION,

            /// <summary>
            ///Contains 3 bytes of data
            ///0: Player ID
            ///1: X Position
            ///2: Y Position
            /// </summary>
            BOARD_PLAYER_MOVED,
            
            /// <summary>
            ///Contains 3 bytes of data
            ///0: Player ID
            ///1: X Position
            ///2: Y Position
            /// </summary>
            BOARD_PLAYER_LANDED,

            /// <summary>
            /// Contains 2 bytes of data
            /// 0: Player ID
            /// 1: Roll Value
            /// </summary>
            BOARD_PLAYER_ROLLED,

            /// <summary>
            /// Contains 5 byte of data
            /// 0: Player ID (0 -255)
            /// 1-4: Direction Availability (1 if the fork in the direction is available, 0 if the fork does not exist)
            /// </summary>
            BOARD_PLAYER_CHOOSING_DIRECTION_OPTIONS,

            /// <summary>
            /// Contains 2 bytes of data
            /// 0: Player ID (0 -255)
            /// 1: Player Direction (0-3)
            /// </summary>
            BOARD_PLAYER_CHANGED_DIRECTION_SELECTION,

            /// <summary>
            /// Contains 2 bytes of data
            /// 0: Player ID (0 -255)
            /// 1: Player Direction (0-3)
            /// </summary>
            BOARD_PLAYER_PLAYER_SELECTED_DIRECTION,
        }

        public EventTypes Type;
        public byte[] Data;
        public String Sender;

        public GameEvent(EventTypes type, byte[] data)
        {
            Sender = "";
            Type = type;
            Data = data;
        }
        public GameEvent(EventTypes type, byte[] data, String sender)
        {
            Sender = sender;
            Type = type;
            Data = data;
        }

        public static T GetDetailedEvent<T>(GameEvent ev) where T : GameEvent
        {
            switch (ev.Type)
            {
                case EventTypes.INPUT:
                    var ret = new Events.GameInput(ev.Data);
                    ret.Sender = ev.Sender;
                    return (T)((GameEvent)ret);
                default:
                    return (T)ev;
            }
        }
    }
}
