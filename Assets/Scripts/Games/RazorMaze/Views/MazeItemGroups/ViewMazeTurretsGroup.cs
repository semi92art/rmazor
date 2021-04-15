using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTurretsGroup : ViewMazeTurretsGroupBase
    {
        #region inject
        
        public ViewMazeTurretsGroup(
            IModelMazeData _Data,
            IViewMazeCommon _MazeCommon,
            ICoordinateConverter _Converter,
            IContainersGetter _ContainersGetter)
            : base(_Data, _MazeCommon, _Converter, _ContainersGetter) { }
        
        #endregion
        
        #region api

        public override void OnTurretShoot(TurretShotEventArgs _Args)
        {
            if (_Args.PreShoot)
                HandleTurretPreShot(_Args);
            else
                HandleTurretShot(_Args);
        }

        #endregion

        #region nonpublic methods

        private void HandleTurretPreShot(TurretShotEventArgs _Args)
        {
            var item = MazeCommon.MazeItems
                .Where(_Itm => _Itm is IViewMazeItemTurret)
                .FirstOrDefault(_Itm => _Itm.Equal(_Args.Item)) as IViewMazeItemTurret;
            item?.PreShoot(_Args);
        }
        
        private void HandleTurretShot(TurretShotEventArgs _Args)
        {
            var item = MazeCommon.MazeItems
                .Where(_Itm => _Itm is IViewMazeItemTurret)
                .FirstOrDefault(_Itm => _Itm.Equal(_Args.Item)) as IViewMazeItemTurret;
            item?.Shoot(_Args);
        }
        
        #endregion


    }
}