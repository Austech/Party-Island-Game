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
        public static Dictionary<string, IGameSubject> Subjects;
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

            public void Subscribe(NotificationDelegate callback)
            {
                observers += callback;
            }

            public void Unsubscribe(NotificationDelegate callback)
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
            DebugObserver.Notify(new GameEvent(GameEvent.EventTypes.PLAYER_JOINED, new byte[0] { }));
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

        static void Main(string[] args)
        {
            Subjects = new Dictionary<string, IGameSubject>();


            DebugObserver = new DebugEventListener();

            Board board = new Board(Tile.TileMapFromStream(new StreamReader(File.OpenRead("testmap.txt"))));
            board.Characters.Add(new BoardCharacter());
            board.Characters[0].Facing = BoardCharacter.FacingDirections.DOWN;

            Subjects.Add("Board", board);
            Subjects.Add("Debugger", DebugObserver);

            Subjects["Board"].Subscribe(DebugObserver.HandleEvent);
            Subjects["Debugger"].Subscribe(board.HandleEvent);
            Subjects["Debugger"].Subscribe(DebugObserver.HandleEvent);

            new Thread(new ThreadStart(InputThread)).Start();

            for (; ; )
            {
                board.Update();
            }
        }
    }
}
