using Common;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.Rotation
{
    public interface IViewMazeRotation : IInit, IOnLevelStageChanged
    {
        event UnityAction<float>     RotationContinued;
        event MazeOrientationHandler RotationFinished;
        void                         OnRotationStarted(MazeRotationEventArgs _Args);
        void                         OnRotationFinished(MazeRotationEventArgs _Args);
    }
}