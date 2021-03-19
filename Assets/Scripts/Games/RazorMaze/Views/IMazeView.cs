using Entities;
using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views
{
    public interface IMazeView
    {
        void Init();
        void SetLevel(int _Level);
        void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation);
        void Rotate(float _Progress);
        void FinishRotation();
        void OnObstacleMoveStarted(Obstacle _Obstacle, V2Int _From, V2Int _To);
        void OnObstacleMove(Obstacle _Obstacle, float _Progress);
        void OnObstacleMoveFinished(Obstacle _Obstacle);
    }
}