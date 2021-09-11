using System;
using System.Linq;
using Entities;
using UnityEngine;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class TurretShotEventArgs : EventArgs
    {
        public MazeItem Item { get; }
        public V2Int From { get; }
        public V2Int To { get; }
        public V2Int Direction { get; }
        public float ProjectileSpeed { get; }
        public bool PreShoot { get; }

        public TurretShotEventArgs(
            MazeItem _Item,
            V2Int _From,
            V2Int _To, 
            V2Int _Direction,
            float _ProjectileSpeed,
            bool _PreShoot)
        {
            Item = _Item;
            From = _From;
            To = _To;
            Direction = _Direction;
            ProjectileSpeed = _ProjectileSpeed;
            PreShoot = _PreShoot;
        }
    }

    public delegate void TurretShotEventHandler(TurretShotEventArgs Args);
    
    public interface ITurretsProceeder : IOnMazeChanged
    {
        event TurretShotEventHandler TurretShoot;
    }
    
    public class TurretsProceeder : ItemsProceederBase, IOnGameLoopUpdate, ITurretsProceeder
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
        
        public void OnGameLoopUpdate()
        {
            if (!Data.ProceedingMazeItems)
                return;
            ProceedTurrets();
        }
        
        #endregion
        
        #region nonpublic methods
        
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
            var to = _Item.Position + _Item.Direction;
            while (ValidPosition(to, Data.Info))
                to += _Item.Direction;
            TurretShoot?.Invoke(new TurretShotEventArgs(
                _Item, 
                _Item.Position, 
                to,
                _Item.Direction,
                Settings.turretProjectileSpeed * 0.1f,
                _PreShoot));
        }
        
        private bool ValidPosition(V2Int _Position, MazeInfo _Info)
        {
            bool isNode = _Info.Path.Any(_PathItem => _PathItem == _Position);
            bool isMazeItem = _Info.MazeItems.Any(_O => 
                _O.Position == _Position
                && (_O.Type == EMazeItemType.Block
                    || _O.Type == EMazeItemType.TrapIncreasing
                    || _O.Type == EMazeItemType.Turret));
            var shredinger = Data.ProceedInfos[EMazeItemType.ShredingerBlock].Values
                .FirstOrDefault(_Inf => _Inf.Item.Position == _Position);

            if (shredinger != null)
                return shredinger.ProceedingStage != 2;
            return isNode && !isMazeItem;
        }
        
        #endregion
    }
}