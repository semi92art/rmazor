using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.Rotation
{

    
    public interface IViewRotation : IInit
    {
        void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation);
        void Rotate(float _Progress);
        void FinishRotation();
    }
}