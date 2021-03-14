using UnityGameLoopDI;

namespace Games.RazorMaze.Models
{
    public delegate void MazeInfoHandler(MazeInfo _Info);
    public delegate void MazeOrientationHandler(MazeRotateDirection _Direction, MazeOrientation _Orientation);
    public delegate void MazeRotationHandler(float _Progress);
    public enum MazeOrientation { North, East, South, West}
    public enum MazeRotateDirection {Clockwise, CounterClockwise}
    
    public interface IMazeModel : IOnUpdate
    {
        event MazeInfoHandler OnMazeChanged;
        event MazeOrientationHandler OnRotationStarted;
        event MazeRotationHandler OnRotation;
        event MazeOrientationHandler OnRotationFinished;
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; }
        void Rotate(MazeRotateDirection _Direction);
    }
}