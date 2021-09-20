using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Models
{
    public enum MazeOrientation { North, East, South, West }
    public enum EMazeMoveDirection { Up, Right, Down, Left }
    
    
    public interface IModelMazeData : IPreInit
    {
        event NoArgsHandler GameLoopUpdate;
        
        int LevelIndex { get; set; }
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; set; }
        Dictionary<V2Int, bool> PathProceeds { get; }
        Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>> ProceedInfos { get; }
        bool ProceedingControls { get; set; }
        void OnGameLoopUpdate();
    }
    
    public class ModelMazeData : IModelMazeData
    {
        #region nonpublic members
        
        private MazeInfo m_Info;
        private bool? m_ProceedingMazeItems;

        #endregion
        
        #region constructor
        
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

        public event NoArgsHandler PreInitialized;
        public event NoArgsHandler GameLoopUpdate;
        public int LevelIndex { get; set; }
        public MazeOrientation Orientation { get; set; } = MazeOrientation.North;
        public Dictionary<V2Int, bool> PathProceeds { get; private set; }

        public Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>> ProceedInfos { get; private set; } 
            = new Dictionary<EMazeItemType, Dictionary<MazeItem, IMazeItemProceedInfo>>();

        public bool ProceedingControls { get; set; }
        
        public void OnGameLoopUpdate() => GameLoopUpdate?.Invoke();

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = CorrectInfo(value);
                PathProceeds = m_Info.Path.ToDictionary(_P => _P, _P => false);
                PathProceeds[m_Info.Path[0]] = true;
            }
        }
        
        #endregion

        #region nonpublic methods

        private MazeInfo CorrectInfo(MazeInfo _Info)
        {
            var info = _Info;
            var additionalPathPositions = info.MazeItems
                .Where(_Item =>
                    _Item.Type == EMazeItemType.Portal
                    || _Item.Type == EMazeItemType.Springboard
                    || _Item.Type == EMazeItemType.GravityBlock
                    || _Item.Type == EMazeItemType.GravityTrap
                    || _Item.Type == EMazeItemType.ShredingerBlock
                    || _Item.Type == EMazeItemType.TrapMoving)
                .Select(_Item => _Item.Position)
                .ToList();
            foreach (var pos in additionalPathPositions.Where(pos => !info.Path.Contains(pos)))
            {
                info.Path.Add(pos);
            }

            return info;
        }

        #endregion
    }
}