namespace Games.RazorMaze.Models.InputSchedulers
{
    public delegate void InputCommandHandler(int _Command, object[] _Args = null);

    public interface IAddCommand
    {
        void AddCommand(int _Command, params object[] _Args);
    }
    
    public interface IInputScheduler : IInputSchedulerGameProceeder, IInputSchedulerUiProceeder { }
    
    public class InputScheduler : IInputScheduler
    {
        #region inject
        
        private IInputSchedulerGameProceeder InputSchedulerGameProceeder { get; }
        private IInputSchedulerUiProceeder InputSchedulerUiProceeder { get; }

        public InputScheduler(
            IInputSchedulerGameProceeder _InputSchedulerGameProceeder,
            IInputSchedulerUiProceeder _InputSchedulerUiProceeder)
        {
            InputSchedulerGameProceeder = _InputSchedulerGameProceeder;
            InputSchedulerUiProceeder = _InputSchedulerUiProceeder;

            InputSchedulerGameProceeder.MoveCommand += (_Command, _Args) =>
                MoveCommand?.Invoke(_Command, _Args);
            InputSchedulerGameProceeder.RotateCommand += (_Command, _Args) =>
                RotateCommand?.Invoke(_Command, _Args);
            InputSchedulerUiProceeder.UiCommand += (_Command, _Args) =>
                UiCommand?.Invoke(_Command, _Args);
        }
        
        #endregion
        
        #region api
        
        public event InputCommandHandler MoveCommand;
        public event InputCommandHandler RotateCommand;
        public event InputCommandHandler UiCommand;

        public void AddCommand(int _Command, object[] _Args = null)
        {
            InputSchedulerGameProceeder.AddCommand(_Command, _Args);
            InputSchedulerUiProceeder.AddCommand(_Command, _Args);
        }

        public void UnlockMovement(bool _Unlock) => InputSchedulerGameProceeder.UnlockMovement(_Unlock);
        public void UnlockRotation(bool _Unlock) => InputSchedulerGameProceeder.UnlockRotation(_Unlock);
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            bool unlockMoveAndRot = _Args.Stage == ELevelStage.StartedOrContinued 
                                  || _Args.Stage == ELevelStage.ReadyToStartOrContinue;
            UnlockMovement(unlockMoveAndRot);
            UnlockRotation(unlockMoveAndRot);
            InputSchedulerGameProceeder.OnLevelStageChanged(_Args);
            InputSchedulerUiProceeder.OnLevelStageChanged(_Args);
        }

        #endregion


    }
}