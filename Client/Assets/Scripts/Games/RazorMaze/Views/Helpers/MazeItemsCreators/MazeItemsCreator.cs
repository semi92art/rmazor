using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.MazeItems.Props;
using SpawnPools;
using Zenject;

namespace Games.RazorMaze.Views.Helpers.MazeItemsCreators
{
    public class MazeItemsCreator : MazeItemsCreatorBase
    {
        #region inject
        
        [Inject]
        public MazeItemsCreator(
            IContainersGetter             _ContainersGetter,
            IMazeCoordinateConverter      _CoordinateConverter, 
            IViewMazeItemPath             _ItemPath,
            IViewMazeItemGravityBlock     _GravityBlock,
            IViewMazeItemMovingTrap       _MovingTrap,
            IViewMazeItemShredingerBlock  _ShredingerBlock,
            IViewMazeItemTurret           _Turret,
            IViewMazeItemSpringboard      _Springboard,
            IViewMazeItemPortal           _Portal,
            IViewMazeItemGravityTrap      _GravityTrap,
            IViewMazeItemTrapReact        _TrapReact,
            IViewMazeItemTrapIncreasing   _TrapIncreasing,
            IViewMazeItemGravityBlockFree _GravityBlockFree) : base(
            _ContainersGetter, 
            _CoordinateConverter, 
            _ItemPath,
            _GravityBlock, 
            _MovingTrap,
            _ShredingerBlock,
            _Turret, 
            _Springboard,
            _Portal,
            _GravityTrap,
            _TrapReact,
            _TrapIncreasing,
            _GravityBlockFree) { }
        
        #endregion
        
        #region api

        public override void InitPathItems(MazeInfo _Info, SpawnPool<IViewMazeItemPath> _PathPool)
        {
            var moneyItemIndices = new List<int>();
            int pathCount = _Info.Path.Count;
            if (pathCount > 10)
                moneyItemIndices.Add(5);
            if (pathCount > 20)
                moneyItemIndices.Add(20);
            if (pathCount > 30)
                moneyItemIndices.Add(15);
            if (pathCount > 40)
                moneyItemIndices.Add(40);
            if (pathCount > 50)
                moneyItemIndices.Add(25);

            for (int i = 0; i < pathCount; i++)
            {
                var pathItemPos = _Info.Path[i];
                var props = new ViewMazeItemProps
                {
                    IsNode = true,
                    IsStartNode = pathItemPos == _Info.Path.First(),
                    Position = pathItemPos
                };
                if (moneyItemIndices.Contains(i)
                    && _Info.MazeItems.All(_Item => _Item.Position != pathItemPos)
                    && _Info.MazeItems.All(_Item => !_Item.Path.Contains(pathItemPos)))
                {
                    props.IsMoneyItem = true;
                }
                var pathItemInPool = _PathPool.FirstInactive;
                pathItemInPool.Init(props);
                _PathPool.Activate(pathItemInPool);
            }
        }

        public override void InitAndActivateBlockItems(MazeInfo _Info, Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> _BlockPools)
        {
            foreach (var mazeItem in _Info.MazeItems.Where(_Item => _Item.Type != EMazeItemType.Block))
            {
                var props = new ViewMazeItemProps
                {
                    Type = mazeItem.Type,
                    Position = mazeItem.Position,
                    Path = mazeItem.Path,
                    Directions = new List<V2Int>{mazeItem.Direction},
                    Pair = mazeItem.Pair,
                    IsNode = false,
                    IsStartNode = false
                };
                var blockItemInPool = _BlockPools[mazeItem.Type].FirstInactive;
                blockItemInPool.Init(props);
                _BlockPools[mazeItem.Type].Activate(blockItemInPool);
            }
        }

        #endregion
        
        #region nonpublic methods
        
        protected override void AddPathItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, V2Int _Position)
            => throw new Exception("this method is only for prototypes");

        protected override void AddMazeItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, MazeItem _Item) 
            => throw new Exception("this method is only for prototypes");

        #endregion
    }
}