using Games.RazorMaze.Models;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public class RazorMazeInputConfiguratorProt : IInputConfigurator, IUpdateTick
    {
        #region inject

        public RazorMazeInputConfiguratorProt(ITicker _Ticker) => _Ticker.Register(this);
        
        #endregion

        #region api

        public bool Locked { get; set; }
        public event IntHandler Command;
        public event NoArgsHandler Initialized;

        public void Init()
        {
            Initialized?.Invoke();
        }

        public void UpdateTick()
        {
            if (Locked || Command == null) 
                return;
            EInputCommand? commandKey = null;
            
            if (Input.GetKeyDown(KeyCode.A))
                commandKey = EInputCommand.MoveLeft;
            else if (Input.GetKeyDown(KeyCode.D))
                commandKey = EInputCommand.MoveRight;
            else if (Input.GetKeyUp(KeyCode.W))
                commandKey = EInputCommand.MoveUp;
            else if (Input.GetKeyDown(KeyCode.S))
                commandKey = EInputCommand.MoveDown;
            else if (Input.GetKeyDown(KeyCode.E))
                commandKey = EInputCommand.RotateClockwise;
            else if (Input.GetKeyDown(KeyCode.Q))
                commandKey = EInputCommand.RotateCounterClockwise;
            else if (Input.GetKeyDown(KeyCode.L))
                commandKey = EInputCommand.LoadLevel;
            else if (Input.GetKeyDown(KeyCode.R))
                commandKey = EInputCommand.ReadyToContinueLevel;
            else if (Input.GetKeyDown(KeyCode.C))
                commandKey = EInputCommand.ContinueLevel;
            else if (Input.GetKeyDown(KeyCode.P))
                commandKey = EInputCommand.PauseLevel;
            else if (Input.GetKeyDown(KeyCode.F))
                commandKey = EInputCommand.FinishLevel;
            else if (Input.GetKeyDown(KeyCode.U))
                commandKey = EInputCommand.UnloadLevel;
                
            if (commandKey.HasValue)
                Command.Invoke((int)commandKey.Value);
        }

        #endregion

    }
}