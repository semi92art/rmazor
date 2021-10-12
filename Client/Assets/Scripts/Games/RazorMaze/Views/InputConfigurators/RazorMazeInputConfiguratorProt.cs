using Games.RazorMaze.Models;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public class RazorMazeInputConfiguratorProt : IInputConfigurator, IUpdateTick
    {
        #region inject

        public RazorMazeInputConfiguratorProt(IUITicker _UITicker) => _UITicker.Register(this);
        
        #endregion

        #region api

        public bool Locked { get; set; }
        public event IntHandlerWithArgs Command;
        public event NoArgsHandler Initialized;

        public void Init()
        {
            Initialized?.Invoke();
        }

        public void UpdateTick()
        {
            if (Locked || Command == null) 
                return;
            int? commandKey = null;
            ProceedMovement(ref commandKey);
            ProceedRotation(ref commandKey);
            ProceedLevelStatement(ref commandKey);
                
            if (commandKey.HasValue)
                Command?.Invoke(commandKey.Value, null);
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
                if (Input.GetKey(KeyCode.LeftShift))
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