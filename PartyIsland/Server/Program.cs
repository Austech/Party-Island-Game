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
        private class DebugEventListener: EventReceiver
        {
            public DebugEventListener(EventDispatcher eDispatcher)
            {
                eDispatcher.RegisterReceiver(this);
            }

            public void HandleEvent(Event ev)
            {
                Console.WriteLine("EVENT: " + ev.Type + "; " + BitConverter.ToString(ev.Data));
            }
        }

        static EventDispatcher eDispatcher = null;

        private static void InputThread()
        {
            eDispatcher.Dispatch(new Event(Event.EventTypes.PLAYER_JOINED, new byte[0] { }));
            Common.Events.GameInput inputEvent = new Common.Events.GameInput(0);
            while (true)
            {
                string input = Console.ReadLine();

                if (input == "primary")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.PRIMARY, true);
                    eDispatcher.Dispatch(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.PRIMARY, false);
                    eDispatcher.Dispatch(inputEvent);
                }
                else if (input == "left")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.LEFT, true);
                    eDispatcher.Dispatch(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.LEFT, false);
                    eDispatcher.Dispatch(inputEvent);
                }
                else if (input == "right")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.RIGHT, true);
                    eDispatcher.Dispatch(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.RIGHT, false);
                    eDispatcher.Dispatch(inputEvent);
                }
                else if (input == "up")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.UP, true);
                    eDispatcher.Dispatch(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.UP, false);
                    eDispatcher.Dispatch(inputEvent);
                }
                else if (input == "down")
                {
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.DOWN, true);
                    eDispatcher.Dispatch(inputEvent);
                    inputEvent.SetInput(Common.Events.GameInput.InputTypes.DOWN, false);
                    eDispatcher.Dispatch(inputEvent);
                }
            }
        }

        static void Main(string[] args)
        {
            Game game = new Game();
            game.SetGameState(new Common.GameStates.CharacterSelection());

            eDispatcher = game.EDispatcher;

            DebugEventListener debugEvent = new DebugEventListener(eDispatcher);

            Board board = new Board(eDispatcher, Tile.TileMapFromStream(new StreamReader(File.OpenRead("testmap.txt"))));
            board.Characters.Add(new BoardCharacter());
            board.Characters[0].Facing = BoardCharacter.FacingDirections.DOWN;
            
            GameServer server = new GameServer(eDispatcher);
            server.Start();

            Thread inputThread = new Thread(new ThreadStart(InputThread));
            inputThread.Start();
            
            for (; ; )
            {
                game.Update(0);
                server.Update();
            }
        }
    }
}
