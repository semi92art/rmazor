using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.SpawnPools;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.MazeItems.ViewMazeItemPath;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazePathItemsGroup :
        IInit,
        IOnLevelStageChanged,
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
        private   bool                         m_FirstMoveDone;
        private   long                         m_CurrentLevelMoney;
        
        #endregion

        #region inject

        protected GlobalGameSettings GlobalGameSettings { get; }
        protected ViewSettings       ViewSettings       { get; }
        protected ModelSettings      ModelSettings      { get; }
        protected IModelGame         Model              { get; }
        protected IMazeItemsCreator  MazeItemsCreator   { get; }
        protected IMoneyCounter      MoneyCounter       { get; }

        protected ViewMazePathItemsGroup(
            GlobalGameSettings _GlobalGameSettings,
            ViewSettings       _ViewSettings,
            ModelSettings      _ModelSettings,
            IModelGame         _Model,
            IMazeItemsCreator  _MazeItemsCreator,
            IMoneyCounter      _MoneyCounter)
        {
            GlobalGameSettings = _GlobalGameSettings;
            ViewSettings       = _ViewSettings;
            ModelSettings      = _ModelSettings;
            Model              = _Model;
            MazeItemsCreator   = _MazeItemsCreator;
            MoneyCounter       = _MoneyCounter;
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
                case ELevelStage.Loaded:
                {
                    m_FirstMoveDone = false;
                    DeactivateAllPaths();
                    MazeItemsCreator.InitPathItems(Model.Data.Info, PathsPool);
                    PathItems = PathsPool.Where(_Item => _Item.ActivatedInSpawnPool).ToList();
                    CollectStartPathItemIfWasNot(false);

                    break;
                }
                case ELevelStage.Finished:
                    MoneyCounter.CurrentLevelMoney = m_CurrentLevelMoney;
                    MoneyCounter.CurrentLevelGroupMoney += m_CurrentLevelMoney;
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    m_CurrentLevelMoney = 0;
                    MoneyCounter.CurrentLevelMoney = 0;
                    if (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex))
                        MoneyCounter.CurrentLevelGroupMoney = 0;
                    break;
            }
            foreach (var item in PathItems)
                item.OnLevelStageChanged(_Args);
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
            m_CurrentLevelMoney += GlobalGameSettings.moneyItemCoast;
        }
        
        #endregion
    }
}