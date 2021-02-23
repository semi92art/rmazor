using System.Collections.Generic;
using Games.Maze.Utils;

namespace Games.Maze
{
    public class MazeLinesComparer : IEqualityComparer<Line2dPoints>
    {
        public static bool SameLines(Line2dPoints _A, Line2dPoints _B)
        {
            return MazePointsComparer.SamePoints(_A.Start, _B.Start)
                   && MazePointsComparer.SamePoints(_A.End, _B.End);
        }

        public bool Equals(Line2dPoints _A, Line2dPoints _B) => SameLines(_A, _B);
        public int GetHashCode(Line2dPoints _V) => _V.GetHashCode();
    }
}