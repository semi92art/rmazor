using System.Collections.Generic;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;

namespace Games.RazorMaze.Views.Helpers.MazeItemsCreators
{
    public abstract class MazeItemsCreatorBase : IMazeItemsCreator
    {
        #region nonpublic members
        
        protected IContainersGetter            ContainersGetter { get; }
        protected ICoordinateConverter         CoordinateConverter { get; }
        protected IViewMazeItemPath            ItemPath { get; }
        protected IViewMazeItemGravityBlock    GravityBlock { get; }
        protected IViewMazeItemMovingTrap      MovingTrap { get; }
        protected IViewMazeItemShredingerBlock ShredingerBlock { get; }
        protected IViewMazeItemTurret          Turret { get; }
        protected IViewMazeItemSpringboard     Springboard { get; }
        protected IViewMazeItemPortal          Portal { get; }
        protected IViewMazeItemGravityTrap     GravityTrap { get; }
        protected IViewMazeItemTrapReact       TrapReact { get; }
        protected IViewMazeItemTrapIncreasing  TrapIncreasing { get; }
        
        #endregion

        #region protected constructor

        protected MazeItemsCreatorBase(
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter, 
            IViewMazeItemPath _ItemPath                   = null,
            IViewMazeItemGravityBlock _GravityBlock       = null,
            IViewMazeItemMovingTrap _MovingTrap           = null,
            IViewMazeItemShredingerBlock _ShredingerBlock = null,
            IViewMazeItemTurret _Turret                   = null,
            IViewMazeItemSpringboard _Springboard         = null,
            IViewMazeItemPortal _Portal                   = null,
            IViewMazeItemGravityTrap _GravityTrap         = null,
            IViewMazeItemTrapReact _TrapReact             = null,
            IViewMazeItemTrapIncreasing _TrapIncreasing   = null)
        {
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
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
        }

        #endregion
        
        #region api
        
        public virtual List<IViewMazeItem> CreateMazeItems(MazeInfo _Info)
        {
            CoordinateConverter.Init(_Info.Size);
            
            var res = new List<IViewMazeItem>();
            foreach (var item in _Info.Path)
                AddPathItem(res, _Info, item);
            foreach (var item in _Info.MazeItems)
                AddMazeItem(res, _Info, item);
            return res;
        }

        public abstract void InitMazeItems(
            MazeInfo _Info,
            SpawnPool<IViewMazeItemPath> _PathPool,
            Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> _BlockPools);

        public IViewMazeItem CloneDefaultBlock(EMazeItemType _Type)
        {
            IViewMazeItem item;
            switch (_Type)
            {
                case EMazeItemType.Block:           item = null; break;
                case EMazeItemType.GravityBlock:    item = GravityBlock; break;
                case EMazeItemType.ShredingerBlock: item = ShredingerBlock; break;
                case EMazeItemType.Portal:          item = Portal; break;
                case EMazeItemType.TrapReact:       item = TrapReact; break;
                case EMazeItemType.TrapIncreasing:  item = TrapIncreasing; break;
                case EMazeItemType.TrapMoving:      item = MovingTrap; break;
                case EMazeItemType.GravityTrap:     item = GravityTrap; break;
                case EMazeItemType.Turret:          item = Turret; break;
                case EMazeItemType.TurretRotating:  item = null; break;
                case EMazeItemType.Springboard:     item = Springboard; break;
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

        protected abstract void AddPathItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, V2Int _Position);
        protected abstract void AddMazeItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, MazeItem _Item);

        #endregion
    }
}