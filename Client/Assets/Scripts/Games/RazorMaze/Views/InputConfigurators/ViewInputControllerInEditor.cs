using Games.RazorMaze.Models;
using Lean.Common;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public class ViewInputControllerInEditor : ViewInputController, IUpdateTick
    {
        #region inject
        
        private IUITicker UITicker { get; }

        public ViewInputControllerInEditor(
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewInputTouchProceeder _TouchProceeder,
            IUITicker _UITicker) 
            : base(_CommandsProceeder, _TouchProceeder)
        {
            UITicker = _UITicker;
        }

        #endregion

        #region api

        public override void Init()
        {
            UITicker.Register(this);
            base.Init();
        }

        public void UpdateTick()
        {
            bool forced = false;
            EInputCommand? commandKey = null;
            ProceedMovement(ref commandKey);
            ProceedRotation(ref commandKey);
            ProceedLevelStatement(ref commandKey, ref forced);
            if (commandKey.HasValue)
                CommandsProceeder.RaiseCommand(commandKey.Value, null, forced);
        }

        #endregion

        #region nonpublic methods

        private static void ProceedMovement(ref EInputCommand? _CommandKey)
        {
            if (LeanInput.GetDown(KeyCode.A))
                _CommandKey = EInputCommand.MoveLeft;
            else if (LeanInput.GetDown(KeyCode.D))
                _CommandKey = EInputCommand.MoveRight;
            else if (LeanInput.GetDown(KeyCode.W))
                _CommandKey = EInputCommand.MoveUp;
            else if (LeanInput.GetDown(KeyCode.S))
                _CommandKey = EInputCommand.MoveDown;
        }

        private static void ProceedRotation(ref EInputCommand? _CommandKey)
        {
            if (LeanInput.GetDown(KeyCode.E))
                _CommandKey = EInputCommand.RotateClockwise;
            else if (LeanInput.GetDown(KeyCode.Q))
                _CommandKey = EInputCommand.RotateCounterClockwise;
        }

        private static void ProceedLevelStatement(ref EInputCommand? _CommandKey, ref bool _Forced)
        {
            if (LeanInput.GetDown(KeyCode.Alpha1))
            {
                if (LeanInput.GetPressed(KeyCode.LeftShift) && LeanInput.GetPressed(KeyCode.LeftAlt) && LeanInput.GetPressed(KeyCode.X))
                    _CommandKey = EInputCommand.LoadRandomLevelWithRotation;
                else if (LeanInput.GetPressed(KeyCode.LeftShift) && LeanInput.GetPressed(KeyCode.LeftAlt))
                    _CommandKey = EInputCommand.LoadRandomLevel;
                else if (LeanInput.GetPressed(KeyCode.LeftShift))
                    _CommandKey = EInputCommand.LoadNextLevel;
                else if (LeanInput.GetPressed(KeyCode.LeftAlt))
                    _CommandKey = EInputCommand.LoadFirstLevelFromCurrentGroup;
                else
                    _CommandKey = EInputCommand.LoadCurrentLevel;
                _Forced = true;
            }
            else if (LeanInput.GetDown(KeyCode.Alpha2))
                (_CommandKey, _Forced) = (EInputCommand.ReadyToStartLevel, true);
            else if (LeanInput.GetDown(KeyCode.Alpha3))
                (_CommandKey, _Forced) = (EInputCommand.StartOrContinueLevel, true);
            else if (LeanInput.GetDown(KeyCode.Alpha4))
                (_CommandKey, _Forced) = (EInputCommand.FinishLevel, true);
            else if (LeanInput.GetDown(KeyCode.Alpha5))
                (_CommandKey, _Forced) = (EInputCommand.ReadyToUnloadLevel, true);
            else if (LeanInput.GetDown(KeyCode.Alpha6))
                (_CommandKey, _Forced) = (EInputCommand.UnloadLevel, true);
            else if (LeanInput.GetDown(KeyCode.Alpha7))
                (_CommandKey, _Forced) = (EInputCommand.PauseLevel, true);
            else if (LeanInput.GetDown(KeyCode.Alpha8))
                (_CommandKey, _Forced) = (EInputCommand.KillCharacter, true);
        }

        #endregion

    }
}