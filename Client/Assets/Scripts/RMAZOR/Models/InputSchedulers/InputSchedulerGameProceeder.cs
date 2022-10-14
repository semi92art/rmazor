using Common;
using Common.Exceptions;
using Common.Helpers;
using Common.Ticker;
using RMAZOR.Views;
using UnityEngine.Events;

namespace RMAZOR.Models.InputSchedulers
{
    public interface IInputSchedulerGameProceeder : IInit, IAddCommand, IOnLevelStageChanged
    {
        event UnityAction<EInputCommand, object[]> MoveCommand; 
        event UnityAction<EInputCommand, object[]> RotateCommand;
        void                                       LockMovement(bool _Lock);
        void                                       LockRotation(bool _Lock);
    }
    
    public class InputSchedulerGameProceeder : InitBase, IInputSchedulerGameProceeder, IUpdateTick
    {
        #region constants

        private const int MaxCommandsCount = 3;

        #endregion
        
        #region nonpublic members

        private readonly EInputCommand?[] m_MoveCommands   = new EInputCommand?[MaxCommandsCount];
        private readonly EInputCommand?[] m_RotateCommands = new EInputCommand?[MaxCommandsCount];
        
        private bool m_MovementLocked = true;
        private bool m_RotationLocked = true;
        private int m_MoveCommandsCount;
        private int m_RotateCommandsCount;

        #endregion
        
        #region inject

        private IModelGameTicker   GameTicker   { get; }
        private IModelCharacter    Character    { get; }
        private IModelMazeRotation MazeRotation { get; }
        
        private InputSchedulerGameProceeder(
            IModelGameTicker   _GameTicker,
            IModelCharacter    _Character,
            IModelMazeRotation _MazeRotation)
        {
            GameTicker   = _GameTicker;
            Character    = _Character;
            MazeRotation = _MazeRotation;
        }

        #endregion

        #region api

        public event UnityAction<EInputCommand, object[]> MoveCommand; 
        public event UnityAction<EInputCommand, object[]> RotateCommand;

        public override void Init()
        {
            MoveCommand   += OnMoveCommand;
            RotateCommand += OnRotateCommand;
            GameTicker.Register(this);
            base.Init();
        }

        public void UpdateTick()
        {
            ScheduleMovementCommands();
            ScheduleRotationCommands();
        }

        public void AddCommand(EInputCommand _Command, object[] _Args = null)
        {
            switch (_Command)
            {
                case EInputCommand.MoveDown:
                case EInputCommand.MoveLeft:
                case EInputCommand.MoveRight:
                case EInputCommand.MoveUp:
                    if (m_MoveCommandsCount >= MaxCommandsCount) return;
                    m_MoveCommands[m_MoveCommandsCount] = _Command;
                    m_MoveCommandsCount++;
                    break;
                case EInputCommand.RotateClockwise:
                case EInputCommand.RotateCounterClockwise:
                    if (m_RotateCommandsCount >= MaxCommandsCount) return;
                    m_RotateCommands[m_RotateCommandsCount] = _Command;
                    m_RotateCommandsCount++;
                    break;
            }
        }
        
        public void LockMovement(bool _Lock)
        {
            m_MovementLocked = _Lock;
        }

        public void LockRotation(bool _Lock)
        {
            m_RotationLocked = _Lock;
        }

        #endregion

        #region nonpublic methods

        private void ScheduleMovementCommands()
        {
            if (m_MovementLocked || m_MoveCommandsCount == 0)
                return;
            int idx = m_MoveCommandsCount - 1;
            EInputCommand? cmd;
            (cmd, m_MoveCommands[idx]) = (m_MoveCommands[idx], null);
            if (!cmd.HasValue)
                return;
            m_MoveCommandsCount--;
            MoveCommand?.Invoke(cmd.Value, null);
        }

        private void ScheduleRotationCommands()
        {
            if (m_RotationLocked || m_RotateCommandsCount == 0)
                return;
            int idx = m_RotateCommandsCount - 1;
            EInputCommand? cmd;
            (cmd, m_RotateCommands[idx]) = (m_RotateCommands[idx], null);
            if (!cmd.HasValue)
                return;
            m_RotateCommandsCount--;
            RotateCommand?.Invoke(cmd.Value, null);
        }
        
        private void OnMoveCommand(EInputCommand _Command, object[] _Args)
        {
            EMazeMoveDirection dir = default;
            switch (_Command)
            {
                case EInputCommand.MoveUp:    dir = EMazeMoveDirection.Up;    break;
                case EInputCommand.MoveDown:  dir = EMazeMoveDirection.Down;  break;
                case EInputCommand.MoveLeft:  dir = EMazeMoveDirection.Left;  break;
                case EInputCommand.MoveRight: dir = EMazeMoveDirection.Right; break;
                case EInputCommand.RotateClockwise:
                case EInputCommand.RotateCounterClockwise:
                    break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            Character.Move(dir);
        }
        
        private void OnRotateCommand(EInputCommand _Command, object[] _Args)
        {
            EMazeRotateDirection dir = default;
            switch (_Command)
            {
                case EInputCommand.MoveUp:   
                case EInputCommand.MoveDown: 
                case EInputCommand.MoveLeft: 
                case EInputCommand.MoveRight:
                    break;
                case EInputCommand.RotateClockwise:       
                    dir = EMazeRotateDirection.Clockwise;     
                    break;
                case EInputCommand.RotateCounterClockwise:
                    dir = EMazeRotateDirection.CounterClockwise;
                    break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            MazeRotation.StartRotation(dir);
        }

        #endregion

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.StartedOrContinued)
                return;
            for (int i = 0; i < MaxCommandsCount; i++)
            {
                m_MoveCommands[i]   = null;
                m_RotateCommands[i] = null;
            }
            m_MoveCommandsCount   = 0;
            m_RotateCommandsCount = 0;
        }
    }
}