using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Ticker;

namespace Games.RazorMaze.Models.InputSchedulers
{
    public interface IInputSchedulerGameProceeder : IAddCommand
    {
        event InputCommandHandler MoveCommand; 
        event InputCommandHandler RotateCommand;
        void UnlockMovement(bool _Unlock);
        void UnlockRotation(bool _Unlock);
    }
    
    public class InputSchedulerGameProceeder : IUpdateTick, IInputSchedulerGameProceeder
    {
        #region constants

        private const int MaxCommandsCount = 3;

        #endregion
        
        #region nonpublic members

        private readonly Queue<int> m_MoveCommands = new Queue<int>();
        private readonly Queue<int> m_RotateCommands = new Queue<int>();
        
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

        public event InputCommandHandler MoveCommand; 
        public event InputCommandHandler RotateCommand;
        
        public void UpdateTick()
        {
            ScheduleMovementCommands();
            ScheduleRotationCommands();
        }

        public void AddCommand(int _Command, object[] _Args = null)
        {
            switch (_Command)
            {
                case InputCommands.MoveDown:
                case InputCommands.MoveLeft:
                case InputCommands.MoveRight:
                case InputCommands.MoveUp:
                    if (m_MoveCommandsCount >= MaxCommandsCount) return;
                    m_MoveCommands.Enqueue(_Command);
                    m_MoveCommandsCount++;
                    break;
                case InputCommands.RotateClockwise:
                case InputCommands.RotateCounterClockwise:
                    if (m_RotateCommandsCount >= MaxCommandsCount) return;
                    m_RotateCommands.Enqueue(_Command);
                    m_RotateCommandsCount++;
                    break;
            }
        }
        
        public void UnlockMovement(bool _Unlock)
        {
            if (!_Unlock)
            {
                m_MoveCommands.Clear();
                m_MoveCommandsCount = 0;
            }
            m_MovementLocked = !_Unlock;
        }

        public void UnlockRotation(bool _Unlock)
        {
            if (!_Unlock)
            {
                m_RotateCommands.Clear();
                m_RotateCommandsCount = 0;
            }
            m_RotationLocked = !_Unlock;
        }

        #endregion

        #region nonpublic methods

        private void ScheduleMovementCommands()
        {
            if (m_MovementLocked || !m_MoveCommands.Any())
                return;
            var cmd = m_MoveCommands.Dequeue();
            m_MoveCommandsCount--;
            m_MovementLocked = true;
            MoveCommand?.Invoke(cmd);
        }

        private void ScheduleRotationCommands()
        {
            if (m_RotationLocked || !m_RotateCommands.Any())
                return;
            var cmd = m_RotateCommands.Dequeue();
            m_RotateCommandsCount--;
            m_RotationLocked = true;
            RotateCommand?.Invoke(cmd);
        }
        
        private void OnMoveCommand(int _Command, object[] _Args)
        {
            EMazeMoveDirection dir = default;
            switch (_Command)
            {
                case InputCommands.MoveUp:    dir = EMazeMoveDirection.Up;    break;
                case InputCommands.MoveDown:  dir = EMazeMoveDirection.Down;  break;
                case InputCommands.MoveLeft:  dir = EMazeMoveDirection.Left;  break;
                case InputCommands.MoveRight: dir = EMazeMoveDirection.Right; break;
                case InputCommands.RotateClockwise:
                case InputCommands.RotateCounterClockwise:
                    break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            Character.Move(dir);
        }
        
        private void OnRotateCommand(int _Command, object[] _Args)
        {
            MazeRotateDirection dir;
            switch (_Command)
            {
                case InputCommands.RotateClockwise:       
                    dir = MazeRotateDirection.Clockwise;        break;
                case InputCommands.RotateCounterClockwise:
                    dir = MazeRotateDirection.CounterClockwise; break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            MazeRotation.StartRotation(dir);
        }

        #endregion
    }
}