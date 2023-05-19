using mazing.common.Runtime;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.Rotation
{
    public interface IViewMazeRotation :
        IInit,
        IMazeRotationStarted,
        IMazeRotationFinished
    {
        event MazeOrientationHandler RotationFinished;
    }
}