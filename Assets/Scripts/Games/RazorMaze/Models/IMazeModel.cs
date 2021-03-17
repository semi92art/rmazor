using UnityGameLoopDI;

namespace Games.RazorMaze.Models
{
    public delegate void MazeInfoHandler(MazeInfo _Info, MazeOrientation _Orientation);
    public delegate void MazeOrientationHandler(MazeRotateDirection _Direction, MazeOrientation _Orientation);
    public delegate void MazeRotationHandler(float _Progress);
    public enum MazeOrientation { North, East, South, West}
    public enum MazeRotateDirection {Clockwise, CounterClockwise}
    
    public interface IMazeModel
    {
        event MazeInfoHandler MazeChanged;
        event MazeOrientationHandler RotationStarted;
        event MazeRotationHandler OnRotation;
        event NoArgsHandler RotationFinished;
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; }
        void Rotate(MazeRotateDirection _Direction);
    }
}