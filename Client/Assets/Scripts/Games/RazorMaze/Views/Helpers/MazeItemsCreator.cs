using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;
using Zenject;

namespace Games.RazorMaze.Views.Helpers
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

        public override void InitMazeItems(
            MazeInfo _Info, 
            SpawnPool<IViewMazeItemPath> _PathPool, 
            Dictionary<EMazeItemType, 
                SpawnPool<IViewMazeItem>> _BlockPools)
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

        protected override void AddPathItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, V2Int _Position)
        {
            throw new Exception("this method is only for prototypes");
        }

        protected override void AddMazeItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, MazeItem _Item)
        {
            throw new Exception("this method is only for prototypes");
            // var props = new ViewMazeItemProps
            // {
            //     Type = _Item.Type,
            //     Position = _Item.Position,
            //     Path = _Item.Path,
            //     Directions = new List<V2Int>{_Item.Direction},
            //     Pair = _Item.Pair,
            //     IsNode = false,
            //     IsStartNode = false
            // };
            // AddMazeItemRelease(_Items, props);
        }

        private void AddPathItemRelease(
            ICollection<IViewMazeItem> _Items,
            ViewMazeItemProps _Props)
        {
            if (ItemPath == null) return;
            var item = (IViewMazeItemPath)ItemPath.Clone();
            item.Init(_Props);
            _Items.Add(item);
        }
        
        private void AddMazeItemRelease(
            ICollection<IViewMazeItem> _Items,
            ViewMazeItemProps _Props)
        {
            IViewMazeItem item = null;
            switch (_Props.Type)
            {
                case EMazeItemType.GravityBlock:    item = (IViewMazeItemGravityBlock)    GravityBlock   .Clone(); break;
                case EMazeItemType.ShredingerBlock: item = (IViewMazeItemShredingerBlock) ShredingerBlock.Clone(); break;
                case EMazeItemType.Portal:          item = (IViewMazeItemPortal)          Portal         .Clone(); break;
                case EMazeItemType.TrapReact:       item = (IViewMazeItemTrapReact)       TrapReact      .Clone(); break;
                case EMazeItemType.TrapIncreasing:  item = (IViewMazeItemTrapIncreasing)  TrapIncreasing .Clone(); break;
                case EMazeItemType.TrapMoving:      item = (IViewMazeItemMovingTrap)      MovingTrap     .Clone(); break;
                case EMazeItemType.GravityTrap:     item = (IViewMazeItemGravityTrap)     GravityTrap    .Clone(); break;
                case EMazeItemType.Turret:          item = (IViewMazeItemTurret)          Turret         .Clone(); break;
                case EMazeItemType.TurretRotating: break;
                case EMazeItemType.Springboard:     item = (IViewMazeItemSpringboard)     Springboard    .Clone(); break;
                case EMazeItemType.Block: break;
                default: throw new SwitchCaseNotImplementedException(_Props.Type);
            }
            if (item == null) return;
            item.Init(_Props);
            _Items.Add(item);
        }
    }
}