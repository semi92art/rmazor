
using System;
using System.Collections.Generic;
using System.Linq;
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
    
    public interface IModelMazeData : IPreInit
    {
        event MazeInfoHandler MazeChanged;
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; set; }
        Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>> ProceedInfos { get; }
        CharacterInfo CharacterInfo { get; }
        bool ProceedingMazeItems { get; set; }
        bool ProceedingControls { get; set; }
    }
    
    public class ModelMazeData : IModelMazeData
    {
        #region nonpublic members
        
        private MazeInfo m_Info;

        #endregion
        
        #region constructor
        
        public void PreInit()
        {
            ProceedInfos = new Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>>();
            var types = Enum.GetValues(typeof(EMazeItemType)).Cast<EMazeItemType>();
            foreach (var type in types)
                ProceedInfos.Add(type, new Dictionary<MazeItem, IMazeItemProceedInfo>());
        }
        
        #endregion

        #region api
        
        public event MazeInfoHandler MazeChanged;
        public MazeOrientation Orientation { get; set; } = MazeOrientation.North;
        public Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>> ProceedInfos { get; private set; } 
            = new Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>>();

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