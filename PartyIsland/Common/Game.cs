using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace Common
{
    public class Game
    {
        GameState currentState;

        /// <summary>
        /// Time in milliseconds of the delay per each update
        /// </summary>
        public int GameUpdateRate
        {
            get;
            set;
        }

        private Stopwatch gameUpdateWatch;
        private int deltaUpdateTime;

        public Game()
        {
            currentState = null;

            gameUpdateWatch = new Stopwatch();
            deltaUpdateTime = 0;

            GameUpdateRate = 1000 / 60; //60 updates per second
        }

        public void Start()
        {
            gameUpdateWatch.Reset();
            gameUpdateWatch.Start();
            deltaUpdateTime = 0;
        }

        public void Stop()
        {
            gameUpdateWatch.Stop();
        }

        public void Update()
        {
            deltaUpdateTime += (int)gameUpdateWatch.ElapsedMilliseconds;

            gameUpdateWatch.Reset();
            gameUpdateWatch.Stop();

            for (; deltaUpdateTime >= GameUpdateRate; deltaUpdateTime -= GameUpdateRate)
            {
                currentState.Update(GameUpdateRate);
            }
        }

        public void SetGameState(GameState gs)
        {
            currentState = gs;
        }
    }
}
