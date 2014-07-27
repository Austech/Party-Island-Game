using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Events
{
    public class GameInput : GameEvent
    {
        const int INPUT_COUNT = 6;
        public enum InputTypes
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            PRIMARY,
            SECONDARY,
        }

        public bool[] InputStates;  //true if the input is activiate, false if deactivated

        public byte PlayerId;

        public GameInput(byte playerId)
            : base(EventTypes.INPUT, new byte[INPUT_COUNT + 1] { 0, 0, 0, 0, 0, 0, 0 })
        {
            PlayerId = playerId;
            InputStates = new bool[INPUT_COUNT];
        }

        public GameInput(byte[] data)
            :base(EventTypes.INPUT, data)
        {
            if (Data == null)
                return;
            PlayerId = Data[0];
            InputStates = new bool[INPUT_COUNT];
            for (var i = 1; i < INPUT_COUNT + 1; i++)
            {
                if (Data[i] == 1)
                    InputStates[i - 1] = true;
                else
                    InputStates[i - 1] = false;
            }
        }

        /// <summary>
        /// Sets an input activation value
        /// returns true if a value was changed
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetInput(InputTypes type, bool value)
        {
            if (InputStates[(int)type] != value)
            {
                InputStates[(int)type] = value;
                UpdateByteArray();
                return true;
            }
            return false;
        }

        public bool GetInput(InputTypes type)
        {
            return InputStates[(int)type];
        }

        public void UpdateByteArray()
        {
            Data = new byte[INPUT_COUNT + 1];
            Data[0] = PlayerId;
            for (var i = 1; i < INPUT_COUNT + 1; i++)
            {
                if (InputStates[i - 1])
                {
                    Data[i] = 1;
                }
                else
                {
                    Data[i] = 0;
                }
            }
        }
    }
}
