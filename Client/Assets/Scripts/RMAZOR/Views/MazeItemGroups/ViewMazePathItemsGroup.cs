using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazePathItemsGroup :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued
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

        private GlobalGameSettings GlobalGameSettings { get; }
        private ViewSettings       ViewSettings       { get; }
        private ModelSettings      ModelSettings      { get; }
        private IModelGame         Model              { get; }
        private IMazeItemsCreator  MazeItemsCreator   { get; }

        private ViewMazePathItemsGroup(
            GlobalGameSettings _GlobalGameSettings,
            ViewSettings       _ViewSettings,
            ModelSettings      _ModelSettings,
            IModelGame         _Model,
            IMazeItemsCreator  _MazeItemsCreator)
        {
            GlobalGameSettings = _GlobalGameSettings;
            ViewSettings       = _ViewSettings;
            ModelSettings      = _ModelSettings;
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
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                {
                    m_FirstMoveDone = false;
                    DeactivateAllPaths();
                    MazeItemsCreator.InitPathItems(Model.Data.Info, m_PathsPool);
                    PathItems = m_PathsPool.Where(_Item => _Item.ActivatedInSpawnPool).ToList();
                    CollectStartPathItemIfWasNot(false);
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
            CollectStartPathItemIfWasNot(true);
            var pathItems = RmazorUtils.GetFullPath(_Args.From, _Args.To)
                .Select(_Pos => PathItems.First(
                    _Item => _Item.Props.Position == _Pos && _Item.ActivatedInSpawnPool));
            int k = 0;
            foreach (var item in pathItems)
            {
                if (item is IViewMazeItemPathFilled itemFilled)
                    itemFilled.HighlightPathItem(++k / ModelSettings.characterSpeed);
                item.OnCharacterMoveStarted(_Args);
            }
        }
        
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            foreach (var item in PathItems)
                item.OnCharacterMoveContinued(_Args);
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
        
        private void CollectStartPathItemIfWasNot(bool _CheckOnMoveStarted)
        {
            if ((_CheckOnMoveStarted && m_FirstMoveDone) || ViewSettings.collectStartPathItemOnLevelLoaded)
                return;
            PathItems
                .First(_P => _P.Props.IsStartNode)
                .Collect(true);
            if (_CheckOnMoveStarted)
                m_FirstMoveDone = true;
        }

        private void OnMoneyItemCollected()
        {
            MoneyItemsCollectedCount += GlobalGameSettings.moneyItemCoast;
        }

        #endregion
    }
}