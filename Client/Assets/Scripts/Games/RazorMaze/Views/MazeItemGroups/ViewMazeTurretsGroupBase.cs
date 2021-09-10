using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeCommon;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazeTurretsGroupBase : IViewMazeTurretsGroup
    {
        #region inject

        protected IModelMazeData Data { get; }
        protected IViewMazeCommon MazeCommon { get; }
        protected ICoordinateConverter Converter { get; }
        protected IContainersGetter ContainersGetter { get; }

        protected ViewMazeTurretsGroupBase(
            IModelMazeData _Data,
            IViewMazeCommon _MazeCommon, 
            ICoordinateConverter _Converter,
            IContainersGetter _ContainersGetter)
        {
            Data = _Data;
            MazeCommon = _MazeCommon;
            Converter = _Converter;
            ContainersGetter = _ContainersGetter;
        }

        #endregion
        
        #region api

        public event NoArgsHandler Initialized;

        public virtual void Init() => Initialized?.Invoke();

        public abstract void OnTurretShoot(TurretShotEventArgs _Args);

        #endregion
    }
}