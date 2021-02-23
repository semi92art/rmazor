using System.Collections.Generic;
using UnityEngine;

namespace Games.Maze
{
    public class PolyLine2dPoints
    {
        public List<Vector2> Points { get; }

        public PolyLine2dPoints(List<Vector2> _Points)
        {
            Points = _Points;
        }
    }
}