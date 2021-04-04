using System;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;
using UnityEngine;
using UnityGameLoopDI;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class TurretShotEventArgs : EventArgs
    {
        public MazeItem Item { get; }
        public float ProjectileSpeed { get; }

        public TurretShotEventArgs(MazeItem _Item, float _ProjectileSpeed)
        {
            Item = _Item;
            ProjectileSpeed = _ProjectileSpeed;
        }
    }

    public delegate void TurretShotEventHandler(TurretShotEventArgs Args);
    
    public interface ITurretsProceeder : IOnMazeChanged
    {
        event TurretShotEventHandler TurretShoot;
    }
    
    public class TurretsProceeder : Ticker, IUpdateTick, ITurretsProceeder
    {
        #region inject
        
        private RazorMazeModelSettings Settings { get; }
        private IModelMazeData Data { get; }

        public TurretsProceeder(RazorMazeModelSettings _Settings, IModelMazeData _Data)
        {
            Settings = _Settings;
            Data = _Data;
        }
        
        #endregion
        
        #region api
        
        public event TurretShotEventHandler TurretShoot;
        
        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void CollectItems(MazeInfo _Info)
        {
            var infos = _Info.MazeItems
                .Where(_Item =>_Item.Type == EMazeItemType.Turret)
                .Select(_Item => new MazeItemTurretProceedInfo
                {
                    Item = _Item,
                    IsProceeding = false,
                    PauseTimer = 0
                });
            foreach (var info in infos)
            {
                if (Data.ProceedInfos.ContainsKey(info.Item))
                    Data.ProceedInfos[info.Item] = info;
                else
                    Data.ProceedInfos.Add(info.Item, info);
            }
        }
        
        void IUpdateTick.UpdateTick()
        {
            if (!Data.ProceedingMazeItems)
                return;
            ProceedTurrets();
        }

        private void ProceedTurrets()
        {
            foreach (var proceed in Data.ProceedInfos.Values
                .Where(_P => !_P.IsProceeding && _P.Item.Type == EMazeItemType.Turret))
            {
                proceed.PauseTimer += Time.deltaTime;
                if (proceed.PauseTimer < Settings.turretShootInterval)
                    continue;
                proceed.PauseTimer = 0;
                proceed.IsProceeding = true;
                ProceedTurret(proceed.Item);
                proceed.IsProceeding = false;
            }
        }

        private void ProceedTurret(MazeItem _Item)
        {
            TurretShoot?.Invoke(new TurretShotEventArgs(_Item, Settings.turretProjectileSpeed * 0.1f));
        }
        
        #endregion
    }
}