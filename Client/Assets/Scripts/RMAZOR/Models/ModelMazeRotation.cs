using mazing.common.Runtime;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using RMAZOR.Views;

namespace RMAZOR.Models
{
    public enum EMazeRotateDirection {Clockwise, CounterClockwise}

    public class MazeRotationEventArgs
    {
        public EMazeRotateDirection Direction          { get; }
        public EMazeOrientation     CurrentOrientation { get; }
        public EMazeOrientation     NextOrientation    { get; }
        public bool                 Instantly          { get; }

        public MazeRotationEventArgs(
            EMazeRotateDirection _Direction,
            EMazeOrientation     _CurrentOrientation,
            EMazeOrientation     _NextOrientation,
            bool                 _Instantly)
        {
            Direction          = _Direction;
            CurrentOrientation = _CurrentOrientation;
            NextOrientation    = _NextOrientation;
            Instantly          = _Instantly;
        }
    }
    
    public delegate void MazeOrientationHandler(MazeRotationEventArgs _Args);
    
    public interface IModelMazeRotation : IInit, IOnLevelStageChanged
    {
        EMazeOrientation Orientation        { get; set; }
        event MazeOrientationHandler RotationStarted;
        event MazeOrientationHandler RotationFinished;
        void StartRotation(EMazeRotateDirection _Direction, EMazeOrientation? _NextOrientation = null);
        void OnRotationFinished(MazeRotationEventArgs _Args);
    }
    
    public class ModelMazeRotation : InitBase, IModelMazeRotation 
    {
        public EMazeOrientation             Orientation { get; set; }
        public event MazeOrientationHandler RotationStarted;
        public event MazeOrientationHandler RotationFinished;

        public void StartRotation(
            EMazeRotateDirection _Direction,
            EMazeOrientation?     _NextOrientation = null)
        {
            var currOrientation = Orientation;
            Orientation = _NextOrientation ?? GetNextOrientation(_Direction, currOrientation);
            var args = new MazeRotationEventArgs(
                _Direction, currOrientation, Orientation, false);
            RotationStarted?.Invoke(args);
        }

        public void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            RotationFinished?.Invoke(_Args);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart when Orientation != EMazeOrientation.North:
                    var rotDir = (int) Orientation < 2
                        ? EMazeRotateDirection.CounterClockwise
                        : EMazeRotateDirection.Clockwise;
                {
                    var currentOrientation = Orientation;
                    Orientation = EMazeOrientation.North;
                    var args = new MazeRotationEventArgs(
                        rotDir, currentOrientation, Orientation, false);
                    RotationStarted?.Invoke(args);
                }
                break;
                case ELevelStage.None:
                case ELevelStage.Unloaded:
                {
                    Orientation = EMazeOrientation.North;
                    var args = new MazeRotationEventArgs(
                        default, default, Orientation, true);
                    RotationStarted?.Invoke(args);
                }
                break;
            }
        }
        
        private static EMazeOrientation GetNextOrientation(
            EMazeRotateDirection _Direction,
            EMazeOrientation      _Orientation)
        {
            int orient = (int) _Orientation;
            orient = _Direction switch
            {
                EMazeRotateDirection.Clockwise        => MathUtils.ClampInverse(orient + 1, 0, 3),
                EMazeRotateDirection.CounterClockwise => MathUtils.ClampInverse(orient - 1, 0, 3),
                _                                     => throw new SwitchCaseNotImplementedException(_Direction)
            };
            return (EMazeOrientation) orient;
        }
    }
}