
using System.Collections.Generic;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Models
{
    public delegate void MazeInfoHandler(MazeInfo Info);
    public enum MazeOrientation { North, East, South, West}
    public enum MazeMoveDirection { Up, Right, Down, Left}

    public class CharacterInfo
    {
        public V2Int Position { get; set; }
        public MazeMoveDirection MoveDirection { get; set; }
        public long HealthPoints { get; set; }
    }
    
    public interface IModelMazeData
    {
        event MazeInfoHandler MazeChanged;
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; set; }
        Dictionary<MazeItem, IMazeItemProceedInfo> ProceedInfos { get; }
        CharacterInfo CharacterInfo { get; }
        bool ProceedingMazeItems { get; set; }
        bool ProceedingControls { get; set; }
    }
    
    public class ModelMazeData : IModelMazeData
    {
        #region nonpublic members
        
        private MazeInfo m_Info;

        #endregion
        
        #region inject
        
        private RazorMazeModelSettings Settings { get; }

        public ModelMazeData(RazorMazeModelSettings _Settings)
        {
            Settings = _Settings;
        }
        
        #endregion
        
        #region api
        
        public event MazeInfoHandler MazeChanged;
        public MazeOrientation Orientation { get; set; } = MazeOrientation.North;
        public Dictionary<MazeItem, IMazeItemProceedInfo> ProceedInfos { get; } 
            = new Dictionary<MazeItem, IMazeItemProceedInfo>();

        public CharacterInfo CharacterInfo { get; } = new CharacterInfo();
        public bool ProceedingMazeItems { get; set; }
        public bool ProceedingControls { get; set; }

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = value;
                MazeChanged?.Invoke(m_Info);
            }
        }
        
        #endregion
    }
}