using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common
{
    public class ViewMazeCommon : InitBase, IViewMazeCommon
    {
        #region nonpublic members

        private readonly Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> m_ItemPools = 
            new Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>>();
        
        private readonly List<IViewMazeItem> m_AllItemsCached = new List<IViewMazeItem>();
        private readonly List<IViewMazeItem> m_ActiveItemsCached = new List<IViewMazeItem>();
        
        #endregion
        
        #region inject

        private ViewSettings      ViewSettings     { get; }
        private IMazeItemsCreator MazeItemsCreator { get; }
        private IModelData        ModelData        { get; }
        
        public ViewMazeCommon(
            IMazeItemsCreator _MazeItemsCreator, 
            IModelData        _ModelData,
            ViewSettings      _ViewSettings)
        {
            MazeItemsCreator  = _MazeItemsCreator;
            ModelData         = _ModelData;
            ViewSettings      = _ViewSettings;
        }
        
        #endregion
        
        #region api
        
        public override void Init()
        {
            InitBlockPools();
            base.Init();
        }

        public IViewMazeItem GetItem(IMazeItemProceedInfo _Info)
        {
            var infos = m_ItemPools[_Info.Type];
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].Equal(_Info))
                    return infos[i];
            }
            return null;
        }
        
        public T GetItem<T>(IMazeItemProceedInfo _Info) where T : class, IViewMazeItem
        {
            return (T) GetItem(_Info);
        }

        public List<IViewMazeItem> GetItems(bool _OnlyActive = true, bool _OnlyInitialized = true)
        {
            if (_OnlyInitialized)
                return _OnlyActive ? m_ActiveItemsCached : m_AllItemsCached;
            var result = new List<IViewMazeItem>();
            foreach (var (_, pool) in m_ItemPools)
                result.AddRange(pool);
            return result;
        }

        public List<T> GetItems<T>(bool _OnlyActive = true) where T : class, IViewMazeItem
        {
            return GetItems(_OnlyActive).OfType<T>().ToList();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded) 
                return;
            DeactivateAllBlocks();
            MazeItemsCreator.InitAndActivateBlockItems(ModelData.Info, m_ItemPools);
            m_AllItemsCached.Clear();
            m_ActiveItemsCached.Clear();
            CachePools();
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitBlockPools()
        {
            var itemTypes = Enum
                .GetValues(typeof(EMazeItemType))
                .Cast<EMazeItemType>()
                .Except(new[] {EMazeItemType.Block, EMazeItemType.Bazooka})
                .ToList();
            foreach (var type in itemTypes)
            {
                var pool = new SpawnPool<IViewMazeItem>();
                m_ItemPools.Add(type, pool);
                var blockItems = Enumerable
                    .Range(0, ViewSettings.blockItemsCount)
                    .Select(_ => MazeItemsCreator.CloneDefaultBlock(type))
                    .ToList();
                pool.AddRange(blockItems);
            }
        }

        private void DeactivateAllBlocks()
        {
            foreach (var pool in m_ItemPools.Values)
                pool.DeactivateAll();
        }

        private void CachePools()
        {
            foreach (var (_, value) in m_ItemPools)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    var item = value[i];
                    if (item.Props == null)
                        continue;
                    m_AllItemsCached.Add(item);
                    if (!item.ActivatedInSpawnPool)
                        continue;
                    m_ActiveItemsCached.Add(item);
                }
            }
        }
        
        #endregion
    }
}