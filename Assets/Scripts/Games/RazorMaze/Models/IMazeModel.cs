using System;
using Entities;

namespace Games.RazorMaze.Models
{
    public class MazeItemMoveEventArgs : EventArgs
    {
        public MazeItem Item { get; }
        public V2Int From { get; }
        public V2Int To { get; }
        public float Progress { get; }
        public bool Breaked { get; }

        public MazeItemMoveEventArgs(MazeItem _Item, V2Int _From, V2Int _To, float _Progress, bool _Breaked = false)
        {
            Item = _Item;
            From = _From;
            To = _To;
            Progress = _Progress;
            Breaked = _Breaked;
        }
    }
    
    public enum MazeOrientation { North, East, South, West}
    public enum MazeMoveDirection { Up, Right, Down, Left}
    public enum MazeRotateDirection {Clockwise, CounterClockwise}
    
    public delegate void MazeInfoHandler(MazeInfo Info, MazeOrientation Orientation);
    public delegate void MazeOrientationHandler(MazeRotateDirection Direction, MazeOrientation Orientation);
    public delegate void MazeItemMoveHandler(MazeItemMoveEventArgs Args);


    public interface IMazeModel
    {
        event MazeInfoHandler MazeChanged;
        event MazeOrientationHandler RotationStarted;
        event FloatHandler Rotation;
        event NoArgsHandler RotationFinished;
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; }
        void Rotate(MazeRotateDirection _Direction);
    }
}