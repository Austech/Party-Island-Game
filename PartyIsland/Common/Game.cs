using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Game
    {
        GameState currentState;

        public Game()
        {
            currentState = null;
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
            currentState = gs;
        }
    }
}
