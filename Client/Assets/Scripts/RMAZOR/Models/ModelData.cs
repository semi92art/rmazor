using System.Linq;
using Common.Entities;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Models
{
    public enum MazeOrientation { North, East, South, West }
    public enum EMazeMoveDirection { Up, Right, Down, Left }
    
    public interface IModelData
    {
        public V2Int[]  PathItems          { get; }
        MazeInfo        Info               { get; set; }
        MazeOrientation Orientation        { get; set; }
        bool            ProceedingControls { get; set; }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModelData : IModelData
    {

        #region nonpublic members
        
        private MazeInfo m_Info;

        #endregion

        #region inject
        
        private IModelMazeInfoCorrector MazeInfoCorrector { get; }

        public ModelData(IModelMazeInfoCorrector _MazeInfoCorrector)
        {
            MazeInfoCorrector = _MazeInfoCorrector;
        }

        #endregion

        #region api
        
        public MazeOrientation Orientation { get; set; } = MazeOrientation.North;
        public bool ProceedingControls { get; set; } = true;

        public V2Int[] PathItems { get; private set; }

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = MazeInfoCorrector.GetCorrectedMazeInfo(value);
                PathItems = m_Info.PathItems.Select(_PI => _PI.Position).ToArray();
            }
        }

        #endregion
    }
}