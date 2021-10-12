using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.Rotation
{
    public interface IViewMazeRotation : IInit, IOnLevelStageChanged
    {
        event FloatHandler RotationContinued;
        event MazeOrientationHandler RotationFinished;
        void StartRotation(MazeRotationEventArgs _Args);
    }
}