using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Models
{
    public enum MazeOrientation { North, East, South, West }
    public enum EMazeMoveDirection { Up, Right, Down, Left }
    
    public delegate void MazeInfoHandler(MazeInfo Info);

    public class CharacterInfo
    {
        public V2Int Position { get; set; }
        public EMazeMoveDirection MoveDirection { get; set; }
        public bool Alive { get; set; }
    }
    
    public interface IModelMazeData : IPreInit
    {
        event NoArgsHandler GameLoopUpdate;
        event NoArgsHandler MazeItemsProceedStarted;
        event NoArgsHandler MazeItemsProceedStopped;
        event MazeInfoHandler MazeChanged;
        
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; set; }
        Dictionary<V2Int, bool> PathProceeds { get; }
        Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>> ProceedInfos { get; }
        CharacterInfo CharacterInfo { get; }
        bool ProceedingMazeItems { get; set; }
        bool ProceedingControls { get; set; }
        void OnGameLoopUpdate();
        void RaiseMazeChanged();
    }
    
    public class ModelMazeData : IModelMazeData
    {
        #region nonpublic members
        
        private MazeInfo m_Info;
        private bool? m_ProceedingMazeItems;

        #endregion
        
        #region constructor
        
        public event NoArgsHandler PreInitialized;
        
        public void PreInit()
        {
            ProceedInfos = new Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>>();
            var types = Enum.GetValues(typeof(EMazeItemType)).Cast<EMazeItemType>();
            foreach (var type in types)
                ProceedInfos.Add(type, new Dictionary<MazeItem, IMazeItemProceedInfo>());
            PreInitialized?.Invoke();
        }
        
        #endregion

        #region api

        public event NoArgsHandler GameLoopUpdate;
        public event NoArgsHandler MazeItemsProceedStarted;
        public event NoArgsHandler MazeItemsProceedStopped;
        public event MazeInfoHandler MazeChanged;
        public MazeOrientation Orientation { get; set; } = MazeOrientation.North;
        public Dictionary<V2Int, bool> PathProceeds { get; private set; }

        public Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>> ProceedInfos { get; private set; } 
            = new Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>>();

        public CharacterInfo CharacterInfo { get; } = new CharacterInfo();

        public bool ProceedingMazeItems
        {
            get => m_ProceedingMazeItems ?? false;
            set
            {
                if (m_ProceedingMazeItems.HasValue && m_ProceedingMazeItems.Value == value)
                    return;
                if (value)
                    MazeItemsProceedStarted?.Invoke();
                else
                    MazeItemsProceedStopped?.Invoke();
                m_ProceedingMazeItems = value;
            }
        }
        
        public bool ProceedingControls { get; set; }
        
        public void OnGameLoopUpdate() => GameLoopUpdate?.Invoke();
        public void RaiseMazeChanged() => MazeChanged?.Invoke(Info);

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = value;
                PathProceeds = m_Info.Path.ToDictionary(_P => _P, _P => false);
                PathProceeds[m_Info.Path[0]] = true;
                MazeChanged?.Invoke(m_Info);
            }
        }
        
        #endregion
    }
}