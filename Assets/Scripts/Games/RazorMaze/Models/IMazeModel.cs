using Entities;

namespace Games.RazorMaze.Models
{
    public enum MazeOrientation { North, East, South, West}
    public enum MazeRotateDirection {Clockwise, CounterClockwise}
    
    public delegate void MazeInfoHandler(MazeInfo Info, MazeOrientation Orientation);
    public delegate void MazeOrientationHandler(MazeRotateDirection Direction, MazeOrientation Orientation);
    public delegate void ObstacleItemHandler(Obstacle Obstacle);
    public delegate void ObstacleStartMoveHandler(Obstacle Obstacle, V2Int From, V2Int To);
    public delegate void ObstacleMoveHandler(Obstacle Obstacle, float Progress);


    public interface IMazeModel
    {
        event MazeInfoHandler MazeChanged;
        event MazeOrientationHandler RotationStarted;
        event FloatHandler Rotation;
        event NoArgsHandler RotationFinished;
        event ObstacleStartMoveHandler ObstacleMoveStarted;
        event ObstacleMoveHandler ObstacleMove;
        event ObstacleItemHandler ObstacleMoveFinished;
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; }
        void Rotate(MazeRotateDirection _Direction);
        void MoveObstacles();
    }
}