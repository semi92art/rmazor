using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine;
using Zenject;

namespace Games.RazorMaze.Views.Helpers
{
    public interface IMazeItemsCreator
    {
        bool Editor { get; set; }
        List<IViewMazeItem> CreateMazeItems(MazeInfo _Info);
    }
    
    public class MazeItemsCreator : IMazeItemsCreator
    {
        #region inject
        
        private IContainersGetter ContainersGetter { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IViewMazeItemPath ItemPath { get; }
        private IViewMazeItemGravityBlock GravityBlock { get; }
        private IViewMazeItemMovingTrap MovingTrap { get; }
        private IViewMazeItemShredingerBlock ShredingerBlock { get; }
        private IViewMazeItemTurret Turret { get; }
        private IViewMazeItemSpringboard Springboard { get; }

        [Inject]
        public MazeItemsCreator(
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter, 
            IViewMazeItemPath _ItemPath,
            IViewMazeItemGravityBlock _GravityBlock,
            IViewMazeItemMovingTrap _MovingTrap,
            IViewMazeItemShredingerBlock _ShredingerBlock,
            IViewMazeItemTurret _Turret,
            IViewMazeItemSpringboard _Springboard)
        {
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            ItemPath = _ItemPath;
            GravityBlock = _GravityBlock;
            MovingTrap = _MovingTrap;
            ShredingerBlock = _ShredingerBlock;
            Turret = _Turret;
            Springboard = _Springboard;
        }
        
        public MazeItemsCreator(
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter)
        {
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
        }
        
        #endregion
        
        #region api

        public bool Editor { get; set; }

        public List<IViewMazeItem> CreateMazeItems(MazeInfo _Info)
        {
            CoordinateConverter.Init(_Info.Size);
            
            var res = new List<IViewMazeItem>();
            foreach (var item in _Info.Path)
                AddPathItem(res, _Info, item);
            foreach (var item in _Info.MazeItems)
                AddMazeItem(res, _Info, item);
            return res;
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void AddPathItem(
            ICollection<IViewMazeItem> _Items,
            MazeInfo _Info,
            V2Int _Position)
        {
            bool isStartNode = !_Items.Any();
            var props = new ViewMazeItemProps
            {
                IsNode = true,
                IsStartNode = isStartNode,
                Position = _Position
            };
            if (!(ItemPath is ViewMazeItemPathProtFake))
            {
                var item = (IViewMazeItemPath)ItemPath.Clone();
                item.Init(props);
                _Items.Add(item);
                return;
            }
            AddPathItemProt(_Items, _Info.Size, props);
        }
        
        private void AddPathItemProt(
            ICollection<IViewMazeItem> _Items,
            V2Int _MazeSize,
            ViewMazeItemProps _Props) 
        {
            var tr = new GameObject("Path Item").transform;
            tr.SetParent(ContainersGetter.MazeItemsContainer);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();
            item.MazeSize = _MazeSize; 
            item.Init(_Props);
            _Items.Add(item);
        }
        
        private void AddMazeItem(
            ICollection<IViewMazeItem> _Items,
            MazeInfo _Info,
            MazeItem _Item)
        {
            var props = new ViewMazeItemProps
            {
                Type = _Item.Type,
                Position = _Item.Position,
                Path = _Item.Path,
                Directions = new List<V2Int>{_Item.Direction},
                IsNode = false,
                IsStartNode = false
            };
            if (   _Item.Type == EMazeItemType.GravityBlock    && !(GravityBlock is ViewMazeItemGravityBlockProtFake)
                || _Item.Type == EMazeItemType.TrapMoving      && !(MovingTrap is ViewMazeItemMovingTrapProtFake)
                || _Item.Type == EMazeItemType.ShredingerBlock && !(ShredingerBlock is ViewMazeItemShredingerBlockProtFake)
                || _Item.Type == EMazeItemType.Turret          && !(Turret is ViewMazeItemTurretProtFake)
                || _Item.Type == EMazeItemType.Springboard     && !(Springboard is ViewMazeItemSpringboardProtFake))
            {
                AddMazeItemRelease(_Items, props);
                return;
            }
            AddMazeItemProt(_Items, _Info.Size, props);
        }

        private void AddMazeItemRelease(
            ICollection<IViewMazeItem> _Items,
            ViewMazeItemProps _Props)
        {
            IViewMazeItem item = null;
            switch (_Props.Type)
            {
                case EMazeItemType.GravityBlock:
                    item = (IViewMazeItemGravityBlock) GravityBlock.Clone(); break;
                case EMazeItemType.ShredingerBlock:
                    item = (IViewMazeItemShredingerBlock) ShredingerBlock.Clone(); break;
                case EMazeItemType.Portal:
                    break;
                case EMazeItemType.TrapReact:
                    break;
                case EMazeItemType.TrapIncreasing:
                    break;
                case EMazeItemType.TrapMoving:
                    item = (IViewMazeItemMovingTrap) MovingTrap.Clone(); break;
                case EMazeItemType.GravityTrap:
                    break;
                case EMazeItemType.Turret:
                    item = (IViewMazeItemTurret) Turret.Clone(); break;
                case EMazeItemType.TurretRotating:
                    break;
                case EMazeItemType.Springboard:
                    item = (IViewMazeItemSpringboard) Springboard.Clone(); break;
                case EMazeItemType.Block:
                    break;
                default: throw new SwitchCaseNotImplementedException(_Props.Type);
            }
            if (item == null) return;
            item.Init(_Props);
            _Items.Add(item);
        }


        private void AddMazeItemProt(
            ICollection<IViewMazeItem> _Items,
            V2Int _MazeSize,
            ViewMazeItemProps _Props)
        {
            if (_Props.Type == EMazeItemType.Block && !Editor)
                return;
            
            var tr = new GameObject("Maze Item").transform;
            tr.SetParent(ContainersGetter.MazeItemsContainer);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();

            item.MazeSize = _MazeSize;
            item.Init(_Props);
            _Items.Add(item);
        }
        
        #endregion
    }
}