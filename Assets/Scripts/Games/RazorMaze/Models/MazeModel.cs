using Entities;
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
                MazeChanged?.Invoke(m_Info = value, Orientation);
                MazeTransformer.StartProcessItems(m_Info);
            }
        }

        public MazeOrientation Orientation { get; private set; } = MazeOrientation.North;

        #region inject
        
        private ICharacterModel CharacterModel { get; }
        private IMazeTransformer MazeTransformer { get; }
        private RazorMazeModelSettings Settings { get; }

        public MazeModel(ICharacterModel _CharacterModel, IMazeTransformer _MazeTransformer, RazorMazeModelSettings _Settings)
        {
            CharacterModel = _CharacterModel;
            MazeTransformer = _MazeTransformer;
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
                (_Breaked, _Progress) =>
                {
                    RotationFinished?.Invoke();
                    MazeTransformer.TransformItemsAfterMazeRotate(Orientation);
                }));
        }

        #endregion
    }
}