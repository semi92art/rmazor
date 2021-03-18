using Utils;

namespace Games.RazorMaze.Models
{
    public class MazeModel : IMazeModel
    {
        public event MazeInfoHandler MazeChanged;
        public event MazeOrientationHandler RotationStarted;
        public event MazeRotationHandler Rotation;
        public event NoArgsHandler RotationFinished;

        private MazeInfo m_Info;

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = value;
                MazeChanged?.Invoke(m_Info, Orientation);
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
            RotationStarted?.Invoke(_Direction, Orientation);
            Coroutines.Run(Coroutines.Lerp(
                0f, 
                1f, 
                0.2f, 
                _Val => Rotation?.Invoke(_Val),
                GameTimeProvider.Instance, 
                () => RotationFinished?.Invoke()));
        }
    }
}