using System.Linq;
using Common.Entities;
using mazing.common.Runtime.Entities;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Models
{
    public enum EMazeOrientation { North, East, South, West }
    public enum EDirection { Up, Right, Down, Left }
    
    public interface IModelData
    {
        public V2Int[] PathItems          { get; }
        MazeInfo       Info               { get; set; }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModelData : IModelData
    {

        #region nonpublic members
        
        private MazeInfo m_Info;

        #endregion

        #region inject
        
        private IModelMazeInfoCorrector MazeInfoCorrector { get; }

        private ModelData(IModelMazeInfoCorrector _MazeInfoCorrector)
        {
            MazeInfoCorrector = _MazeInfoCorrector;
        }

        #endregion

        #region api
        
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