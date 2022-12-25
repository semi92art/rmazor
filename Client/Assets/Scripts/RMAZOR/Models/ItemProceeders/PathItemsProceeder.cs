using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using mazing.common.Runtime.Entities;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views;

namespace RMAZOR.Models.ItemProceeders
{
    public delegate void PathProceedHandler(V2Int _PathItem);
    
    public interface IPathItemsProceeder :
        ICharacterMoveStarted, 
        ICharacterMoveContinued, 
        ICharacterMoveFinished
    {
        event PathProceedHandler PathProceeded;
        event PathProceedHandler PathCompleted;
        bool                     AllPathsProceeded { get; }
        Dictionary<V2Int, bool>  PathProceeds      { get; }
    }
    
    public class PathItemsProceeder : IPathItemsProceeder, IOnLevelStageChanged
    {
        #region nonpublic members

        private V2Int[] m_CurrentFullPath;
        private bool    m_AllPathItemsNotInPathProceeded;

        #endregion
        
        #region inject
        
        private IModelData      Data      { get; }
        private IModelCharacter Character { get; }

        private PathItemsProceeder(IModelData _Data, IModelCharacter _Character)
        {
            Data      = _Data;
            Character = _Character;
        }
        
        #endregion
        
        #region api

        public bool                         AllPathsProceeded { get; private set; }
        public Dictionary<V2Int, bool>      PathProceeds      { get; private set; }
        public event PathProceedHandler     PathProceeded;
        public event PathProceedHandler     PathCompleted;
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            m_CurrentFullPath = RmazorUtils.GetFullPath(_Args.From, _Args.To);
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
            var path = RmazorUtils.GetFullPath(_Args.From, _Args.Position);
            for (int i = 0; i < path.Length; i++)
                ProceedPathItem(path[i]);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            ProceedPathItem(_Args.To);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.Loaded)
                CollectPathProceeds();
        }

        #endregion
        
        #region nonpublic methods
        
        private void ProceedPathItem(V2Int _PathItem)
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
            PathProceeded?.Invoke(_PathItem);
            if (!m_AllPathItemsNotInPathProceeded)
                return;
            for (int i = 0; i < m_CurrentFullPath.Length; i++)
            {
                if (!PathProceeds[m_CurrentFullPath[i]])
                    return;
            }
            AllPathsProceeded = true;
            PathCompleted?.Invoke(_PathItem);
        }

        private void CollectPathProceeds()
        {
            AllPathsProceeded = false;
            PathProceeds = Data.Info.PathItems
                .Select(_PI => _PI.Position)
                .ToDictionary(
                    _P => _P, 
                    _P => false);
            Data.Info.PathItems
                .Where(_PI => _PI.Blank)
                .ToList()
                .ForEach(_PI => PathProceeds[_PI.Position] = true);
        }
        
        #endregion

        
    }
}