﻿using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTurretsGroup : ViewMazeTurretsGroupBase
    {
        #region nonpublic members

        private int m_BulletCounter;
        
        #endregion
        
        #region inject
        
        public ViewMazeTurretsGroup(
            IModelMazeData _Data,
            IViewMazeCommon _Common,
            ICoordinateConverter _Converter,
            IContainersGetter _ContainersGetter)
            : base(_Data, _Common, _Converter, _ContainersGetter) { }
        
        #endregion
        
        #region api

        public override void PostInit()
        {
            foreach (var item in GetItems().Cast<IViewMazeItemTurret>())
            {
                int sortingOrder = DrawingUtils.GetBlockSortingOrder(EMazeItemType.Turret) + m_BulletCounter++;
                item.SetBulletSortingOrder(sortingOrder);
            }
            base.PostInit();
        }

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
            var item = Common.GetItem<IViewMazeItemTurret>(_Args.Item);
            item?.PreShoot(_Args);
        }
        
        private void HandleTurretShot(TurretShotEventArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemTurret>(_Args.Item);
            item?.Shoot(_Args);
        }

        private int GetBulletSortingOrder()
        {
            return DrawingUtils.GetBlockSortingOrder(EMazeItemType.Turret) + m_BulletCounter++;
        }
        
        #endregion
    }
}