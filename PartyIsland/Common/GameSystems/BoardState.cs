using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Common.GameStates
{
    public class BoardSystem: IGameSystem, IGameObserver, IGameSubject
    {
        public Board BoardData;

        private int characterMoveDelay; //Time in milliseconds it takes for each tile movement
        private DefaultGameSubject defaultGameSubject;
        private Random random;  //used for rolling and choosing minigames
        private int timeCounter;

        public BoardSystem(Board board, int moveDelay)
        {
            defaultGameSubject = new DefaultGameSubject();
            characterMoveDelay = moveDelay;
            random = new Random();
            BoardData = board;
            timeCounter = 0;
        }

        public void Update(int frameTime)
        {
            UpdateBoard(BoardData, frameTime);
        }

        private void UpdateBoard(Board board, int frameTime)
        {
            timeCounter += frameTime;

            switch (board.State)
            {
                case Board.BoardStates.PLAYER_MOVING:
                    //Is the player out of movement?
                    if (board.RollValue == 0)
                    {
                        //Switch to the next player
                        board.RollValue = -1;

                        var currentCharacter = board.Characters[board.CurrentPlayer];

                        currentCharacter.LandedTileType = board.TileMap[currentCharacter.X, currentCharacter.Y].Type;
                        Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_LANDED, new byte[3] { board.CurrentPlayer, (byte)currentCharacter.X, (byte)currentCharacter.Y }));

                        board.CurrentPlayer++;

                        //Are we out of players to go to?
                        if (board.CurrentPlayer >= board.Characters.Count)
                        {
                            board.State = Board.BoardStates.CHOOSING_MINIGAME;
                            byte miniGameId = (byte)random.Next(0, 255);
                            Notify(new GameEvent(GameEvent.EventTypes.MINIGAME_SELECTED, new byte[1] { miniGameId }));
                            board.State = Board.BoardStates.END;
                        }
                        else
                        {
                            board.State = Board.BoardStates.WAITING_FOR_ROLL;
                        }
                    }
                    else
                    {
                        if (timeCounter >= characterMoveDelay)
                        {
                            timeCounter = 0;
                            //Move Character
                            var currentCharacter = board.Characters[board.CurrentPlayer];
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

                                Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_MOVED, new byte[3] { board.CurrentPlayer, (byte)currentCharacter.X, (byte)currentCharacter.Y }));

                                if (board.TileMap[currentCharacter.X, currentCharacter.Y].Type == Tile.TileTypes.FORK)
                                {
                                    byte[] data = new byte[5] { board.CurrentPlayer, 0, 0, 0, 0 };
                                    board.ChosenDirection = board.TileMap[currentCharacter.X, currentCharacter.Y].Forks[0];
                                    Notify(new Events.BoardPlayerChoosingDirectionOptions(board.TileMap[currentCharacter.X, currentCharacter.Y], board.CurrentPlayer));
                                    board.State = Board.BoardStates.PLAYER_CHOOSING_FORK;
                                }
                                else
                                {
                                    //Only decrease the roll value if the player is not on a fork
                                    board.RollValue--;
                                }
                            }
                            else
                            {
                                board.RollValue = 0;
                            }
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
                    if (inputEvent.PlayerId != BoardData.CurrentPlayer)
                        break;

                    if (inputEvent.GetInput(Events.GameInput.InputTypes.PRIMARY))
                    {
                        switch (BoardData.State)
                        {
                            case Board.BoardStates.WAITING_FOR_ROLL:
                                Roll();
                                break;
                            case Board.BoardStates.PLAYER_CHOOSING_FORK:
                                var tile = BoardData.TileMap[BoardData.Characters[BoardData.CurrentPlayer].X, BoardData.Characters[BoardData.CurrentPlayer].Y];

                                if (tile.Forks.Contains(BoardData.ChosenDirection))
                                {
                                    //Choose Fork
                                    BoardData.Characters[BoardData.CurrentPlayer].Facing = BoardData.ChosenDirection;
                                    BoardData.State = Board.BoardStates.PLAYER_MOVING;
                                    Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_PLAYER_SELECTED_DIRECTION, new byte[2] { BoardData.CurrentPlayer, (byte)BoardData.Characters[BoardData.CurrentPlayer].Facing }));
                                }
                                break;
                        }
                    }
                    switch (BoardData.State)
                    {
                        case Board.BoardStates.PLAYER_CHOOSING_FORK:
                            var tile = BoardData.TileMap[BoardData.Characters[BoardData.CurrentPlayer].X, BoardData.Characters[BoardData.CurrentPlayer].Y];
                            var facingPosition = BoardCharacter.FacingDirections.LEFT;

                            if (inputEvent.GetInput(Events.GameInput.InputTypes.DOWN))
                                facingPosition = BoardCharacter.FacingDirections.DOWN;
                            if (inputEvent.GetInput(Events.GameInput.InputTypes.UP))
                                facingPosition = BoardCharacter.FacingDirections.UP;
                            if (inputEvent.GetInput(Events.GameInput.InputTypes.LEFT))
                                facingPosition = BoardCharacter.FacingDirections.LEFT;
                            if (inputEvent.GetInput(Events.GameInput.InputTypes.RIGHT))
                                facingPosition = BoardCharacter.FacingDirections.RIGHT;

                            if (tile.Forks.Contains(facingPosition))
                            {
                                BoardData.ChosenDirection = facingPosition;
                                Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_CHANGED_DIRECTION_SELECTION, new byte[2] { BoardData.CurrentPlayer, (byte)BoardData.ChosenDirection }));
                            }
                            break;
                    }
                    break;

                case GameEvent.EventTypes.BOARD_ENCODE_REQUEST:
                    Notify(new GameEvent(GameEvent.EventTypes.BOARD_ENCODE_RESPONSE, BoardData.Encode()));
                    break;
            }
        }

        public void Roll()
        {
            BoardData.RollValue = random.Next(1, 11);
            Notify(new GameEvent(GameEvent.EventTypes.BOARD_PLAYER_ROLLED, new byte[2] { BoardData.CurrentPlayer, (byte)BoardData.RollValue }));

            BoardData.State = Board.BoardStates.PLAYER_MOVING;
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

            for (var i = 0; i < 4; i++)
            {
                BoardCharacter.FacingDirections direction = (BoardCharacter.FacingDirections)i;

                var offsetList = GetTileOffset(direction).ToList();

                if (x + offsetList[0] < 0 || x + offsetList[0] >= BoardData.TileMap.GetLength(0))
                    continue;
                if (y + offsetList[1] < 0 || y + offsetList[1] >= BoardData.TileMap.GetLength(1))
                    continue;
                if (BoardData.TileMap[x + offsetList[0], y + offsetList[1]].Type == Tile.TileTypes.NONE)
                    continue;
                if (BoardCharacter.GetOppositeDirection(direction) == character.Facing)
                    continue;

                yield return direction;
            }
        }
    }
}
