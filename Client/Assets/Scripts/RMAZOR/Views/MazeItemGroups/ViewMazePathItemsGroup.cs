using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.SpawnPools;
using Common.Utils;
using Managers;
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
        List<IViewMazeItemPath> PathItems { get; }
        void OnPathProceed(V2Int _PathItem);
    }
    
    public class ViewMazePathItemsGroup : InitBase, IViewMazePathItemsGroup
    {
        #region nonpublic members
        
        private SpawnPool<IViewMazeItemPath> m_PathsPool;
        private bool                         m_FirstMoveDone;
        private int                          m_MoneyItemsCollectedCount;
        
        #endregion

        #region inject
        
        private ViewSettings      ViewSettings     { get; }
        private IModelData        ModelData        { get; }
        private IMazeItemsCreator MazeItemsCreator { get; }
        private IManagersGetter   Managers         { get; }

        public ViewMazePathItemsGroup(
            ViewSettings      _ViewSettings,
            IModelData        _ModelData,
            IMazeItemsCreator _MazeItemsCreator,
            IManagersGetter   _Managers)
        {
            ViewSettings     = _ViewSettings;
            ModelData        = _ModelData;
            MazeItemsCreator = _MazeItemsCreator;
            Managers         = _Managers;
        }
        
        #endregion
        
        #region api
        
        public List<IViewMazeItemPath> PathItems { get; private set; }


        public void Init()
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
                    MazeItemsCreator.InitPathItems(ModelData.Info, m_PathsPool);
                    PathItems = m_PathsPool.Where(_Item => _Item.ActivatedInSpawnPool).ToList();
                    if (!ViewSettings.StartPathItemFilledOnStart)
                        UnfillStartPathItem();
                    break;
                }
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                {
                    if (m_MoneyItemsCollectedCount > 0)
                    {
                        int moneyItemsCount = m_MoneyItemsCollectedCount;
                        var savedGameEntity = Managers.ScoreManager.
                            GetSavedGameProgress(CommonData.SavedGameFileName, true);
                        Cor.Run(Cor.WaitWhile(
                            () => savedGameEntity.Result == EEntityResult.Pending,
                            () =>
                            {
                                if (savedGameEntity.Result == EEntityResult.Fail)
                                {
                                    Dbg.LogError("Failed to load money entity");
                                    return;
                                }
                                var savedData = new MoneyArgs
                                {
                                    FileName = CommonData.SavedGameFileName,
                                    Money = savedGameEntity.Value.CastTo<MoneyArgs>().Money + moneyItemsCount
                                };
                                Managers.ScoreManager.SaveGameProgress(
                                    savedData, false);
                            }));
                    }
                    break;
                }
                case ELevelStage.ReadyToUnloadLevel:
                    m_MoneyItemsCollectedCount = 0;
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
            IViewMazeItemPath activePathItem;
            while ((activePathItem = m_PathsPool.FirstActive) != null)
                m_PathsPool.Deactivate(activePathItem);
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
            m_MoneyItemsCollectedCount++;
        }
        
        #endregion
    }
}