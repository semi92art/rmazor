using Ticker;
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
        private ModelSettings Settings { get; }
        private IModelMazeData Data { get; }
        private IGameTicker GameTicker { get; }

        public ModelMazeRotation(
            ModelSettings _Settings,
            IModelMazeData _Data,
            IGameTicker _GameTicker)
        {
            Settings = _Settings;
            Data = _Data;
            GameTicker = _GameTicker;
        }
        
        
        public event MazeOrientationHandler RotationStarted;
        public event FloatHandler Rotation;
        public event MazeOrientationHandler RotationFinished;
        
        
        public void Rotate(MazeRotateDirection _Direction)
        {
            if (!Data.ProceedingControls)
                return;
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
                GameTicker, 
                (_Stopped, _Progress) =>
                {
                    RotationFinished?.Invoke(_Direction, Data.Orientation);
                }));
        }
    }
}