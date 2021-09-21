using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;
using Utils;

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
        private IModelMazeData ModelData { get; }
        private IMazeItemsCreator MazeItemsCreator { get; }

        public ViewMazePathItemsGroup(ViewSettings _ViewSettings,
            IModelMazeData _ModelData,
            IMazeItemsCreator _MazeItemsCreator)
        {
            ViewSettings = _ViewSettings;
            ModelData = _ModelData;
            MazeItemsCreator = _MazeItemsCreator;
        }
        

        #endregion
        
        #region api
        
        public event NoArgsHandler Initialized;

        public List<IViewMazeItemPath> PathItems => m_PathsPool.Where(_Item => _Item.Activated).ToList();

        public void Init()
        {
            InitPools(ModelData.Info);
            if (!ViewSettings.StartPathItemFilledOnStart)
                UnfillStartPathItem();
            Initialized?.Invoke();
            m_Initialized = true;
        }

        public void OnPathProceed(V2Int _PathItem)
        {
            var item = PathItems.First(_Item => _Item.Props.Position == _PathItem && _Item.Props.IsNode);
            item.Proceeding = true;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !m_Initialized,
                () =>
                {
                    foreach (var item in PathItems)
                        item.OnLevelStageChanged(_Args);
                }));
        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            if (!m_FirstMoveDone && ViewSettings.StartPathItemFilledOnStart)
                UnfillStartPathItem();
        }

        #endregion
        
        #region nonpublic methods

        private void InitPools(MazeInfo _Info)
        {
            if (m_PathsPool == null)
                InitPoolsOnStart();
            DeactivateAllPaths();
            MazeItemsCreator.InitPathItems(_Info, m_PathsPool);
        }
        
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
            PathItems.Single(_Item => _Item.Props.IsStartNode).Proceeding = true;
        }
        
        #endregion
    }
}