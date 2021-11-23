using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Views;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public delegate void PathProceedHandler(V2Int PathItem);
    
    public interface IPathItemsProceeder :
        ICharacterMoveStarted, 
        ICharacterMoveContinued, 
        ICharacterMoveFinished
    {
        bool                     AllPathsProceeded { get; }
        Dictionary<V2Int, bool>  PathProceeds      { get; }
        event PathProceedHandler PathProceedEvent;
        event PathProceedHandler AllPathsProceededEvent;
    }
    
    public class PathItemsProceeder : IPathItemsProceeder, IOnLevelStageChanged
    {
        #region nonpublic members

        private List<V2Int> m_CurrentFullPath;
        private bool        m_AllPathItemsNotInPathProceeded;

        #endregion
        
        #region inject
        
        private IModelData Data { get; }

        public PathItemsProceeder(IModelData _Data)
        {
            Data = _Data;
        }
        
        #endregion
        
        #region api

        public bool                     AllPathsProceeded { get; private set; }
        public Dictionary<V2Int, bool>  PathProceeds       { get; private set; }
        public event PathProceedHandler PathProceedEvent;
        public event PathProceedHandler AllPathsProceededEvent;
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            m_CurrentFullPath = RazorMazeUtils.GetFullPath(_Args.From, _Args.To);
            m_AllPathItemsNotInPathProceeded = PathProceeds
                .Where(_Kvp => !m_CurrentFullPath.Contains(_Kvp.Key))
                .Select(_Kvp => _Kvp.Value)
                .All(_Value => _Value);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var path = RazorMazeUtils.GetFullPath(_Args.From, _Args.Position);
            for (int i = 0; i < path.Count; i++)
                ProceedPathItem(path[i]);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            ProceedPathItem(_Args.To);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                CollectPathProceeds();
        }
        
        #endregion
        
        #region nonpublic methods

        private void ProceedPathItem(V2Int _PathItem)
        {
            if (!PathProceeds.ContainsKey(_PathItem) || PathProceeds[_PathItem])
                return;
            PathProceeds[_PathItem] = true;
            PathProceedEvent?.Invoke(_PathItem);
            if (!m_AllPathItemsNotInPathProceeded)
                return;
            for (int i = 0; i < m_CurrentFullPath.Count; i++)
            {
                if (!PathProceeds[m_CurrentFullPath[i]])
                    return;
            }
            AllPathsProceeded = true;
            AllPathsProceededEvent?.Invoke(_PathItem);
        }

        private void CollectPathProceeds()
        {
            AllPathsProceeded = false;
            PathProceeds = Data.Info.Path
                .ToDictionary(
                    _P => _P, 
                    _P => Data.Info.MazeItems.Any(_Item => _Item.Position == _P));
            PathProceeds[Data.Info.Path[0]] = true;
        }
        
        #endregion
    }
}