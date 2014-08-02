using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.GameStates
{
    public class BoardState: GameState
    {
        public Board BoardData;

        public BoardState()
        {

        }

        public override void Update(int dt)
        {
            BoardData.Update(dt);
        }
    }
}
