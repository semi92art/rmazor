using System;
using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.Utils;
using SpawnPools;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeCommon
{
    public class ViewMazeCommon : ViewMazeCommonBase
    {
        #region nonpublic members

        private Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> m_BlockPools;
        private SpawnPool<IViewMazeItemPath> m_PathsPool;
        
        #endregion
        
        #region inject
        
        private ViewSettings ViewSettings { get; }
        
        public ViewMazeCommon(
            ITicker _Ticker,
            IMazeItemsCreator _MazeItemsCreator, 
            IModelMazeData _Model,
            IContainersGetter _ContainersGetter, 
            ICoordinateConverter _CoordinateConverter,
            ViewSettings _ViewSettings) 
            : base(_Ticker, _MazeItemsCreator, _Model, _ContainersGetter, _CoordinateConverter)
        {
            ViewSettings = _ViewSettings;
        }
        
        #endregion
        
        #region api

        public override List<IViewMazeItem> MazeItems
        {
            get
            {
                IEnumerable<IViewMazeItem> res = m_PathsPool.ToList();
                res = m_BlockPools.Values
                    .Aggregate(res, (_Current, _Pool) => 
                        _Current.Concat(_Pool));
                return res.Where(_Item => _Item.Props != null).ToList();
            }
        }

        public override void Init()
        {
            InitPools(Model.Info);
            Camera.main.backgroundColor = DrawingUtils.ColorBack; //TODO заменить на что-то адекватное
            base.Init();
        }

        public override IViewMazeItem GetItem(MazeItem _Item)
        {
            return m_BlockPools[_Item.Type].SingleOrDefault(_Itm => _Itm.Equal(_Item));
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void InitPools(MazeInfo _Info)
        {
            if (m_BlockPools == null)
                InitPoolsOnStart();
            DeactivateAllBlocksAndPaths();
            MazeItemsCreator.InitMazeItems(_Info, m_PathsPool, m_BlockPools);
        }
        
        private void InitPoolsOnStart()
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
            m_PathsPool = new SpawnPool<IViewMazeItemPath>();
            var pathItems = Enumerable
                .Range(0, ViewSettings.PathItemsCount)
                .Select(_ => MazeItemsCreator.CloneDefaultPath())
                .ToList();
            m_PathsPool.AddRange(pathItems);
        }

        private void DeactivateAllBlocksAndPaths()
        {
            foreach (var pool in m_BlockPools.Values)
            {
                IViewMazeItem activeItem;
                while ((activeItem = pool.FirstActive) != null)
                    pool.Deactivate(activeItem);
            }
            
            IViewMazeItemPath activePathItem;
            while ((activePathItem = m_PathsPool.FirstActive) != null)
                m_PathsPool.Deactivate(activePathItem);
        }
        
        #endregion
    }
}