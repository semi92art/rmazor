using mazing.common.Runtime.Exceptions;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.Rotation
{
    public abstract class ViewMazeRotationBase : IViewMazeRotation
    {
        #region api

        public bool                                  Initialized { get; private set; }
        public event          UnityAction            Initialize;
        public abstract event UnityAction<float>     RotationContinued;
        public abstract event MazeOrientationHandler RotationFinished;
        
        public virtual void Init()
        {
            Initialize?.Invoke();
            Initialized = true;
        }

        public abstract void OnMazeRotationStarted(MazeRotationEventArgs _Args);
        public abstract void OnMazeRotationFinished(MazeRotationEventArgs _Args);
        public abstract void OnLevelStageChanged(LevelStageArgs _Args);


        #endregion
        
        #region nonpublic methods

        protected static float GetAngleByOrientation(EMazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case EMazeOrientation.North: return 0;
                case EMazeOrientation.East:  return 270;
                case EMazeOrientation.South: return 180;
                case EMazeOrientation.West:  return 90;
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
        }
        
        #endregion

    }
}