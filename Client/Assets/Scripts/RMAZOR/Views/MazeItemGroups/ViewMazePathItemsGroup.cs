using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.SpawnPools;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazePathItemsGroup :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted
    {
        int                     MoneyItemsCollectedCount { get; }
        List<IViewMazeItemPath> PathItems                { get; }
        void                    OnPathProceed(V2Int _PathItem);
    }
    
    public class ViewMazePathItemsGroup : InitBase, IViewMazePathItemsGroup
    {
        #region nonpublic members
        
        private SpawnPool<IViewMazeItemPath> m_PathsPool;
        private bool                         m_FirstMoveDone;
        
        #endregion

        #region inject
        
        private ViewSettings      ViewSettings     { get; }
        private IModelGame        Model            { get; }
        private IMazeItemsCreator MazeItemsCreator { get; }
        private IManagersGetter   Managers         { get; }

        public ViewMazePathItemsGroup(
            ViewSettings      _ViewSettings,
            IModelGame        _Model,
            IMazeItemsCreator _MazeItemsCreator,
            IManagersGetter   _Managers)
        {
            ViewSettings     = _ViewSettings;
            Model            = _Model;
            MazeItemsCreator = _MazeItemsCreator;
            Managers         = _Managers;
        }
        
        #endregion
        
        #region api

        public int                     MoneyItemsCollectedCount { get; private set; }
        public List<IViewMazeItemPath> PathItems                { get; private set; }


        public override void Init()
        {
            if (m_PathsPool == null)
                InitPoolsOnStart();
            base.Init();
        }

        public void OnPathProceed(V2Int _PathItem)
        {
            var item = PathItems.First(_Item => _Item.Props.Position == _PathItem);
            item.Collected = true;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                {
                    DeactivateAllPaths();
                    MazeItemsCreator.InitPathItems(Model.Data.Info, m_PathsPool);
                    PathItems = m_PathsPool.Where(_Item => _Item.ActivatedInSpawnPool).ToList();
                    if (!ViewSettings.StartPathItemFilledOnStart)
                        UnfillStartPathItem();
                    break;
                }
                case ELevelStage.ReadyToUnloadLevel:
                    MoneyItemsCollectedCount = 0;
                    break;
            }
            foreach (var item in PathItems)
                item.OnLevelStageChanged(_Args);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            if (!m_FirstMoveDone && ViewSettings.StartPathItemFilledOnStart)
                UnfillStartPathItem();
            m_FirstMoveDone = true;
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitPoolsOnStart()
        {
            m_PathsPool = new SpawnPool<IViewMazeItemPath>();
            var pathItems = Enumerable
                .Range(0, ViewSettings.PathItemsCount)
                .Select(_ => MazeItemsCreator.CloneDefaultPath())
                .ToList();
            foreach (var item in pathItems)
                item.MoneyItemCollected = OnMoneyItemCollected;
            m_PathsPool.AddRange(pathItems);
        }
        
        private void DeactivateAllPaths()
        {
            m_PathsPool.DeactivateAll();
        }
        
        private void UnfillStartPathItem()
        {
            for (int i = 0; i < PathItems.Count; i++)
            {
                var item = PathItems[i];
                if (!item.Props.IsStartNode) 
                    continue;
                item.Collected = true;
                break;
            }
        }

        private void OnMoneyItemCollected()
        {
            MoneyItemsCollectedCount++;
        }
        
        #endregion
    }
}