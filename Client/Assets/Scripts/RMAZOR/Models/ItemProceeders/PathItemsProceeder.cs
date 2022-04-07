using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views;

namespace RMAZOR.Models.ItemProceeders
{
    public delegate void PathProceedHandler(V2Int _PathItem);
    
    public interface IPathItemsProceeder :
        ICharacterMoveStarted, 
        ICharacterMoveContinued, 
        ICharacterMoveFinished,
        IGetAllProceedInfos
    {
        bool                     AllPathsProceeded { get; }
        Dictionary<V2Int, bool>  PathProceeds      { get; }
        event PathProceedHandler PathProceedEvent;
        event PathProceedHandler AllPathsProceededEvent;
        void ProceedPathItem(V2Int _PathItem);
    }
    
    public class PathItemsProceeder : IPathItemsProceeder, IOnLevelStageChanged
    {
        #region nonpublic members

        private V2Int[] m_CurrentFullPath;
        private bool        m_AllPathItemsNotInPathProceeded;

        #endregion
        
        #region inject
        
        private IModelData      Data      { get; }
        private IModelCharacter Character { get; }

        public PathItemsProceeder(IModelData _Data, IModelCharacter _Character)
        {
            Data = _Data;
            Character = _Character;
        }
        
        #endregion
        
        #region api

        public bool                         AllPathsProceeded { get; private set; }
        public Dictionary<V2Int, bool>      PathProceeds      { get; private set; }
        public event PathProceedHandler     PathProceedEvent;
        public event PathProceedHandler     AllPathsProceededEvent;
        public Func<IMazeItemProceedInfo[]> GetAllProceedInfos { get; set; }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            m_CurrentFullPath = RazorMazeUtils.GetFullPath(_Args.From, _Args.To);
            m_AllPathItemsNotInPathProceeded = true;
            foreach ((var key, bool value) in PathProceeds)
            {
                if (m_CurrentFullPath.Contains(key))
                    continue;
                if (value)
                    continue;
                m_AllPathItemsNotInPathProceeded = false;
                break;
            }
        }

        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            var path = RazorMazeUtils.GetFullPath(_Args.From, _Args.Position);
            for (int i = 0; i < path.Length; i++)
                ProceedPathItem(path[i]);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            ProceedPathItem(_Args.To);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                CollectPathProceeds();
        }

        public void ProceedPathItem(V2Int _PathItem)
        {
            if (!Character.Alive)
                return;
            if (AllPathsProceeded)
                return;
            if (!PathProceeds.ContainsKey(_PathItem))
                return;
            if (PathProceeds[_PathItem])
                return;
            PathProceeds[_PathItem] = true;
            PathProceedEvent?.Invoke(_PathItem);
            if (!m_AllPathItemsNotInPathProceeded)
                return;
            for (int i = 0; i < m_CurrentFullPath.Length; i++)
            {
                if (!PathProceeds[m_CurrentFullPath[i]])
                    return;
            }
            AllPathsProceeded = true;
            AllPathsProceededEvent?.Invoke(_PathItem);
        }
        
        #endregion
        
        #region nonpublic methods

        private void CollectPathProceeds()
        {
            AllPathsProceeded = false;
            PathProceeds = Data.Info.PathItems
                .Select(_PI => _PI.Position)
                .ToDictionary(
                    _P => _P, 
                    _P => false);
                    // _P => Data.Info.MazeItems.Any(_Item =>
                    // {
                    //     if (_Item.Position != _P)
                    //         return false;
                    //     var types = new[] {EMazeItemType.Portal, EMazeItemType.ShredingerBlock};
                    //     return types.Contains(_Item.Type);
                    // }));
            Data.Info.PathItems
                .Where(_PI => _PI.Blank)
                .ToList()
                .ForEach(_PI => PathProceeds[_PI.Position] = true);
        }
        
        #endregion

        
    }
}