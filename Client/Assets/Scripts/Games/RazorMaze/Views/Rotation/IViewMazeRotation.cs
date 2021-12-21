using Games.RazorMaze.Models;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.Rotation
{
    public interface IViewMazeRotation : IInit, IOnLevelStageChanged
    {
        event UnityAction<float>     RotationContinued;
        event MazeOrientationHandler RotationFinished;
        void                         OnRotationStarted(MazeRotationEventArgs _Args);
        void                         OnRotationFinished(MazeRotationEventArgs _Args);
    }
}