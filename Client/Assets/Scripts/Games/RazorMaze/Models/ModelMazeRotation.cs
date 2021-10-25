using Exceptions;
using Games.RazorMaze.Views;
using Ticker;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Models
{
    public enum MazeRotateDirection {Clockwise, CounterClockwise}

    public class MazeRotationEventArgs
    {
        public MazeRotateDirection Direction { get; }
        public MazeOrientation CurrentOrientation { get; }
        public MazeOrientation NextOrientation { get; }
        public bool Instantly { get; }

        public MazeRotationEventArgs(
            MazeRotateDirection _Direction, 
            MazeOrientation _CurrentOrientation,
            MazeOrientation _NextOrientation,
            bool _Instantly)
        {
            Direction = _Direction;
            CurrentOrientation = _CurrentOrientation;
            NextOrientation = _NextOrientation;
            Instantly = _Instantly;
        }
    }
    
    public delegate void MazeOrientationHandler(MazeRotationEventArgs Args);
    
    public interface IModelMazeRotation : IInit, IOnLevelStageChanged
    {
        event MazeOrientationHandler RotationStarted;
        event MazeOrientationHandler RotationFinishedInternal;
        void StartRotation(MazeRotateDirection _Direction, MazeOrientation? _NextOrientation = null);
        void OnRotationFinished(MazeRotationEventArgs _Args);
    }
    
    public class ModelMazeRotation : IModelMazeRotation 
    {
        private ModelSettings Settings { get; }
        private IModelData Data { get; }
        private IGameTicker GameTicker { get; }

        public ModelMazeRotation(
            ModelSettings _Settings,
            IModelData _Data,
            IGameTicker _GameTicker)
        {
            Settings = _Settings;
            Data = _Data;
            GameTicker = _GameTicker;
        }
        
        public event MazeOrientationHandler RotationStarted;
        public event MazeOrientationHandler RotationFinishedInternal;
        public event UnityAction Initialized;
        
        public void Init()
        {
            Initialized?.Invoke();
        }
        
        public void StartRotation(
            MazeRotateDirection _Direction, 
            MazeOrientation? _NextOrientation = null)
        {
            if (!Data.ProceedingControls)
                return;
            var currOrientation = Data.Orientation;
            Data.Orientation = _NextOrientation ?? GetNextOrientation(_Direction, currOrientation);
            var args = new MazeRotationEventArgs(
                _Direction, currOrientation, Data.Orientation, false);
            RotationStarted?.Invoke(args);
        }

        public void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            RotationFinishedInternal?.Invoke(_Args);
        }
        
        private static MazeOrientation GetNextOrientation(
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

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var stage = _Args.Stage;
            if (stage == ELevelStage.Unloaded)
            {
                Data.Orientation = MazeOrientation.North;
                var args = new MazeRotationEventArgs(
                    default, default, Data.Orientation, true);
                RotationStarted?.Invoke(args);
            }
            if (_Args.Stage == ELevelStage.ReadyToStart)
            {
                if (Data.Orientation == MazeOrientation.North)
                    return;
                var rotDir = (int) Data.Orientation < 2
                    ? MazeRotateDirection.CounterClockwise
                    : MazeRotateDirection.Clockwise;
                var currOrient = Data.Orientation;
                Data.Orientation = MazeOrientation.North;
                var args = new MazeRotationEventArgs(
                    rotDir, currOrient, Data.Orientation, false);
                RotationStarted?.Invoke(args);
            }
        }
    }
}