using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTurretsGroup :
        IInit,
        IViewMazeItemGroup
    {
        void OnTurretShoot(TurretShotEventArgs _Args);
    }
    
    public abstract class ViewMazeTurretsGroupBase : ViewMazeItemsGroupBase, IViewMazeTurretsGroup
    {
        #region inject

        protected IModelData Data { get; }
        protected IMazeCoordinateConverter Converter { get; }
        protected IContainersGetter ContainersGetter { get; }

        protected ViewMazeTurretsGroupBase(
            IModelData _Data,
            IViewMazeCommon _Common, 
            IMazeCoordinateConverter _Converter,
            IContainersGetter _ContainersGetter) : base(_Common)
        {
            Data = _Data;
            Converter = _Converter;
            ContainersGetter = _ContainersGetter;
        }

        #endregion
        
        #region api

        public override EMazeItemType[] Types => new[] {EMazeItemType.Turret};
        public event UnityAction Initialized;
        public virtual void Init() => Initialized?.Invoke();
        public abstract void OnTurretShoot(TurretShotEventArgs _Args);

        #endregion

    }
}