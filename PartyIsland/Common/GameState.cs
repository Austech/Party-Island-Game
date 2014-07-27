using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public abstract class GameState
    {
        public GameState()
        {
        }

        public abstract void Update(float dt);
    }
}
