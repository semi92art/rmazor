using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.SpawnPools;
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

        private CommonGameSettings CommonGameSettings { get; }
        private ViewSettings       ViewSettings       { get; }
        private IModelGame         Model              { get; }
        private IMazeItemsCreator  MazeItemsCreator   { get; }

        public ViewMazePathItemsGroup(
            CommonGameSettings _CommonGameSettings,
            ViewSettings      _ViewSettings,
            IModelGame        _Model,
            IMazeItemsCreator _MazeItemsCreator)
        {
            CommonGameSettings = _CommonGameSettings;
            ViewSettings       = _ViewSettings;
            Model              = _Model;
            MazeItemsCreator   = _MazeItemsCreator;
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
            item.Collect(true);
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
                    if (ViewSettings.collectStartPathItemOnLevelLoaded)
                        CollectStartPathItem();
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
            if (!m_FirstMoveDone && !ViewSettings.collectStartPathItemOnLevelLoaded)
                CollectStartPathItem();
            m_FirstMoveDone = true;
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitPoolsOnStart()
        {
            m_PathsPool = new SpawnPool<IViewMazeItemPath>();
            var pathItems = Enumerable
                .Range(0, ViewSettings.pathItemsCount)
                .Select(_ => MazeItemsCreator.CloneDefaultPath())
                .ToList();
            foreach (var item in pathItems)
            {
                item.MoneyItemCollected -= OnMoneyItemCollected;
                item.MoneyItemCollected += OnMoneyItemCollected;
            }
            
            m_PathsPool.AddRange(pathItems);
        }
        
        private void DeactivateAllPaths()
        {
            m_PathsPool.DeactivateAll();
        }
        
        private void CollectStartPathItem()
        {
            PathItems.First(_P => _P.Props.IsStartNode).Collect(true);
        }

        private void OnMoneyItemCollected()
        {
            MoneyItemsCollectedCount += CommonGameSettings.moneyItemCoast;
        }
        
        #endregion
    }
}