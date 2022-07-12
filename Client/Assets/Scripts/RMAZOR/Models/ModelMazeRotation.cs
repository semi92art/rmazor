using Common;
using Common.Exceptions;
using Common.Helpers;
using Common.Utils;
using RMAZOR.Views;

namespace RMAZOR.Models
{
    public enum EMazeRotateDirection {Clockwise, CounterClockwise}

    public class MazeRotationEventArgs
    {
        public EMazeRotateDirection Direction          { get; }
        public MazeOrientation      CurrentOrientation { get; }
        public MazeOrientation      NextOrientation    { get; }
        public bool                 Instantly          { get; }

        public MazeRotationEventArgs(
            EMazeRotateDirection _Direction,
            MazeOrientation      _CurrentOrientation,
            MazeOrientation      _NextOrientation,
            bool                 _Instantly)
        {
            Direction          = _Direction;
            CurrentOrientation = _CurrentOrientation;
            NextOrientation    = _NextOrientation;
            Instantly          = _Instantly;
        }
    }
    
    public delegate void MazeOrientationHandler(MazeRotationEventArgs Args);
    
    public interface IModelMazeRotation : IInit, IOnLevelStageChanged
    {
        MazeOrientation Orientation        { get; set; }
        event MazeOrientationHandler RotationStarted;
        event MazeOrientationHandler RotationFinished;
        void StartRotation(EMazeRotateDirection _Direction, MazeOrientation? _NextOrientation = null);
        void OnRotationFinished(MazeRotationEventArgs _Args);
    }
    
    public class ModelMazeRotation : InitBase, IModelMazeRotation 
    {
        private IModelData Data { get; }

        public ModelMazeRotation(IModelData _Data)
        {
            Data = _Data;
        }

        public MazeOrientation              Orientation { get; set; }
        public event MazeOrientationHandler RotationStarted;
        public event MazeOrientationHandler RotationFinished;

        public void StartRotation(
            EMazeRotateDirection _Direction,
            MazeOrientation?     _NextOrientation = null)
        {
            if (!Data.ProceedingControls)
                return;
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
                case ELevelStage.ReadyToStart when _Args.PreviousStage != ELevelStage.CharacterKilled:
                    if (Orientation == MazeOrientation.North)
                        return;
                    var rotDir = (int) Orientation < 2
                        ? EMazeRotateDirection.CounterClockwise
                        : EMazeRotateDirection.Clockwise;
                    var currentOrientation = Orientation;
                    Orientation = MazeOrientation.North;
                {
                    var args = new MazeRotationEventArgs(
                        rotDir, currentOrientation, Orientation, false);
                    RotationStarted?.Invoke(args);
                }
                break;
                case ELevelStage.Unloaded:
                    Orientation = MazeOrientation.North;
                {
                    var args = new MazeRotationEventArgs(
                        default, default, Orientation, true);
                    RotationStarted?.Invoke(args);
                }
                break;
            }
        }
        
        private static MazeOrientation GetNextOrientation(
            EMazeRotateDirection _Direction,
            MazeOrientation      _Orientation)
        {
            int orient = (int) _Orientation;
            orient = _Direction switch
            {
                EMazeRotateDirection.Clockwise        => MathUtils.ClampInverse(orient + 1, 0, 3),
                EMazeRotateDirection.CounterClockwise => MathUtils.ClampInverse(orient - 1, 0, 3),
                _                                     => throw new SwitchCaseNotImplementedException(_Direction)
            };
            return (MazeOrientation) orient;
        }
    }
}