using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazePathItemsGroup :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted
    {
        List<IViewMazeItemPath> PathItems { get; }
        void OnPathProceed(V2Int _PathItem);
    }
    
    public class ViewMazePathItemsGroup : IViewMazePathItemsGroup
    {
        #region nonpublic members
        
        private SpawnPool<IViewMazeItemPath> m_PathsPool;
        private bool                         m_FirstMoveDone;
        private bool                         m_Initialized;
        private int                          m_MoneyItemsCollectedCount;
        
        #endregion

        #region inject
        
        private ViewSettings      ViewSettings     { get; }
        private IModelData        ModelData        { get; }
        private IMazeItemsCreator MazeItemsCreator { get; }
        private IManagersGetter   Managers         { get; }

        public ViewMazePathItemsGroup(ViewSettings _ViewSettings,
            IModelData _ModelData,
            IMazeItemsCreator _MazeItemsCreator,
            IManagersGetter _Managers)
        {
            ViewSettings = _ViewSettings;
            ModelData = _ModelData;
            MazeItemsCreator = _MazeItemsCreator;
            Managers = _Managers;
        }
        
        #endregion
        
        #region api
        
        public event UnityAction Initialized;

        public List<IViewMazeItemPath> PathItems => m_PathsPool.Where(_Item => _Item.ActivatedInSpawnPool).ToList();

        public void Init()
        {
            if (m_PathsPool == null)
                InitPoolsOnStart();
            Initialized?.Invoke();
            m_Initialized = true;
        }

        public void OnPathProceed(V2Int _PathItem)
        {
            var item = PathItems.First(_Item => _Item.Props.Position == _PathItem);
            item.Collected = true;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
            {
                m_MoneyItemsCollectedCount = 0;
                DeactivateAllPaths();
                MazeItemsCreator.InitPathItems(ModelData.Info, m_PathsPool);
                if (!ViewSettings.StartPathItemFilledOnStart)
                    UnfillStartPathItem();
            }
            else if (_Args.Stage == ELevelStage.Finished)
            {
                if (m_MoneyItemsCollectedCount > 0)
                {
                    var moneyEntity = Managers.ScoreManager.GetScore(DataFieldIds.Money);
                    Coroutines.Run(Coroutines.WaitWhile(
                        () => !moneyEntity.Loaded,
                        () =>
                        {
                            var currentMoneyCount = moneyEntity.GetFirstScore();
                            if (currentMoneyCount.HasValue)
                            {
                                Managers.ScoreManager.SetScore(
                                    DataFieldIds.Money, 
                                    currentMoneyCount.Value + m_MoneyItemsCollectedCount);
                            }
                        }));
                }
            }
            foreach (var item in PathItems)
                item.OnLevelStageChanged(_Args);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
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
            IViewMazeItemPath activePathItem;
            while ((activePathItem = m_PathsPool.FirstActive) != null)
                m_PathsPool.Deactivate(activePathItem);
        }
        
        private void UnfillStartPathItem()
        {
            PathItems.Single(_Item => _Item.Props.IsStartNode).Collected = true;
        }

        private void OnMoneyItemCollected()
        {
            m_MoneyItemsCollectedCount++;
        }
        
        #endregion
    }
}