using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazeTurretsGroupBase : ViewMazeItemsGroupBase, IViewMazeTurretsGroup
    {
        #region inject

        protected IModelData Data { get; }
        protected ICoordinateConverter Converter { get; }
        protected IContainersGetter ContainersGetter { get; }

        protected ViewMazeTurretsGroupBase(
            IModelData _Data,
            IViewMazeCommon _Common, 
            ICoordinateConverter _Converter,
            IContainersGetter _ContainersGetter) : base(_Common)
        {
            Data = _Data;
            Converter = _Converter;
            ContainersGetter = _ContainersGetter;
        }

        #endregion
        
        #region api

        public override EMazeItemType[] Types => new[] {EMazeItemType.Turret};
        public event NoArgsHandler Initialized;
        public virtual void Init() => Initialized?.Invoke();
        public abstract void OnTurretShoot(TurretShotEventArgs _Args);

        #endregion

    }
}