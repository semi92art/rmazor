using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Games.RazorMaze.Views;
using Ticker;
using UnityEngine.Events;

namespace Games.RazorMaze.Models.InputSchedulers
{
    public interface IInputSchedulerGameProceeder : IAddCommand, IOnLevelStageChanged
    {
        event UnityAction<EInputCommand, object[]> MoveCommand; 
        event UnityAction<EInputCommand, object[]> RotateCommand;
        void                                       UnlockMovement(bool _Unlock);
        void                                       UnlockRotation(bool _Unlock);
    }
    
    public class InputSchedulerGameProceeder : IInputSchedulerGameProceeder, IUpdateTick
    {
        #region constants

        private const int MaxCommandsCount = 3;

        #endregion
        
        #region nonpublic members
        
        private readonly Queue<EInputCommand> m_MoveCommands   = new Queue<EInputCommand>();
        private readonly Queue<EInputCommand> m_RotateCommands = new Queue<EInputCommand>();
        
        private bool m_MovementLocked = true;
        private bool m_RotationLocked = true;
        private int m_MoveCommandsCount;
        private int m_RotateCommandsCount;

        #endregion
        
        #region inject

        private IModelCharacter Character { get; }
        private IModelMazeRotation MazeRotation { get; }
        
        public InputSchedulerGameProceeder(
            IGameTicker _GameTicker,
            IModelCharacter _Character,
            IModelMazeRotation _MazeRotation)
        {
            Character = _Character;
            MazeRotation = _MazeRotation;
            _GameTicker.Register(this);
            MoveCommand += OnMoveCommand;
            RotateCommand += OnRotateCommand;
        }

        #endregion

        #region api

        public event UnityAction<EInputCommand, object[]> MoveCommand; 
        public event UnityAction<EInputCommand, object[]> RotateCommand;
        
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
                    m_MoveCommands.Enqueue(_Command);
                    m_MoveCommandsCount++;
                    break;
                case EInputCommand.RotateClockwise:
                case EInputCommand.RotateCounterClockwise:
                    if (m_RotateCommandsCount >= MaxCommandsCount) return;
                    m_RotateCommands.Enqueue(_Command);
                    m_RotateCommandsCount++;
                    break;
            }
        }
        
        public void UnlockMovement(bool _Unlock)
        {
            m_MovementLocked = !_Unlock;
        }

        public void UnlockRotation(bool _Unlock)
        {
            m_RotationLocked = !_Unlock;
        }

        #endregion

        #region nonpublic methods

        private void ScheduleMovementCommands()
        {
            if (m_MovementLocked || m_MoveCommandsCount == 0)
                return;
            var cmd = m_MoveCommands.Dequeue();
            m_MoveCommandsCount--;
            m_MovementLocked = true;
            MoveCommand?.Invoke(cmd, null);
        }

        private void ScheduleRotationCommands()
        {
            if (m_RotationLocked || m_RotateCommandsCount == 0)
                return;
            var cmd = m_RotateCommands.Dequeue();
            m_RotateCommandsCount--;
            m_RotationLocked = true;
            RotateCommand?.Invoke(cmd, null);
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
            MazeRotateDirection dir;
            switch (_Command)
            {
                case EInputCommand.RotateClockwise:       
                    dir = MazeRotateDirection.Clockwise;        break;
                case EInputCommand.RotateCounterClockwise:
                    dir = MazeRotateDirection.CounterClockwise; break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            MazeRotation.StartRotation(dir);
        }

        #endregion

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.StartedOrContinued)
            {
                m_MoveCommands.Clear();
                m_MoveCommandsCount = 0;
                m_RotateCommands.Clear();
                m_RotateCommandsCount = 0;
            }
        }
    }
}