using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.Utils;
using Managers;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTurretsGroup : ViewMazeTurretsGroupBase
    {
        #region nonpublic members

        private          int   m_BulletCounter;
        private          bool  m_IsHapticPause;
        
        #endregion
        
        #region inject
        
        private IManagersGetter Managers { get; }
        
        public ViewMazeTurretsGroup(
            IModelData _Data,
            IViewMazeCommon _Common,
            IMazeCoordinateConverter _Converter,
            IContainersGetter _ContainersGetter,
            IManagersGetter _Managers)
            : base(_Data, _Common, _Converter, _ContainersGetter)
        {
            Managers = _Managers;
        }
        
        #endregion
        
        #region api

        public override void OnTurretShoot(TurretShotEventArgs _Args)
        {
            if (_Args.PreShoot)
                HandleTurretPreShot(_Args);
            else
                HandleTurretShot(_Args);
        }
        
        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            foreach (var item in GetItems().Cast<IViewMazeItemTurret>())
            {
                int sortingOrder = SortingOrders.GetBlockSortingOrder(EMazeItemType.Turret) + m_BulletCounter++;
                item.SetProjectileSortingOrder(sortingOrder);
            }
        }

        #endregion

        #region nonpublic methods

        private void HandleTurretPreShot(TurretShotEventArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemTurret>(_Args.Info);
            item?.PreShoot(_Args);
        }
        
        private void HandleTurretShot(TurretShotEventArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemTurret>(_Args.Info);
            item?.Shoot(_Args);
            // FIXME ебаный костыль. Требуется более разумный вызов хаптиков и звуков заодно.
            if (!m_IsHapticPause)
            {
                Managers.HapticsManager.PlayPreset(EHapticsPresetType.Selection);
                m_IsHapticPause = true;
                Coroutines.Run(Coroutines.Delay(
                    0.5f,
                    () => m_IsHapticPause = false));
            }
        }

        #endregion
    }
}