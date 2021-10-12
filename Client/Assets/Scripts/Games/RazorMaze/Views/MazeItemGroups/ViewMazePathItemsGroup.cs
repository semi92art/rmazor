using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazePathItemsGroup : IViewMazePathItemsGroup
    {
        #region nonpublic members
        
        private SpawnPool<IViewMazeItemPath> m_PathsPool;
        private bool m_FirstMoveDone;
        private bool m_Initialized;
        
        #endregion

        #region inject
        
        private ViewSettings ViewSettings { get; }
        private IModelData ModelData { get; }
        private IMazeItemsCreator MazeItemsCreator { get; }

        public ViewMazePathItemsGroup(ViewSettings _ViewSettings,
            IModelData _ModelData,
            IMazeItemsCreator _MazeItemsCreator)
        {
            ViewSettings = _ViewSettings;
            ModelData = _ModelData;
            MazeItemsCreator = _MazeItemsCreator;
        }
        

        #endregion
        
        #region api
        
        public event NoArgsHandler Initialized;

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
                DeactivateAllPaths();
                MazeItemsCreator.InitPathItems(ModelData.Info, m_PathsPool);
                if (!ViewSettings.StartPathItemFilledOnStart)
                    UnfillStartPathItem();
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
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            foreach (var pathItem in PathItems)
                pathItem.OnCharacterMoveFinished(_Args);
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
        
        #endregion
    }
}