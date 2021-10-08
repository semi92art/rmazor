using Exceptions;
using Games.RazorMaze.Models;
using Utils;

namespace Games.RazorMaze.Views.Rotation
{
    public abstract class ViewMazeRotationBase : IViewMazeRotation
    {
        #region api

        public event NoArgsHandler Initialized;
        public abstract event FloatHandler RotationContinued;
        public abstract event MazeOrientationHandler RotationFinished;
        
        public virtual void Init() => Initialized?.Invoke();
        public abstract void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation);

        #endregion
        
        #region nonpublic methods
        
        protected static MazeOrientation GetNextOrientation(
            MazeRotateDirection _Direction,
            MazeOrientation _Orientation)
        {
            int orient = (int) _Orientation;
            switch (_Direction)
            {
                case MazeRotateDirection.Clockwise:
                    orient = MathUtils.ClampInverse(orient + 1, 0, 3); break;
                case MazeRotateDirection.CounterClockwise:
                    orient = MathUtils.ClampInverse(orient - 1, 0, 3); break;
                default: throw new SwitchCaseNotImplementedException(_Direction);
            }
            return (MazeOrientation) orient;
        }
        
        protected static float GetAngleByOrientation(MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North: return 0;
                case MazeOrientation.East:  return 270;
                case MazeOrientation.South: return 180;
                case MazeOrientation.West:  return 90;
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
        }
        
        #endregion
    }
}