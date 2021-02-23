using System.Collections.Generic;
using UnityEngine;

namespace Games.Maze
{
    public class MazeGraphNode
    {
        public Vector2 Point { get; set; }
        public List<Vector2> Neighbours { get; set; }
    }
}