using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class BoardCharacter
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
    }
}
