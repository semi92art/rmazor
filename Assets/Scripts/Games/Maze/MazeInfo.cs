using System.Collections.Generic;
using UnityEngine;

namespace Games.Maze
{
    public enum MazeCellsType
    {
        Rectangular,
        Hexagonal,
        Triangular,
        //Circled
    }
    
    public class MazeInfo
    {
        public bool IsBig { get; set; }
        public MazeCellsType ShapeType { get; set; }
        public MazeCellsType CellsType { get; set; }
        public Vector2Int Dimensions { get; set; }
        public Vector2 Size { get; set; }
        public float WallWidth { get; set; }
        public List<Line2dPoints> Walls { get; set; }
        public List<MazeGraphNode> PathNodes { get; set; }
    }
}