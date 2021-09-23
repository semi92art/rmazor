using System;

namespace Games.RazorMaze.Models
{
    public class InputSchedulerInGame : IInputScheduler
    {
        public event InputCommandHandler MoveCommand;
        public event InputCommandHandler RotateCommand;
        public event InputCommandHandler OtherCommand;


        public void AddCommand(int _Command, object[] _Args = null)
        {
            throw new NotImplementedException();
        }

        public void UnlockMovement(bool _Unlock)
        {
            throw new NotImplementedException();
        }

        public void UnlockRotation(bool _Unlock)
        {
            throw new NotImplementedException();
        }
    }
}