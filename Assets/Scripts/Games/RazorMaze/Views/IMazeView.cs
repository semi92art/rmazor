using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views
{
    public interface IMazeView
    {
        void Init(IMazeModel _Model);
        void SetLevel(int _Level);
        void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation);
        void Rotate(float _Progress);
        void FinishRotation();
    }
}