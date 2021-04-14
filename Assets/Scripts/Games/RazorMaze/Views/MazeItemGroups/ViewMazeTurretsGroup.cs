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
                HandleTurretPreShot(_Args.Item);
            else
                HandleTurretShot(_Args.Item, _Args.ProjectileSpeed);
        }

        #endregion

        #region nonpublic methods

        private void HandleTurretPreShot(MazeItem _Item)
        {
            var item = MazeCommon.MazeItems
                .Where(_Itm => _Itm is IViewMazeItemTurret)
                .FirstOrDefault(_Itm => _Itm.Equal(_Item)) as IViewMazeItemTurret;
            item?.PreShoot();
        }
        
        private void HandleTurretShot(MazeItem _Item, float _Speed)
        {
            var item = MazeCommon.MazeItems
                .Where(_Itm => _Itm is IViewMazeItemTurret)
                .FirstOrDefault(_Itm => _Itm.Equal(_Item)) as IViewMazeItemTurret;
            item?.Shoot(_Speed);
        }
        
        #endregion


    }
}