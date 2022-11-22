using System.Collections.Generic;
using Common.Helpers;
using UnityEngine.Events;

namespace RMAZOR.Models.InputSchedulers
{
    public interface IAddCommand
    {
        void AddCommand(EInputCommand _Command, Dictionary<string, object> _Args = null);
    }
    
    public interface IInputScheduler : IInputSchedulerGameProceeder, IInputSchedulerUiProceeder { }
    
    public class InputScheduler : InitBase, IInputScheduler
    {
        #region inject
        
        private IInputSchedulerGameProceeder InputSchedulerGameProceeder { get; }
        private IInputSchedulerUiProceeder   InputSchedulerUiProceeder { get; }

        private InputScheduler(
            IInputSchedulerGameProceeder _InputSchedulerGameProceeder,
            IInputSchedulerUiProceeder   _InputSchedulerUiProceeder)
        {
            InputSchedulerGameProceeder = _InputSchedulerGameProceeder;
            InputSchedulerUiProceeder   = _InputSchedulerUiProceeder;

            InputSchedulerGameProceeder.MoveCommand += (_Command, _Args) =>
                MoveCommand?.Invoke(_Command, _Args);
            InputSchedulerGameProceeder.RotateCommand += (_Command, _Args) =>
                RotateCommand?.Invoke(_Command, _Args);
            InputSchedulerUiProceeder.UiCommand += (_Command, _Args) =>
                UiCommand?.Invoke(_Command, _Args);
        }
        
        #endregion
        
        #region api
        
        public event UnityAction<EInputCommand, Dictionary<string, object>> MoveCommand;
        public event UnityAction<EInputCommand, Dictionary<string, object>> RotateCommand;
        public event UnityAction<EInputCommand, Dictionary<string, object>> UiCommand;

        public override void Init()
        {
            InputSchedulerGameProceeder.Init();
            InputSchedulerUiProceeder.Init();
            base.Init();
        }

        public void AddCommand(EInputCommand _Command, Dictionary<string, object> _Args = null)
        {
            InputSchedulerGameProceeder.AddCommand(_Command, _Args);
            InputSchedulerUiProceeder.AddCommand(_Command, _Args);
        }

        public void LockMovement(bool _Lock) => InputSchedulerGameProceeder.LockMovement(_Lock);
        public void LockRotation(bool _Lock) => InputSchedulerGameProceeder.LockRotation(_Lock);
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            bool @lock = _Args.LevelStage != ELevelStage.StartedOrContinued 
                        && _Args.LevelStage != ELevelStage.ReadyToStart;
            LockMovement(@lock);
            LockRotation(@lock);
            InputSchedulerGameProceeder.OnLevelStageChanged(_Args);
        }

        #endregion
        
    }
}