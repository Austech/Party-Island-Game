using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Events
{
    public class BoardPlayerChoosingDirectionOptions: Event
    {
        byte[] AvailableForks;

        public BoardPlayerChoosingDirectionOptions(Tile tile, byte PlayerId)
            :base(EventTypes.BOARD_PLAYER_CHOOSING_DIRECTION_OPTIONS, null)
        {
            AvailableForks = new byte[4] { 0, 0, 0, 0 };
            for (var i = 0; i < tile.Forks.Length; i++)
            {
                AvailableForks[(byte)tile.Forks[i]] = 1;
            }

            Data = new byte[5] { PlayerId, AvailableForks[0], AvailableForks[1], AvailableForks[2], AvailableForks[3] };
        }
    }
}
