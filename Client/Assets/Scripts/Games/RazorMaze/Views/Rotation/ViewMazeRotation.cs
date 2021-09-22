using System;
using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.Rotation
{
    public class ViewMazeRotation : IViewMazeRotation
    {
        public event NoArgsHandler Initialized;
        
        public void Init()
        {
            throw new NotImplementedException();
        }

        public void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            throw new NotImplementedException();
        }

        public void Rotate(float _Progress)
        {
            throw new NotImplementedException();
        }

        public void FinishRotation()
        {
            throw new NotImplementedException();
        }
    }
}