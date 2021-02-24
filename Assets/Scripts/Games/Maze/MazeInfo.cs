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

    public class MazeInfoLite
    {
        public int Index { get; set; }
        public MazeCellsType CellsType { get; set; }
        public bool IsBig { get; set; }
        public bool IsDark { get; set; }
        public bool IsTime { get; set; }
        public bool IsInfiniteStage { get; set; }

        public static bool operator ==(MazeInfoLite _A, MazeInfoLite _B)
        {
            if (_A == null && _B == null)
                return true;
            if (_A == null || _B == null)
                return false;
            return _A.Index == _B.Index
                   && _A.CellsType == _B.CellsType
                   && _A.IsBig == _B.IsBig
                   && _A.IsDark == _B.IsDark
                   && _A.IsTime == _B.IsTime
                   && _A.IsInfiniteStage == _B.IsInfiniteStage;
        }

        public static bool operator !=(MazeInfoLite _A, MazeInfoLite _B)
        {
            return !(_A == _B);
        }
    }
    
    public class MazeInfo : MazeInfoLite
    {
        public MazeCellsType ShapeType { get; set; }
        public Vector2Int Dimensions { get; set; }
        public Vector2 Size { get; set; }
        public float WallWidth { get; set; }
        public List<Line2dPoints> Walls { get; set; }
        public List<MazeGraphNode> PathNodes { get; set; }
        public List<Vector2> Enemies { get; set; }
        public System.TimeSpan? Timer { get; set; }
    }
}