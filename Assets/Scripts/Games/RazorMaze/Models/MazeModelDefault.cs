using System;
using UnityGameLoopDI;
using Utils;

namespace Games.RazorMaze.Models
{
    public class MazeModelDefault : UnityGameLoopObjectDI, IMazeModel
    {
        public event MazeInfoHandler OnMazeChanged;
        public event MazeOrientationHandler OnRotationStarted;
        public event MazeRotationHandler OnRotation;
        public event MazeOrientationHandler OnRotationFinished;

        private MazeInfo m_Info;

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = value;
                OnMazeChanged?.Invoke(m_Info);
            }
        }
        public MazeOrientation Orientation { get; private set; } = MazeOrientation.North;

        public void Rotate(MazeRotateDirection _Direction)
        {
            int orient = (int) Orientation;
            switch (_Direction)
            {
                case MazeRotateDirection.Clockwise:
                    orient++;
                    if (orient > 3) orient = 0;
                    break;
                case MazeRotateDirection.CounterClockwise:
                    orient--;
                    if (orient < 0) orient = 3;
                    break;
            }
            Orientation = (MazeOrientation) orient;
            OnRotationStarted?.Invoke(_Direction, Orientation);
            Coroutines.Run(Coroutines.Lerp(
                0f, 
                1f, 
                0.3f, 
                _Val => OnRotation?.Invoke(_Val),
                GameTimeProvider.Instance, 
                () => OnRotationFinished?.Invoke(_Direction, Orientation)));
        }

        [Obsolete("This method")]
        public void OnUpdate()
        {
            //TODO
        }
    }
}