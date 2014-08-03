using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common
{
    public class Board : IStateEncodable
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
        public byte CurrentPlayer;
        public int RollValue;
        public List<BoardCharacter> Characters;
        public Tile[,] TileMap;
        public BoardCharacter.FacingDirections ChosenDirection; //direction the character is choosing for forks

        public Board(Tile[,] map)
        {
            ChosenDirection = BoardCharacter.FacingDirections.UP;
            State = BoardStates.WAITING_FOR_ROLL;
            CurrentPlayer = 0;
            RollValue = -1;
            Characters = new List<BoardCharacter>();
            TileMap = map;
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
