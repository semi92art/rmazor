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
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter, 
            IViewMazeItemPath _ItemPath,
            IViewMazeItemGravityBlock _GravityBlock,
            IViewMazeItemMovingTrap _MovingTrap,
            IViewMazeItemShredingerBlock _ShredingerBlock,
            IViewMazeItemTurret _Turret,
            IViewMazeItemSpringboard _Springboard,
            IViewMazeItemPortal _Portal,
            IViewMazeItemGravityTrap _GravityTrap,
            IViewMazeItemTrapReact _TrapReact,
            IViewMazeItemTrapIncreasing _TrapIncreasing) : base(
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
            _TrapIncreasing) { }
        
        #endregion
        
        #region api

        public override void InitPathItems(MazeInfo _Info, SpawnPool<IViewMazeItemPath> _PathPool)
        {
            CoordinateConverter.Init(_Info.Size);
            foreach (var pathItemPos in _Info.Path)
            {
                var props = new ViewMazeItemProps
                {
                    IsNode = true,
                    IsStartNode = pathItemPos == _Info.Path.First(),
                    Position = pathItemPos
                };
                var pathItemInPool = _PathPool.FirstInactive;
                pathItemInPool.Init(props);
                _PathPool.Activate(pathItemInPool);
            }
        }

        public override void InitBlockItems(MazeInfo _Info, Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> _BlockPools)
        {
            CoordinateConverter.Init(_Info.Size);
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