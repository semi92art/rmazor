using Games.RazorMaze.Models;
using UnityEngine;
using UnityGameLoopDI;

namespace Games.RazorMaze.Views
{
    public class RazorMazeInputConfiguratorProt : UnityGameLoopObjectDI, IInputConfigurator, IOnUpdate 
    {
        public event IntHandler OnCommand;
        public void ConfigureInput() { }

        public void OnUpdate()
        {
            if (OnCommand == null) 
                return;
            if (Input.GetKeyDown(KeyCode.A))
                OnCommand.Invoke((int)EInputCommand.MoveLeft);
            else if (Input.GetKeyDown(KeyCode.D))
                OnCommand.Invoke((int)EInputCommand.MoveRight);
            else if (Input.GetKeyUp(KeyCode.W))
                OnCommand.Invoke((int)EInputCommand.MoveUp);
            else if (Input.GetKeyDown(KeyCode.S))
                OnCommand.Invoke((int)EInputCommand.MoveDown);
            else if (Input.GetKeyDown(KeyCode.E))
                OnCommand.Invoke((int)EInputCommand.RotateClockwise);
            else if (Input.GetKeyDown(KeyCode.Q))
                OnCommand.Invoke((int)EInputCommand.RotateCounterClockwise);
            
            if (Input.anyKeyDown)
                Utils.Dbg.Log("Key pressed!!!");
        }
    }
}