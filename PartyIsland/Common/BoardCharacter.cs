using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Common
{
    public class BoardCharacter: IStateEncodable
    {
        public enum FacingDirections: byte
        {
            UP = 0,
            DOWN = 1,
            LEFT = 2,
            RIGHT = 3,
        }
        public static FacingDirections GetOppositeDirection(FacingDirections direction)
        {
            switch (direction)
            {
                default:
                case FacingDirections.UP:
                    return FacingDirections.DOWN;
                case FacingDirections.LEFT:
                    return FacingDirections.RIGHT;
                case FacingDirections.DOWN:
                    return FacingDirections.UP;
                case FacingDirections.RIGHT:
                    return FacingDirections.LEFT;
            }
        }

        public FacingDirections Facing;
        public Tile.TileTypes LandedTileType;

        public int X, Y;
        public int Cash;

        public BoardCharacter()
        {
            Facing = FacingDirections.UP;

            LandedTileType = Tile.TileTypes.NONE;
            X = 0; Y = 0;
            Cash = 0;
        }

        public byte[] Encode()
        {
            var memory = new MemoryStream();
            var writer = new BinaryWriter(memory);

            writer.Write(X);
            writer.Write(Y);
            writer.Write(Cash);
            writer.Write((byte)LandedTileType);
            writer.Write((byte)Facing);

            writer.Close();
            memory.Close();
            return memory.ToArray();
        }

        public void Decode(BinaryReader reader)
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();

            Cash = reader.ReadInt32();

            LandedTileType = (Tile.TileTypes)reader.ReadByte();
            Facing = (FacingDirections)reader.ReadByte();
        }
    }
}
