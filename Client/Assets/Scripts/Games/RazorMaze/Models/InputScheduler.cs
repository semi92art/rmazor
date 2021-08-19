using System.Collections.Generic;
using System.Linq;
using Exceptions;
using UnityGameLoopDI;

namespace Games.RazorMaze.Models
{
    public delegate void EInputCommandHandler(EInputCommand _Command);
    
    public interface IInputScheduler
    {
        event EInputCommandHandler MoveCommand; 
        event EInputCommandHandler RotateCommand; 
        void AddCommand(EInputCommand _Command);
        //void LockMovement();
        void UnlockMovement();
        //void LockRotation();
        void UnlockRotation();
    }
    
    public class InputScheduler : IInputScheduler, IUpdateTick
    {
        #region nonpublic members
        
        private readonly Queue<EInputCommand> m_MoveCommands = new Queue<EInputCommand>();
        private readonly Queue<EInputCommand> m_RotateCommands = new Queue<EInputCommand>();

        private bool m_MovementLocked;
        private bool m_RotationLocked;
        private int m_MoveCommandsCount;
        private int m_RotateCommandsCount;
        
        #endregion
        
        #region inject

        public InputScheduler(ITicker _Ticker)
        {
            _Ticker.Register(this);
        }
        
        #endregion
        
        #region api
        
        public event EInputCommandHandler MoveCommand;
        public event EInputCommandHandler RotateCommand;

        public void AddCommand(EInputCommand _Command)
        {
            switch (_Command)
            {
                case EInputCommand.MoveDown:
                case EInputCommand.MoveLeft:
                case EInputCommand.MoveRight:
                case EInputCommand.MoveUp:
                    if (m_MoveCommandsCount >= 3) return;
                    m_MoveCommands.Enqueue(_Command);
                    m_MoveCommandsCount++;
                    break;
                case EInputCommand.RotateClockwise:
                case EInputCommand.RotateCounterClockwise:
                    if (m_RotateCommandsCount >= 3) return;
                    m_RotateCommands.Enqueue(_Command);
                    m_RotateCommandsCount++;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Command);
            }
        }

        public void UnlockMovement() => m_MovementLocked = false;
        public void UnlockRotation() => m_RotationLocked = false;

        public void UpdateTick()
        {
            ScheduleMovement();
            ScheduleRotation();
        }
        
        #endregion

        #region nonpublic methods

        private void ScheduleMovement()
        {
            if (m_MovementLocked || !m_MoveCommands.Any())
                return;
            var cmd = m_MoveCommands.Dequeue();
            m_MoveCommandsCount--;
            m_MovementLocked = true;
            MoveCommand?.Invoke(cmd);
        }

        private void ScheduleRotation()
        {
            if (m_RotationLocked || !m_RotateCommands.Any())
                return;
            var cmd = m_RotateCommands.Dequeue();
            m_RotateCommandsCount--;
            m_RotationLocked = true;
            RotateCommand?.Invoke(cmd);
        }
        
        #endregion
    }
}