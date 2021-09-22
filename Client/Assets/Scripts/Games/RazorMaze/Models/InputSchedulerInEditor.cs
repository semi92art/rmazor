using System;
using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Ticker;

namespace Games.RazorMaze.Models
{
    public delegate void InputCommandHandler(int _Command, object[] _Args = null);
    
    public interface IInputScheduler
    {
        event InputCommandHandler MoveCommand; 
        event InputCommandHandler RotateCommand;
        event InputCommandHandler OtherCommand;
        void AddCommand(int _Command, params object[] _Args);
        void UnlockMovement(bool _Unlock);
        void UnlockRotation(bool _Unlock);
    }
    
    public class InputSchedulerInEditor : IInputScheduler, IUpdateTick
    {
        #region nonpublic members
        
        private readonly Queue<EInputCommand> m_MoveCommands = new Queue<EInputCommand>();
        private readonly Queue<EInputCommand> m_RotateCommands = new Queue<EInputCommand>();
        private readonly Queue<Tuple<EInputCommand, object[]>> m_OtherCommands = new Queue<Tuple<EInputCommand, object[]>>();

        private bool m_MovementLocked;
        private bool m_RotationLocked;
        private int m_MoveCommandsCount;
        private int m_RotateCommandsCount;
        private int m_OtherCommandsCount;
        private int m_MaxCommandsCount;
        
        #endregion
        
        #region inject

        public InputSchedulerInEditor(IUITicker _UITicker)
        {
            _UITicker.Register(this);
        }
        
        #endregion
        
        #region api
        
        public event InputCommandHandler MoveCommand;
        public event InputCommandHandler RotateCommand;
        public event InputCommandHandler OtherCommand;

        public void AddCommand(int _Command, object[] _Args = null)
        {
            switch (_Command)
            {
                case (int)EInputCommand.MoveDown:
                case (int)EInputCommand.MoveLeft:
                case (int)EInputCommand.MoveRight:
                case (int)EInputCommand.MoveUp:
                    if (m_MoveCommandsCount >= 3) return;
                    m_MoveCommands.Enqueue((EInputCommand)_Command);
                    m_MoveCommandsCount++;
                    break;
                case (int)EInputCommand.RotateClockwise:
                case (int)EInputCommand.RotateCounterClockwise:
                    if (m_RotateCommandsCount >= 3) return;
                    m_RotateCommands.Enqueue((EInputCommand)_Command);
                    m_RotateCommandsCount++;
                    break;
                case (int)EInputCommand.LoadLevel:
                case (int)EInputCommand.ReadyToContinueLevel:
                case (int)EInputCommand.ContinueLevel:
                case (int)EInputCommand.FinishLevel:
                case (int)EInputCommand.PauseLevel:
                case (int)EInputCommand.UnloadLevel:
                    m_OtherCommands.Enqueue(new Tuple<EInputCommand, object[]>((EInputCommand)_Command, _Args));
                    m_OtherCommandsCount++;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Command);
            }
        }

        public void UnlockMovement(bool _Unlock) => m_MovementLocked = !_Unlock;
        public void UnlockRotation(bool _Unlock) => m_RotationLocked = !_Unlock;

        public void UpdateTick()
        {
            ScheduleMovementCommands();
            ScheduleRotationCommands();
            ScheduleOtherCommands();
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
            MoveCommand?.Invoke((int)cmd);
        }

        private void ScheduleRotationCommands()
        {
            if (m_RotationLocked || !m_RotateCommands.Any())
                return;
            var cmd = m_RotateCommands.Dequeue();
            m_RotateCommandsCount--;
            m_RotationLocked = true;
            RotateCommand?.Invoke((int)cmd);
        }

        private void ScheduleOtherCommands()
        {
            if (!m_OtherCommands.Any())
                return;
            var cmd = m_OtherCommands.Dequeue();
            m_OtherCommandsCount--;
            OtherCommand?.Invoke((int)cmd.Item1);
        }
        
        #endregion
    }
}