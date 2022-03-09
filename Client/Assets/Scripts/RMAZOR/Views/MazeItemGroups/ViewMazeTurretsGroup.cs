using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Managers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.Utils;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeTurretsGroup :
        IInit,
        IViewMazeItemGroup
    {
        void OnTurretShoot(TurretShotEventArgs _Args);
    }
    
    public class ViewMazeTurretsGroup : ViewMazeItemsGroupBase, IViewMazeTurretsGroup
    {
        #region nonpublic members

        private int  m_BulletCounter;
        private bool m_IsHapticPause;
        
        #endregion
        
        #region inject
        
        private IManagersGetter Managers { get; }
        
        public ViewMazeTurretsGroup(
            IViewMazeCommon _Common,
            IManagersGetter _Managers)
            : base(_Common)
        {
            Managers = _Managers;
        }
        
        #endregion
        
        #region api
        
        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.Turret};

        public void OnTurretShoot(TurretShotEventArgs _Args)
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
                Cor.Run(Cor.Delay(
                    0.5f,
                    () => m_IsHapticPause = false));
            }
        }

        #endregion
    }
}