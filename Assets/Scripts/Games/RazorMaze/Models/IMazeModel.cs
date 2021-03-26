namespace Games.RazorMaze.Models
{

    
    public enum MazeOrientation { North, East, South, West}
    public enum MazeMoveDirection { Up, Right, Down, Left}
    public enum MazeRotateDirection {Clockwise, CounterClockwise}
    
    public delegate void MazeInfoHandler(MazeInfo Info, MazeOrientation Orientation);
    public delegate void MazeOrientationHandler(MazeRotateDirection Direction, MazeOrientation Orientation);
    

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