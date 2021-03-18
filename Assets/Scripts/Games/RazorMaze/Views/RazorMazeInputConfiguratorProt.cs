﻿using Games.RazorMaze.Models;
using UnityEngine;
using UnityGameLoopDI;

namespace Games.RazorMaze.Views
{
    public class RazorMazeInputConfiguratorProt : UnityGameLoopObjectDI, IInputConfigurator, IOnUpdate 
    {
        public event IntHandler Command;
        public void ConfigureInput() { }
        
        public void OnUpdate()
        {
            if (Command == null) 
                return;
            if (Input.GetKeyDown(KeyCode.A))
                Command.Invoke((int)EInputCommand.MoveLeft);
            else if (Input.GetKeyDown(KeyCode.D))
                Command.Invoke((int)EInputCommand.MoveRight);
            else if (Input.GetKeyUp(KeyCode.W))
                Command.Invoke((int)EInputCommand.MoveUp);
            else if (Input.GetKeyDown(KeyCode.S))
                Command.Invoke((int)EInputCommand.MoveDown);
            else if (Input.GetKeyDown(KeyCode.E))
                Command.Invoke((int)EInputCommand.RotateClockwise);
            else if (Input.GetKeyDown(KeyCode.Q))
                Command.Invoke((int)EInputCommand.RotateCounterClockwise);
        }
    }
}