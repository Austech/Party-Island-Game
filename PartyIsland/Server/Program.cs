using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using System.IO;

namespace Server
{
    class Program
    {
        private static DebugEventListener DebugObserver;

        public class DebugEventListener: IGameObserver, IGameSubject
        {
            private event Common.NotificationDelegate observers;

            public DebugEventListener()
            {
            }

            public void HandleEvent(GameEvent ev)
            {
                Console.WriteLine("EVENT: " + ev.Type + "; " + BitConverter.ToString(ev.Data));
            }

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
                if(observers != null)
                    observers(ge);
            }
        }

        private static void InputThread()
        {
            //DebugObserver.Notify(new GameEvent(GameEvent.EventTypes.PLAYER_JOINED, new byte[0] { }));
            Common.Events.GameInput inputEvent = new Common.Events.GameInput(0);
            while (true)
            {
                string input = Console.ReadLine();

                if (input == "primary")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.PRIMARY, true);
                    DebugObserver.Notify(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.PRIMARY, false);
                    DebugObserver.Notify(inputEvent);
                }
                else if (input == "left")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.LEFT, true);
                    DebugObserver.Notify(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.LEFT, false);
                    DebugObserver.Notify(inputEvent);
                }
                else if (input == "right")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.RIGHT, true);
                    DebugObserver.Notify(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.RIGHT, false);
                    DebugObserver.Notify(inputEvent);
                }
                else if (input == "up")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.UP, true);
                    DebugObserver.Notify(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.UP, false);
                    DebugObserver.Notify(inputEvent);
                }
                else if (input == "down")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.DOWN, true);
                    DebugObserver.Notify(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.DOWN, false);
                    DebugObserver.Notify(inputEvent);
                }
            }
        }

        private static Board board = new Board(new Tile[5, 5]);

        static void Main(string[] args)
        {

            DebugObserver = new DebugEventListener(); //Used for hand typed game events and logging events going through

            var server = new GameServer(500);
            var game = new Game();
            var characterSelection = new Common.GameStates.CharacterSelection();

            for (var i = 0; i < board.TileMap.GetLength(0); i++)
            {
                for (var j = 0; j < board.TileMap.GetLength(1); j++)
                {

                    if (j != 0 && i % j == 0)
                        board.TileMap[i, j] = new Tile(i, j, Tile.TileTypes.RED);
                    else
                    {
                        board.TileMap[i, j] = new Tile(i, j, Tile.TileTypes.BLUE);
                    }
                }
            }

            game.SetGameState(characterSelection);


            //In this  example, all observers/subjects listen to eachother.
            characterSelection.AddObserver(DebugObserver.HandleEvent);
            characterSelection.AddObserver(server.HandleEvent);

            server.AddObserver(DebugObserver.HandleEvent);
            server.AddObserver(characterSelection.HandleEvent);
            server.AddObserver(board.HandleEvent);
            server.AddObserver(HandleEvent);

            DebugObserver.AddObserver(server.HandleEvent);
            DebugObserver.AddObserver(characterSelection.HandleEvent);
            DebugObserver.AddObserver(board.HandleEvent);

            board.AddObserver(DebugObserver.HandleEvent);
            board.AddObserver(server.HandleEvent);

            server.Start();

            new Thread(new ThreadStart(InputThread)).Start();   //New thread for constant console input for manual events

            for (; ; )
            {
                board.Update();
                //game.Update(0);
                server.Update();

                Thread.Sleep(500);
            }
        }

        static void HandleEvent(GameEvent ev)
        {
            switch (ev.Type)
            {
                case GameEvent.EventTypes.PLAYER_JOINED:
                    board.Characters.Add(new BoardCharacter());
                    break;
            }
        }
    }
}
