using Utils;

namespace Games.RazorMaze.Models
{
    public enum MazeRotateDirection {Clockwise, CounterClockwise}
    public delegate void MazeOrientationHandler(MazeRotateDirection Direction, MazeOrientation Orientation);
    
    public interface IModelMazeRotation
    {
        event MazeOrientationHandler RotationStarted;
        event FloatHandler Rotation;
        event MazeOrientationHandler RotationFinished;
        void Rotate(MazeRotateDirection _Direction);
    }
    
    public class ModelMazeRotation : IModelMazeRotation
    {
        private RazorMazeModelSettings Settings { get; }
        private IModelMazeData Data { get; }

        public ModelMazeRotation(
            RazorMazeModelSettings _Settings,
            IModelMazeData _Data)
        {
            Settings = _Settings;
            Data = _Data;
        }
        
        
        public event MazeOrientationHandler RotationStarted;
        public event FloatHandler Rotation;
        public event MazeOrientationHandler RotationFinished;
        
        
        public void Rotate(MazeRotateDirection _Direction)
        {
            int orient = (int) Data.Orientation;
            int addict = _Direction == MazeRotateDirection.Clockwise ? 1 : -1;
            orient = MathUtils.ClampInverse(orient + addict, 0, 3);
            Data.Orientation = (MazeOrientation) orient;
            RotationStarted?.Invoke(_Direction, Data.Orientation);
            Coroutines.Run(Coroutines.Lerp(
                0f, 
                1f, 
                1 / Settings.mazeRotateSpeed, 
                _Val => Rotation?.Invoke(_Val),
                GameTimeProvider.Instance, 
                (_Stopped, _Progress) =>
                {
                    RotationFinished?.Invoke(_Direction, Data.Orientation);
                }));
        }
    }
}