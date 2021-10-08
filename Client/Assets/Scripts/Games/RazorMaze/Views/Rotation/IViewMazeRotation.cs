using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.Rotation
{

    
    public interface IViewMazeRotation : IInit
    {
        event FloatHandler RotationContinued;
        event MazeOrientationHandler RotationFinished;
        void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation);
    }
}