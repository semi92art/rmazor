using Ticker;
using Utils;

namespace Games.RazorMaze.Models
{
    public enum MazeRotateDirection {Clockwise, CounterClockwise}
    public delegate void MazeOrientationHandler(MazeRotateDirection Direction, MazeOrientation Orientation);
    
    public interface IModelMazeRotation
    {
        event MazeOrientationHandler RotationStarted;
        event MazeOrientationHandler RotationFinishedInternal;
        void StartRotation(MazeRotateDirection _Direction);
        void OnRotationFinished(MazeRotateDirection _Direction, MazeOrientation _Orientation);
    }
    
    public class ModelMazeRotation : IModelMazeRotation
    {
        private ModelSettings Settings { get; }
        private IModelData Data { get; }
        private IGameTicker GameTicker { get; }

        public ModelMazeRotation(
            ModelSettings _Settings,
            IModelData _Data,
            IGameTicker _GameTicker)
        {
            Settings = _Settings;
            Data = _Data;
            GameTicker = _GameTicker;
        }
        
        public event MazeOrientationHandler RotationStarted;
        public event MazeOrientationHandler RotationFinishedInternal;
        
        
        public void StartRotation(MazeRotateDirection _Direction)
        {
            if (!Data.ProceedingControls)
                return;
            RotationStarted?.Invoke(_Direction, Data.Orientation);
        }

        public void OnRotationFinished(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            Data.Orientation = _Orientation;
            RotationFinishedInternal?.Invoke(_Direction, _Orientation);
        }
    }
}