using System;
using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Models
{
    public enum MazeOrientation { North, East, South, West }
    public enum EMazeMoveDirection { Up, Right, Down, Left }

    public delegate void MazeInfoHandler(MazeInfo Info);
    
    public interface IModelData
    {
        event MazeInfoHandler MazeInfoSet;
        event NoArgsHandler GameLoopUpdate;
        
        int LevelIndex { get; set; }
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; set; }
        bool ProceedingControls { get; set; }
        void OnGameLoopUpdate();
    }
    
    public class ModelData : IModelData
    {
        #region nonpublic members
        
        private MazeInfo m_Info;
        private bool? m_ProceedingMazeItems;

        #endregion

        #region api
        
        public event MazeInfoHandler MazeInfoSet;
        public event NoArgsHandler GameLoopUpdate;
        public int LevelIndex { get; set; }
        public MazeOrientation Orientation { get; set; } = MazeOrientation.North;


        public bool ProceedingControls { get; set; }
        
        public void OnGameLoopUpdate() => GameLoopUpdate?.Invoke();

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = CorrectInfo(value);
                MazeInfoSet?.Invoke(m_Info);
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