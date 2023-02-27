using mazing.common.Runtime;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.Rotation
{
    public interface IViewMazeRotation :
        IInit,
        IOnLevelStageChanged, 
        IMazeRotationStarted,
        IMazeRotationFinished
    {
        event UnityAction<float>     RotationContinued;
        event MazeOrientationHandler RotationFinished;
    }
}