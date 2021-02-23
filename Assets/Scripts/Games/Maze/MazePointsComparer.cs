using System.Collections.Generic;
using Games.Maze.Utils;
using UnityEngine;

namespace Games.Maze
{
    public class MazePointsComparer : IEqualityComparer<Vector2>
    {
        public static bool SamePoints(Vector2 _A, Vector2 _B)
        {
            float epsilon = 0.001f;
            return Mathf.Abs(_A.x - _B.x) < epsilon
                   && Mathf.Abs(_A.y - _B.y) < epsilon;
        }

        public bool Equals(Vector2 _A, Vector2 _B) => SamePoints(_A, _B);
        public int GetHashCode(Vector2 _V) => _V.GetHashCode();
    }
}