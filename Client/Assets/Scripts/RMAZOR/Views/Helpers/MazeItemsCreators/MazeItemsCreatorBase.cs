using System.Collections.Generic;
using Common.Exceptions;
using Common.SpawnPools;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Helpers.MazeItemsCreators
{
    public abstract class MazeItemsCreatorBase : IMazeItemsCreator
    {
        #region nonpublic members
        
        protected IViewMazeItemPath             ItemPath { get; }
        protected IViewMazeItemGravityBlock     GravityBlock { get; }
        protected IViewMazeItemMovingTrap       MovingTrap { get; }
        protected IViewMazeItemShredingerBlock  ShredingerBlock { get; }
        protected IViewMazeItemTurret           Turret { get; }
        protected IViewMazeItemSpringboard      Springboard { get; }
        protected IViewMazeItemPortal           Portal { get; }
        protected IViewMazeItemGravityTrap      GravityTrap { get; }
        protected IViewMazeItemTrapReact        TrapReact { get; }
        protected IViewMazeItemTrapIncreasing   TrapIncreasing { get; }
        protected IViewMazeItemGravityBlockFree GravityBlockFree { get; }

        #endregion

        #region protected constructor

        protected MazeItemsCreatorBase(
            IViewMazeItemPath             _ItemPath         = null,
            IViewMazeItemGravityBlock     _GravityBlock     = null,
            IViewMazeItemMovingTrap       _MovingTrap       = null,
            IViewMazeItemShredingerBlock  _ShredingerBlock  = null,
            IViewMazeItemTurret           _Turret           = null,
            IViewMazeItemSpringboard      _Springboard      = null,
            IViewMazeItemPortal           _Portal           = null,
            IViewMazeItemGravityTrap      _GravityTrap      = null,
            IViewMazeItemTrapReact        _TrapReact        = null,
            IViewMazeItemTrapIncreasing   _TrapIncreasing   = null,
            IViewMazeItemGravityBlockFree _GravityBlockFree = null)
        {
            ItemPath = _ItemPath;
            GravityBlock = _GravityBlock;
            MovingTrap = _MovingTrap;
            ShredingerBlock = _ShredingerBlock;
            Turret = _Turret;
            Springboard = _Springboard;
            Portal = _Portal;
            GravityTrap = _GravityTrap;
            TrapReact = _TrapReact;
            TrapIncreasing = _TrapIncreasing;
            GravityBlockFree = _GravityBlockFree;
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
            IViewMazeItem item;
            switch (_Type)
            {
                case EMazeItemType.Block:            item = null;            break;
                case EMazeItemType.GravityBlock:     item = GravityBlock;    break;
                case EMazeItemType.ShredingerBlock:  item = ShredingerBlock; break;
                case EMazeItemType.Portal:           item = Portal;          break;
                case EMazeItemType.TrapReact:        item = TrapReact;       break;
                case EMazeItemType.TrapIncreasing:   item = TrapIncreasing;  break;
                case EMazeItemType.TrapMoving:       item = MovingTrap;      break;
                case EMazeItemType.GravityTrap:      item = GravityTrap;     break;
                case EMazeItemType.Turret:           item = Turret;          break;
                case EMazeItemType.GravityBlockFree: item = GravityBlockFree;break;
                case EMazeItemType.Springboard:      item = Springboard;     break;
                case EMazeItemType.MovingBlockFree:  item = null;            break;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
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