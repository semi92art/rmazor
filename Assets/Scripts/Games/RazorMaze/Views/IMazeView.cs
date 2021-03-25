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
        void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args);
    }
}