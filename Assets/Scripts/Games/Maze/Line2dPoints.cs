using UnityEngine;

namespace Games.Maze
{
    public class Line2dPoints
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }

        public Line2dPoints(Vector2 _Start, Vector2 _End)
        {
            Start = _Start;
            End = _End;
        }

        public float Length => Vector2.Distance(Start, End);
    }
}