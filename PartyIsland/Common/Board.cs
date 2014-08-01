using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common
{
    public class Board : IGameSubject, IGameObserver, IStateEncodable
    {
        public enum BoardStates: byte
        {
            WAITING_FOR_ROLL,
            PLAYER_MOVING,
            /// <summary>
            /// Wait for player input to choose what path to choose
            /// </summary>
            PLAYER_CHOOSING_FORK,
            CHOOSING_MINIGAME,
            END,
        }

        public BoardStates State;

        public byte CurrentPlayer
        {
            get;
            private set;
        }
        public int RollValue
        {
            get;
            private set;
        }

        public List<BoardCharacter> Characters;

        private Random random;  //used for rolling and choosing minigames

        public Tile[,] TileMap;

        public BoardCharacter.FacingDirections ChosenDirection; //direction the character is choosing for forks

        private DefaultGameSubject defaultGameSubject;

        public Board(Tile[,] map)
        {
            defaultGameSubject = new DefaultGameSubject();

            ChosenDirection = BoardCharacter.FacingDirections.UP;

            State = BoardStates.WAITING_FOR_ROLL;
            CurrentPlayer = 0;
            RollValue = -1;
            Characters = new List<BoardCharacter>();
            TileMap = map;
            random = new Random();
        }

        public void Update()
        {
            switch (State)
            {
                case BoardStates.PLAYER_MOVING:
                    //Is the player out of movement?
                    if (RollValue == 0)
                    {
                        //Switch to the next player
                        RollValue = -1;

                        var currentCharacter = Characters[CurrentPlayer];

                        currentCharacter.LandedTileType = TileMap[currentCharacter.X, currentCharacter.Y].Type;
                        Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_LANDED, new byte[3] { CurrentPlayer, (byte)currentCharacter.X, (byte)currentCharacter.Y }));

                        CurrentPlayer++;

                        //Are we out of players to go to?
                        if (CurrentPlayer >= Characters.Count)
                        {
                            State = BoardStates.CHOOSING_MINIGAME;
                            byte miniGameId = (byte)random.Next(0, 255);
                            Notify(new GameEvent(GameEvent.EventTypes.MINIGAME_SELECTED, new byte[1] { miniGameId }));
                            State = BoardStates.END;
                        }
                        else
                        {
                            State = BoardStates.WAITING_FOR_ROLL;
                        }
                    }
                    else
                    {
                        //Move player

                        var currentCharacter = Characters[CurrentPlayer];
                        var allowedMovement = CanWalk(currentCharacter).ToList();
                        
                        if (!allowedMovement.Contains(currentCharacter.Facing) && allowedMovement.Count > 0)
                        {
                            currentCharacter.Facing = allowedMovement[0];
                        }

                        if (allowedMovement.Contains(currentCharacter.Facing))
                        {
                            var offset = GetTileOffset(currentCharacter.Facing).ToArray();

                            currentCharacter.X += offset[0];
                            currentCharacter.Y += offset[1];

                            Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_MOVED, new byte[3] { CurrentPlayer, (byte)currentCharacter.X, (byte)currentCharacter.Y }));

                            if (TileMap[currentCharacter.X, currentCharacter.Y].Type == Tile.TileTypes.FORK)
                            {
                                byte[] data = new byte[5] { CurrentPlayer, 0, 0, 0, 0 };
                                ChosenDirection = TileMap[currentCharacter.X, currentCharacter.Y].Forks[0];
                                Notify(new Events.BoardPlayerChoosingDirectionOptions(TileMap[currentCharacter.X, currentCharacter.Y], CurrentPlayer));
                                State = BoardStates.PLAYER_CHOOSING_FORK;
                            }
                            else
                            {
                                //Only decrease the roll value if the player is not on a fork
                                RollValue--;
                            }
                        }
                        else
                        {
                            RollValue = 0;
                        }
                    }
                    break;
            }
        }

        public void AddObserver(NotificationDelegate callback)
        {
            defaultGameSubject.AddObserver(callback);
        }

        public void RemoveObserver(NotificationDelegate callback)
        {
            defaultGameSubject.RemoveObserver(callback);
        }

        public void Notify(GameEvent ge)
        {
            defaultGameSubject.Notify(ge);
        }

        public void HandleEvent(GameEvent ev)
        {
            switch (ev.Type)
            {
                case GameEvent.EventTypes.INPUT:
                    var inputEvent = (Events.GameInput)ev;
                    if (inputEvent.PlayerId != CurrentPlayer)
                        break;

                    if (inputEvent.GetInput(Events.GameInput.InputTypes.PRIMARY))
                    {
                        switch (State)
                        {
                            case BoardStates.WAITING_FOR_ROLL:
                                Roll();
                                break;
                            case BoardStates.PLAYER_CHOOSING_FORK:
                                var tile = TileMap[Characters[CurrentPlayer].X, Characters[CurrentPlayer].Y];

                                if (tile.Forks.Contains(ChosenDirection))
                                {
                                    //Choose Fork
                                    Characters[CurrentPlayer].Facing = ChosenDirection;
                                    State = BoardStates.PLAYER_MOVING;
                                    Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_PLAYER_SELECTED_DIRECTION, new byte[2] { CurrentPlayer, (byte)Characters[CurrentPlayer].Facing }));
                                }
                                break;
                        }
                    }
                    switch (State)
                    {
                        case BoardStates.PLAYER_CHOOSING_FORK:
                            var tile = TileMap[Characters[CurrentPlayer].X, Characters[CurrentPlayer].Y];
                            var facingPosition = BoardCharacter.FacingDirections.LEFT;

                            if(inputEvent.GetInput(Events.GameInput.InputTypes.DOWN))
                                    facingPosition = BoardCharacter.FacingDirections.DOWN;
                            if(inputEvent.GetInput(Events.GameInput.InputTypes.UP))
                                    facingPosition = BoardCharacter.FacingDirections.UP;
                            if(inputEvent.GetInput(Events.GameInput.InputTypes.LEFT))
                                    facingPosition = BoardCharacter.FacingDirections.LEFT;
                            if(inputEvent.GetInput(Events.GameInput.InputTypes.RIGHT))
                                    facingPosition = BoardCharacter.FacingDirections.RIGHT;

                            if (tile.Forks.Contains(facingPosition))
                            {
                                ChosenDirection = facingPosition;
                                Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_CHANGED_DIRECTION_SELECTION, new byte[2] { CurrentPlayer, (byte)ChosenDirection }));
                            }
                            break;
                    }
                    break;

                case GameEvent.EventTypes.BOARD_ENCODE_REQUEST:
                    Notify(new GameEvent(GameEvent.EventTypes.BOARD_ENCODE_RESPONSE, Encode()));
                    break;
            }
        }

        public void Roll()
        {
            RollValue = random.Next(1, 11);
            Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_ROLLED, new byte[2] { CurrentPlayer, (byte)RollValue }));

            State = BoardStates.PLAYER_MOVING;
        }

        /// <summary>
        ///Returns the tile offset based on direction
        ///IE: Direction.LEFT would returnf -1,0
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public IEnumerable<int> GetTileOffset(BoardCharacter.FacingDirections direction)
        {
            switch (direction)
            {
                case BoardCharacter.FacingDirections.LEFT:
                    yield return -1;
                    yield return 0;
                    break;
                case BoardCharacter.FacingDirections.RIGHT:
                    yield return 1;
                    yield return 0;
                    break;
                case BoardCharacter.FacingDirections.UP:
                    yield return 0;
                    yield return -1;
                    break;
                case BoardCharacter.FacingDirections.DOWN:
                    yield return 0;
                    yield return 1;
                    break;
            }
        }

        /// <summary>
        ///Returns availible tiles the player can move to
        ///Players are not allowed to move to tiles behind them
        ///IE: A player facing right cannot go to a tile to the left of them
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public IEnumerable<BoardCharacter.FacingDirections> CanWalk(BoardCharacter character)
        {
            var x = character.X;
            var y = character.Y;

            for(var i = 0; i < 4; i++)
            {
                BoardCharacter.FacingDirections direction = (BoardCharacter.FacingDirections)i;

                var offsetList = GetTileOffset(direction).ToList();

                if (x + offsetList[0] < 0 || x + offsetList[0] >= TileMap.GetLength(0))
                    continue;
                if (y + offsetList[1] < 0 || y + offsetList[1] >= TileMap.GetLength(1))
                    continue;
                if (TileMap[x + offsetList[0], y + offsetList[1]].Type == Tile.TileTypes.NONE)
                    continue;
                if (BoardCharacter.GetOppositeDirection(direction) == character.Facing)
                    continue;

                yield return direction;
            }
        }

        public byte[] Encode()
        {
            var memory = new MemoryStream();
            var writer = new BinaryWriter(memory);

            writer.Write((byte)State);
            writer.Write(CurrentPlayer);
            writer.Write(RollValue);

            writer.Write((short)TileMap.GetLength(0));
            writer.Write((short)TileMap.GetLength(1));

            for (var i = 0; i < TileMap.GetLength(0); i++)
            {
                for (var j = 0; j < TileMap.GetLength(1); j++)
                {
                    writer.Write(TileMap[i, j].Encode());
                }
            }

            writer.Write((byte)Characters.Count);

            for (var i = 0; i < Characters.Count; i++)
            {
                writer.Write(Characters[i].Encode());
            }

            

            writer.Close();
            memory.Close();
            return memory.ToArray();
        }

        public void Decode(BinaryReader reader)
        {
            State = (BoardStates)reader.ReadByte();
            CurrentPlayer = reader.ReadByte();
            RollValue = reader.ReadInt32();

            var x = reader.ReadInt16();
            var y = reader.ReadInt16();

            TileMap = new Tile[x, y];

            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    TileMap[i, j] = new Tile(i, j, Tile.TileTypes.BLUE);
                    TileMap[i, j].Decode(reader);
                }
            }

            Characters.Clear();
            var charactersCount = reader.ReadByte();
            for (var i = 0; i < charactersCount; i++)
            {
                var characterAdd = new BoardCharacter();
                characterAdd.Decode(reader);
                Characters.Add(characterAdd);
            }
        }
    }
}
