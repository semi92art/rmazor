using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public class ViewInputConfiguratorInEditor : ViewInputConfigurator, IUpdateTick
    {
        #region inject

        public ViewInputConfiguratorInEditor(
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
            if (Locked) 
                return;
            int? commandKey = null;
            ProceedMovement(ref commandKey);
            ProceedRotation(ref commandKey);
            ProceedLevelStatement(ref commandKey);
                
            if (commandKey.HasValue)
                RaiseCommand(commandKey.Value, null);
        }

        #endregion

        #region nonpublic methods

        private static void ProceedMovement(ref int? _CommandKey)
        {
            if (Input.GetKeyDown(KeyCode.A))
                _CommandKey = InputCommands.MoveLeft;
            else if (Input.GetKeyDown(KeyCode.D))
                _CommandKey = InputCommands.MoveRight;
            else if (Input.GetKeyUp(KeyCode.W))
                _CommandKey = InputCommands.MoveUp;
            else if (Input.GetKeyDown(KeyCode.S))
                _CommandKey = InputCommands.MoveDown;
        }

        private static void ProceedRotation(ref int? _CommandKey)
        {
            if (Input.GetKeyDown(KeyCode.E))
                _CommandKey = InputCommands.RotateClockwise;
            else if (Input.GetKeyDown(KeyCode.Q))
                _CommandKey = InputCommands.RotateCounterClockwise;
        }

        private static void ProceedLevelStatement(ref int? _CommandKey)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
                    _CommandKey = InputCommands.LoadRandomLevel;
                else if (Input.GetKey(KeyCode.LeftShift))
                    _CommandKey = InputCommands.LoadNextLevel;
                else if (Input.GetKey(KeyCode.LeftAlt))
                    _CommandKey = InputCommands.LoadFirstLevelFromCurrentGroup;
                else
                    _CommandKey = InputCommands.LoadCurrentLevel;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                _CommandKey = InputCommands.ReadyToContinueLevel;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                _CommandKey = InputCommands.ContinueLevel;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                _CommandKey = InputCommands.FinishLevel;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                _CommandKey = InputCommands.UnloadLevel;
                
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                _CommandKey = InputCommands.PauseLevel;
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                _CommandKey = InputCommands.KillCharacter;
        }

        #endregion

    }
}