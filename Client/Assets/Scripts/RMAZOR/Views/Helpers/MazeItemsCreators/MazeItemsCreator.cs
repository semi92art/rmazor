using System;
using System.Collections.Generic;
using System.Linq;
using Common.SpawnPools;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.MazeItems.ViewMazeItemPath;
using Zenject;

namespace RMAZOR.Views.Helpers.MazeItemsCreators
{
    public class MazeItemsCreator : MazeItemsCreatorBase
    {
        #region inject
        
        private IMoneyItemsOnPathItemsDistributor MoneyItemsOnPathItemsDistributor { get; }
        
        [Inject]
        private MazeItemsCreator(
            IMoneyItemsOnPathItemsDistributor _MoneyItemsOnPathItemsDistributor,
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
            IViewMazeItemGravityBlockFree _GravityBlockFree,
            IViewMazeItemHammer           _Hammer,
            IViewMazeItemSpear            _Spear,
            IViewMazeItemDiode            _Diode) 
            : base(
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
            _GravityBlockFree,
            _Hammer,
            _Spear,
            _Diode)
        {
            MoneyItemsOnPathItemsDistributor = _MoneyItemsOnPathItemsDistributor;
        }
        
        #endregion
        
        #region api

        public override void InitPathItems(MazeInfo _Info, SpawnPool<IViewMazeItemPath> _PathPool)
        {
            var moneyItemIndices = MoneyItemsOnPathItemsDistributor.GetMoneyItemIndices(_Info);
            for (int i = 0; i < _Info.PathItems.Count; i++)
            {
                var pathItemPos = _Info.PathItems[i].Position;
                var props = new ViewMazeItemProps
                {
                    IsNode = true,
                    IsStartNode = pathItemPos == _Info.PathItems[0].Position,
                    Position = pathItemPos,
                    Blank = _Info.PathItems[i].Blank,
                };
                if (moneyItemIndices.Contains(i)
                    && _Info.MazeItems.All(_Item => _Item.Position != pathItemPos)
                    && _Info.MazeItems.All(_Item => !_Item.Path.Contains(pathItemPos)
                    && !props.Blank))
                {
                    props.IsMoneyItem = true;
                }
                var pathItemInPool = _PathPool.FirstInactive;
                pathItemInPool.UpdateState(props);
                _PathPool.Activate(pathItemInPool);
            }
        }

        public override void InitAndActivateBlockItems(MazeInfo _Info, Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> _BlockPools)
        {
            foreach (var mazeItem in _Info.MazeItems.Where(_Item => 
                !new [] {EMazeItemType.Block, EMazeItemType.Spear}.Contains(_Item.Type)))
            {
                var props = new ViewMazeItemProps
                {
                    Type = mazeItem.Type,
                    Position = mazeItem.Position,
                    Path = mazeItem.Path,
                    Directions = mazeItem.Directions,
                    Pair = mazeItem.Pair,
                    Blank = mazeItem.Blank,
                    Args = mazeItem.Args,
                    IsNode = false,
                    IsStartNode = false
                };
                var blockItemInPool = _BlockPools[mazeItem.Type].FirstInactive;
                blockItemInPool.UpdateState(props);
                _BlockPools[mazeItem.Type].Activate(blockItemInPool);
            }
        }

        #endregion
        
        #region nonpublic methods
        
        protected override void AddPathItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, PathItem _Item)
        {
            throw new NotSupportedException("this method is only for prototypes");
        }

        protected override void AddMazeItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, MazeItem _Item) 
            => throw new NotSupportedException("this method is only for prototypes");

        #endregion
    }
}