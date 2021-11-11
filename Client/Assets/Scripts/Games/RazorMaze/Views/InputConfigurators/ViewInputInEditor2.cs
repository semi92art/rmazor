using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
using UnityEngine.InputSystem;


namespace Games.RazorMaze.Views.InputConfigurators
{
    public class ViewInputInEditor2 : ViewInput, IUpdateTick
    {
        #region nonpublic members

        private static Keyboard Kb => Keyboard.current;


        #endregion
        
        #region inject

        public ViewInputInEditor2(
            IUITicker _UITicker,
            IModelGame _Model,
            IContainersGetter _ContainersGetter) 
            : base(_Model, _ContainersGetter)
        {
            _UITicker.Register(this);
        }

        #endregion

        #region api
        
        public void UpdateTick()
        {
            bool forced = false;
            int? commandKey = null;
            ProceedMovement(ref commandKey);
            ProceedRotation(ref commandKey);
            ProceedLevelStatement(ref commandKey, ref forced);
            if (commandKey.HasValue)
                RaiseCommand(commandKey.Value, null, forced);
        }

        #endregion

        #region nonpublic methods

        private static void ProceedMovement(ref int? _CommandKey)
        {
            if (Kb.aKey.wasPressedThisFrame)
                _CommandKey = InputCommands.MoveLeft;
            else if (Kb.dKey.wasPressedThisFrame)
                _CommandKey = InputCommands.MoveRight;
            else if (Kb.wKey.wasPressedThisFrame)
                _CommandKey = InputCommands.MoveUp;
            else if (Kb.sKey.wasPressedThisFrame)
                _CommandKey = InputCommands.MoveDown;
        }

        private static void ProceedRotation(ref int? _CommandKey)
        {
            if (Kb.eKey.wasPressedThisFrame)
                _CommandKey = InputCommands.RotateClockwise;
            else if (Kb.qKey.wasPressedThisFrame)
                _CommandKey = InputCommands.RotateCounterClockwise;
        }

        private static void ProceedLevelStatement(ref int? _CommandKey, ref bool _Forced)
        {
            if (Kb.digit1Key.wasPressedThisFrame)
            {
                if (Kb.leftShiftKey.isPressed && Kb.leftAltKey.isPressed && Kb.xKey.isPressed)
                    _CommandKey = InputCommands.LoadRandomLevelWithRotation;
                else if (Kb.leftShiftKey.isPressed && Kb.leftAltKey.isPressed)
                    _CommandKey = InputCommands.LoadRandomLevel;
                else if (Kb.leftShiftKey.isPressed)
                    _CommandKey = InputCommands.LoadNextLevel;
                else if (Kb.leftAltKey.isPressed)
                    _CommandKey = InputCommands.LoadFirstLevelFromCurrentGroup;
                else
                    _CommandKey = InputCommands.LoadCurrentLevel;
                _Forced = true;
            }
            else if (Kb.digit2Key.wasPressedThisFrame)
                (_CommandKey, _Forced) = (InputCommands.ReadyToStartLevel, true);
            else if (Kb.digit3Key.wasPressedThisFrame)
                (_CommandKey, _Forced) = (InputCommands.StartOrContinueLevel, true);
            else if (Kb.digit4Key.wasPressedThisFrame)
                (_CommandKey, _Forced) = (InputCommands.FinishLevel, true);
            else if (Kb.digit5Key.wasPressedThisFrame)
                (_CommandKey, _Forced) = (InputCommands.UnloadLevel, true);
                
            else if (Kb.digit6Key.wasPressedThisFrame)
                (_CommandKey, _Forced) = (InputCommands.PauseLevel, true);
            else if (Kb.digit7Key.wasPressedThisFrame)
                (_CommandKey, _Forced) = (InputCommands.KillCharacter, true);
        }

        #endregion

    }
}