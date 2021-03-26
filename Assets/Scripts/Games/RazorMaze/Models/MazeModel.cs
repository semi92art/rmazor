using Utils;

namespace Games.RazorMaze.Models
{
    public class MazeModel : IMazeModel
    {
        #region nonpublic members
        
        private MazeInfo m_Info;

        #endregion
        
        #region api
        
        public event MazeInfoHandler MazeChanged;
        public event MazeOrientationHandler RotationStarted;
        public event FloatHandler Rotation;
        public event NoArgsHandler RotationFinished;

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = value;
                MazeChanged?.Invoke(m_Info, Orientation);
                MazeMovingItemsProceeder.StartProcessItems(m_Info);
                MazeTrapsReactProceeder.OnMazeChanged(m_Info);
            }
        }

        public MazeOrientation Orientation { get; private set; } = MazeOrientation.North;

        #region inject
        
        private IMazeMovingItemsProceeder MazeMovingItemsProceeder { get; }
        private IMazeTrapsReactProceeder MazeTrapsReactProceeder { get; }
        private RazorMazeModelSettings Settings { get; }

        public MazeModel(
            IMazeMovingItemsProceeder _MazeMovingItemsProceeder, 
            IMazeTrapsReactProceeder _MazeTrapsReactProceeder,
            RazorMazeModelSettings _Settings)
        {
            MazeMovingItemsProceeder = _MazeMovingItemsProceeder;
            MazeTrapsReactProceeder = _MazeTrapsReactProceeder;
            Settings = _Settings;
        }
        
        #endregion
        
        public void Rotate(MazeRotateDirection _Direction)
        {
            int orient = (int) Orientation;
            int addict = _Direction == MazeRotateDirection.Clockwise ? 1 : -1;
            orient = MathUtils.ClampInverse(orient + addict, 0, 3);
            Orientation = (MazeOrientation) orient;
            RotationStarted?.Invoke(_Direction, Orientation);
            Coroutines.Run(Coroutines.Lerp(
                0f, 
                1f, 
                1 / Settings.mazeRotateSpeed, 
                _Val => Rotation?.Invoke(_Val),
                GameTimeProvider.Instance, 
                (_Stopped, _Progress) =>
                {
                    RotationFinished?.Invoke();
                    MazeMovingItemsProceeder.TransformItemsAfterMazeRotate(Orientation);
                }));
        }

        #endregion

    }
}