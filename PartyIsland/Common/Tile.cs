using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Common
{
    public class Tile
    {
        public enum TileTypes
        {
            BLUE,
            RED,
            SPECIAL,
            /// <summary>
            /// Fork in the map to change direction
            /// </summary>
            FORK,
            NONE,
        }

        public TileTypes Type;
        public int X, Y;

        public BoardCharacter.FacingDirections[] Forks;

        public Tile(int x, int y, TileTypes t)
        {
            X = x;
            Y = y;
            Type = t;
            Forks = null;
        }

        public static Tile[,] TileMapFromStream(StreamReader reader)
        {
            var width = int.Parse(reader.ReadLine());
            var height = int.Parse(reader.ReadLine());

            Tile[,] map = new Tile[width, height];
            int x = 0;
            int y = 0;
            for (var i = 0; i < height; i++)
            {
                x = 0;
                var stringRow = reader.ReadLine();
                if (stringRow == null)
                    break;
                for (var j = 0; j < stringRow.Length; j++)
                {
                    int dataValue = stringRow[j];
                    if (dataValue == -1)
                        break;

                    char tileText = (char)dataValue;

                    Tile.TileTypes type = TileTypes.NONE;
                    var tile = new Tile(j, i, type);
                    switch (tileText)
                    {
                        case '0':
                            type = TileTypes.NONE;
                            break;
                        case '1':
                            type = TileTypes.BLUE;
                            break;
                        case '2':
                            type = TileTypes.RED;
                            break;
                        case '3':
                            type = TileTypes.SPECIAL;
                            break;
                        case 'F':
                            type = TileTypes.FORK;
                            j++;  //'{'
                            var forks = new List<BoardCharacter.FacingDirections>();
                            for (var k = 0; k < 4; k++)
                            {
                                j++;
                                var forkDirection = (char)stringRow[j];
                                if (forkDirection == '1')
                                {
                                    forks.Add((BoardCharacter.FacingDirections)k);
                                }
                            }
                            tile.Forks = forks.ToArray();
                            j++; //'}'
                            break;
                    }

                    tile.Type = type;
                    map[x, y] = tile;

                    x++;
                }
                y++;
            }

            return map;
        }
    }
}
