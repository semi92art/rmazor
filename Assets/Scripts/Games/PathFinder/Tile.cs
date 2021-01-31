using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Games.PathFinder
{
    public class Tile : GridObject, IHasNeighbours<Tile>
    {
        public bool isPassable;

        public Tile(int _X, int _Y)
            : base(_X, _Y)
        {
            isPassable = true;
        }

        public IEnumerable<Tile> AllNeighbours { get; set; }
        public IEnumerable<Tile> Neighbours
        {
            get { return AllNeighbours.Where(_O => _O.isPassable); }
        }

        public void FindNeighbours(Dictionary<Point, TileBehaviour> _Board,
            Vector2 _BoardSize, bool _EqualLineLengths)
        {
            List<Tile> neighbours = new List<Tile>();
            List<Point> neighboursList;
            if (Y % 2 == 0)
            {
                neighboursList = NeighbourShift;
            }
            else neighboursList = NeighbourShiftOffset;

            foreach (Point point in neighboursList)
            {
                int neighbourX = X + point.X;
                int neighbourY = Y + point.Y;

                if (neighbourX >= 0 &&
                    neighbourX < (int) _BoardSize.x &&
                    neighbourY >= 0 &&
                    neighbourY < (int) _BoardSize.y)
                {
                    if (_Board.ContainsKey(new Point(neighbourX, neighbourY)))
                    {
                        neighbours.Add(_Board[new Point(neighbourX, neighbourY)].tile);
                    }
                }
            }

            AllNeighbours = neighbours;
        }

        public static List<Point> NeighbourShift
        {
            get
            {
                return new List<Point>
                {
                    new Point(-1, 0),
                    new Point(-1, -1),
                    new Point(0, -1),
                    new Point(1, 0),
                    new Point(0, 1),
                    new Point(-1, 1),
                };
            }
        }

        public static List<Point> NeighbourShiftOffset
        {
            get
            {
                return new List<Point>
                {
                    new Point(-1, 0),
                    new Point(0, -1),
                    new Point(1, -1),
                    new Point(1, 0),
                    new Point(1, 1),
                    new Point(0, 1),
                };
            }
        }

        public bool Equals(Tile _Other)
        {
            if (base.X == _Other.X && base.Y == _Other.Y)
            {
                return true;
            }
            else return false;
        }
    }
}
