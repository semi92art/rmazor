using System.Collections.Generic;
using Common.SpawnPools;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Helpers.MazeItemsCreators
{
    public abstract class MazeItemsCreatorBase : IMazeItemsCreator
    {
        #region nonpublic members

        private IViewMazeItemPath             ItemPath         { get; }
        private IViewMazeItemGravityBlock     GravityBlock     { get; }
        private IViewMazeItemMovingTrap       MovingTrap       { get; }
        private IViewMazeItemShredingerBlock  ShredingerBlock  { get; }
        private IViewMazeItemTurret           Turret           { get; }
        private IViewMazeItemSpringboard      Springboard      { get; }
        private IViewMazeItemPortal           Portal           { get; }
        private IViewMazeItemGravityTrap      GravityTrap      { get; }
        private IViewMazeItemTrapReact        TrapReact        { get; }
        private IViewMazeItemTrapIncreasing   TrapIncreasing   { get; }
        private IViewMazeItemGravityBlockFree GravityBlockFree { get; }
        private IViewMazeItemHammer           Hammer           { get; }
        private IViewMazeItemSpear            Spear            { get; }
        private IViewMazeItemDiode            Diode            { get; }

        #endregion

        #region protected constructor

        protected MazeItemsCreatorBase(
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
        {
            ItemPath         = _ItemPath;
            GravityBlock     = _GravityBlock;
            MovingTrap       = _MovingTrap;
            ShredingerBlock  = _ShredingerBlock;
            Turret           = _Turret;
            Springboard      = _Springboard;
            Portal           = _Portal;
            GravityTrap      = _GravityTrap;
            TrapReact        = _TrapReact;
            TrapIncreasing   = _TrapIncreasing;
            GravityBlockFree = _GravityBlockFree;
            Hammer           = _Hammer;
            Spear            = _Spear;
            Diode            = _Diode;
        }

        #endregion
        
        #region api
        
        public virtual List<IViewMazeItem> CreateMazeItems(MazeInfo _Info)
        {
            var res = new List<IViewMazeItem>();
            foreach (var item in _Info.PathItems)
                AddPathItem(res, _Info, item);
            foreach (var item in _Info.MazeItems)
                AddMazeItem(res, _Info, item);
            return res;
        }

        public abstract void InitPathItems(MazeInfo _Info, SpawnPool<IViewMazeItemPath> _PathPool);
        public abstract void InitAndActivateBlockItems(MazeInfo _Info, Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> _BlockPools);

        public IViewMazeItem CloneDefaultBlock(EMazeItemType _Type)
        {
            IViewMazeItem item = _Type switch
            {
                EMazeItemType.GravityBlock     => GravityBlock,
                EMazeItemType.ShredingerBlock  => ShredingerBlock,
                EMazeItemType.Portal           => Portal,
                EMazeItemType.TrapReact        => TrapReact,
                EMazeItemType.TrapIncreasing   => TrapIncreasing,
                EMazeItemType.TrapMoving       => MovingTrap,
                EMazeItemType.GravityTrap      => GravityTrap,
                EMazeItemType.Turret           => Turret,
                EMazeItemType.GravityBlockFree => GravityBlockFree,
                EMazeItemType.Springboard      => Springboard,
                EMazeItemType.Hammer           => Hammer,
                EMazeItemType.Spear            => Spear,
                EMazeItemType.Diode            => Diode,
                EMazeItemType.Block            => null,
                _                              => null
            };
            return (IViewMazeItem)item?.Clone();
        }

        public IViewMazeItemPath CloneDefaultPath()
        {
            return (IViewMazeItemPath)ItemPath.Clone();
        }
        
        #endregion

        #region nonpublic methods

        protected abstract void AddPathItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, PathItem _Item);
        protected abstract void AddMazeItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, MazeItem _Item);

        #endregion
    }
}