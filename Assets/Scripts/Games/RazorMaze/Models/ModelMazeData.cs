
using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Models
{
    public enum MazeOrientation { North, East, South, West }
    public enum EMazeMoveDirection { Up, Right, Down, Left }
    
    public delegate void MazeInfoHandler(MazeInfo Info);
    public delegate void PathProceedHandler(V2Int PathItem);

    public class CharacterInfo
    {
        public V2Int Position { get; set; }
        public EMazeMoveDirection MoveDirection { get; set; }
        public long HealthPoints { get; set; }
    }
    
    public interface IModelMazeData : IPreInit, ICharacterMoveContinued
    {
        event MazeInfoHandler MazeChanged;
        event PathProceedHandler PathProceedEvent;
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; set; }
        Dictionary<V2Int, bool> PathProceeds { get; }
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
        public event PathProceedHandler PathProceedEvent;
        public MazeOrientation Orientation { get; set; } = MazeOrientation.North;
        public Dictionary<V2Int, bool> PathProceeds { get; private set; }

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
                PathProceeds = m_Info.Path.ToDictionary(_P => _P, _P => false);
                PathProceeds[m_Info.Path[0]] = true;
                MazeChanged?.Invoke(m_Info);
            }
        }
        
        #endregion

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            if (!PathProceeds.ContainsKey(_Args.Current))
                return;
            if (PathProceeds[_Args.Current])
                return;
            PathProceeds[_Args.Current] = true;
            PathProceedEvent?.Invoke(_Args.Current);
        }
    }
}