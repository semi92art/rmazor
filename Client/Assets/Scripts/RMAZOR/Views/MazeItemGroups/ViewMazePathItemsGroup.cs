using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.ViewMazeItemPath;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazePathItemsGroup :
        IViewMazeItemGroup,
        IInit,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IOnPathCompleted
    {
        List<IViewMazeItemPath> PathItems                { get; }
        void                    OnPathProceed(V2Int _PathItem);
    }
    
    public class ViewMazePathItemsGroup : InitBase, IViewMazePathItemsGroup
    {
        #region nonpublic members
        
        protected SpawnPool<IViewMazeItemPath> PathsPool;

        private bool m_FirstMoveDone;
        
        #endregion

        #region inject

        private   GlobalGameSettings GlobalGameSettings { get; }
        protected ViewSettings       ViewSettings       { get; }
        protected ModelSettings      ModelSettings      { get; }
        protected IModelGame         Model              { get; }
        private   IMazeItemsCreator  MazeItemsCreator   { get; }
        private   IRewardCounter     RewardCounter      { get; }

        protected ViewMazePathItemsGroup(
            GlobalGameSettings _GlobalGameSettings,
            ViewSettings       _ViewSettings,
            ModelSettings      _ModelSettings,
            IModelGame         _Model,
            IMazeItemsCreator  _MazeItemsCreator,
            IRewardCounter      _RewardCounter)
        {
            GlobalGameSettings = _GlobalGameSettings;
            ViewSettings       = _ViewSettings;
            ModelSettings      = _ModelSettings;
            Model              = _Model;
            MazeItemsCreator   = _MazeItemsCreator;
            RewardCounter      = _RewardCounter;
        }
        
        #endregion
        
        #region api

        public List<IViewMazeItemPath> PathItems { get; private set; }
        
        public override void Init()
        {
            if (PathsPool == null)
                InitPoolsOnStart();
            base.Init();
        }

        public void OnPathProceed(V2Int _PathItem)
        {
            var item = PathItems.First(_Item => _Item.Props.Position == _PathItem);
            item.Collect(true, false);
        }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.None:
                    return;
                case ELevelStage.Loaded:
                {
                    m_FirstMoveDone = false;
                    DeactivateAllPaths();
                    MazeItemsCreator.InitPathItems(Model.Data.Info, PathsPool);
                    PathItems = PathsPool.Where(_Item => _Item.ActivatedInSpawnPool).ToList();
                    CollectStartPathItemIfWasNot(false);
                    break;
                }
            }
            if (PathItems == null)
                return;
            foreach (var item in PathItems)
                item?.OnLevelStageChanged(_Args);
        }
        
        public virtual void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            CollectStartPathItemIfWasNot(true);
            var pathItems = RmazorUtils.GetFullPath(_Args.From, _Args.To)
                .Select(_Pos => PathItems.First(
                    _Item => _Item.Props.Position == _Pos && _Item.ActivatedInSpawnPool));
            foreach (var item in pathItems)
                item.OnCharacterMoveStarted(_Args);
        }
        
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            foreach (var item in PathItems)
                item.OnCharacterMoveContinued(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            foreach (var item in PathItems)
                item.OnCharacterMoveFinished(_Args);
        }
        
        public virtual void OnPathCompleted(V2Int _LastPath)
        {
        }
        
        public IEnumerable<EMazeItemType> Types => new EMazeItemType[0];
        public IEnumerable<IViewMazeItem> GetActiveItems()
        {
            return PathItems;
        }

        #endregion
        
        #region nonpublic methods
        
        protected void InitPoolsOnStart()
        {
            PathsPool = new SpawnPool<IViewMazeItemPath>();
            var pathItems = Enumerable
                .Range(0, ViewSettings.pathItemsCount)
                .Select(_ => MazeItemsCreator.CloneDefaultPath())
                .ToList();
            foreach (var item in pathItems)
            {
                item.MoneyItemCollected -= OnMoneyItemCollected;
                item.MoneyItemCollected += OnMoneyItemCollected;
            }
            PathsPool.AddRange(pathItems);
        }
        
        private void DeactivateAllPaths()
        {
            PathsPool.DeactivateAll();
        }
        
        private void CollectStartPathItemIfWasNot(bool _CheckOnMoveStarted)
        {
            if ((_CheckOnMoveStarted && m_FirstMoveDone) || ViewSettings.collectStartPathItemOnLevelLoaded)
                return;
            PathItems
                .First(_P => _P.Props.IsStartNode)
                .Collect(true, false);
            if (_CheckOnMoveStarted)
                m_FirstMoveDone = true;
        }

        private void OnMoneyItemCollected()
        {
            RewardCounter.CurrentLevelMoney += GlobalGameSettings.moneyItemCoast;
        }
        
        #endregion
    }
}