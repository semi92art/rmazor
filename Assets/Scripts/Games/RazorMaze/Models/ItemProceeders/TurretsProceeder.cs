using System;
using System.Linq;
using UnityEngine;
using UnityGameLoopDI;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class TurretShotEventArgs : EventArgs
    {
        public MazeItem Item { get; }
        public float ProjectileSpeed { get; }
        public bool PreShoot { get; }

        public TurretShotEventArgs(MazeItem _Item, float _ProjectileSpeed, bool _PreShoot)
        {
            Item = _Item;
            ProjectileSpeed = _ProjectileSpeed;
            PreShoot = _PreShoot;
        }
    }

    public delegate void TurretShotEventHandler(TurretShotEventArgs Args);
    
    public interface ITurretsProceeder : IOnMazeChanged
    {
        event TurretShotEventHandler TurretShoot;
    }
    
    public class TurretsProceeder : ItemsProceederBase, IUpdateTick, ITurretsProceeder
    {
        #region nonpubic members
        
        protected override EMazeItemType[] Types => 
            new[] {EMazeItemType.Turret, EMazeItemType.TurretRotating};
        
        #endregion
        
        #region inject
        
        public TurretsProceeder(ModelSettings _Settings, IModelMazeData _Data) 
            : base(_Settings, _Data) { }
        
        #endregion
        
        #region api
        
        public event TurretShotEventHandler TurretShoot;
        
        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }
        
        #endregion
        
        #region nonpublic methods

        void IUpdateTick.UpdateTick()
        {
            if (!Data.ProceedingMazeItems)
                return;
            ProceedTurrets();
        }

        private void ProceedTurrets()
        {
            foreach (var type in Types)
            {
                var infos = GetProceedInfos(type);
                foreach (var info in infos.Values.Where(_Info => !_Info.IsProceeding))
                {
                    info.PauseTimer += Time.deltaTime;
                    if (info.PauseTimer < Settings.turretPreShootInterval)
                        continue;
                    info.PauseTimer = 0;
                    info.IsProceeding = true;
                    ProceedTurret(info.Item, true);
                }
                
                foreach (var info in infos.Values.Where(_Info => _Info.IsProceeding))
                {
                    info.PauseTimer += Time.deltaTime;
                    if (info.PauseTimer < Settings.turretShootInterval)
                        continue;
                    info.PauseTimer = 0;
                    ProceedTurret(info.Item, false);
                    info.IsProceeding = false;
                }
            }
        }

        private void ProceedTurret(MazeItem _Item, bool _PreShoot)
        {
            TurretShoot?.Invoke(new TurretShotEventArgs(
                _Item, Settings.turretProjectileSpeed * 0.1f, _PreShoot));
        }
        
        #endregion
    }
}