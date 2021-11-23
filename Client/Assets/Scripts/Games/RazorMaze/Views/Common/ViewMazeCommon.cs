using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.Common
{
    public class ViewMazeCommon : ViewMazeCommonBase
    {
        #region nonpublic members

        private Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> m_BlockPools;
        
        #endregion
        
        #region inject

        private IManagersGetter Managers { get; }
        private ViewSettings ViewSettings { get; }
        
        public ViewMazeCommon(
            IViewGameTicker _GameTicker,
            IMazeItemsCreator _MazeItemsCreator, 
            IModelData _ModelData,
            IContainersGetter _ContainersGetter, 
            IMazeCoordinateConverter _CoordinateConverter,
            IManagersGetter _Managers,
            ViewSettings _ViewSettings) 
            : base(_GameTicker, _MazeItemsCreator, _ModelData, _ContainersGetter, _CoordinateConverter)
        {
            Managers = _Managers;
            ViewSettings = _ViewSettings;
        }
        
        #endregion
        
        #region api

        public override List<IViewMazeItem> MazeItems
        {
            get
            {
                IEnumerable<IViewMazeItem> res = new List<IViewMazeItem>();
                return m_BlockPools.Values
                    .Aggregate(res, (_Current, _Pool) => 
                        _Current.Concat(_Pool))
                    .Where(_Item => _Item.Props != null).ToList();
            }
        }

        public override void Init()
        {
            if (m_BlockPools == null)
                InitBlockPools();
            base.Init();
        }

        public override IViewMazeItem GetItem(IMazeItemProceedInfo _Info)
        {
            var infos = m_BlockPools[_Info.Type];
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].Equal(_Info))
                    return infos[i];
            }
            return null;
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
            {
                DeactivateAllBlocks();
                MazeItemsCreator.InitAndActivateBlockItems(ModelData.Info, m_BlockPools);
            }
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitBlockPools()
        {
            m_BlockPools = new Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>>();
            var itemTypes = Enum
                .GetValues(typeof(EMazeItemType))
                .Cast<EMazeItemType>()
                .Except(new[] {EMazeItemType.Block})
                .ToList();
            foreach (var type in itemTypes)
            {
                var pool = new SpawnPool<IViewMazeItem>();
                m_BlockPools.Add(type, pool);
                var blockItems = Enumerable
                    .Range(0, ViewSettings.BlockItemsCount)
                    .Select(_ => MazeItemsCreator.CloneDefaultBlock(type))
                    .ToList();
                pool.AddRange(blockItems);
            }
        }

        private void DeactivateAllBlocks()
        {
            foreach (var pool in m_BlockPools.Values)
            {
                IViewMazeItem activeItem;
                while ((activeItem = pool.FirstActive) != null)
                    pool.Deactivate(activeItem);
            }
        }
        
        #endregion
    }
}