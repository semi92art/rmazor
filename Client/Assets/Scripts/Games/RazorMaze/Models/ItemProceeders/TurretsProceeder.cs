using System;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using TimeProviders;
using Utils;

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
    
    public interface ITurretsProceeder : IItemsProceeder
    {
        event TurretShotEventHandler TurretShoot;
    }
    
    public class TurretsProceeder : ItemsProceederBase, IOnGameLoopUpdate, ITurretsProceeder
    {
        #region constants

        public const int StageShoot = 1;
        
        #endregion
        
        #region nonpubic members
        
        protected override EMazeItemType[] Types => 
            new[] {EMazeItemType.Turret, EMazeItemType.TurretRotating};
        
        #endregion
        
        #region inject
        
        private IGameTimeProvider GameTimeProvider { get; }
        
        public TurretsProceeder(
            ModelSettings _Settings,
            IModelMazeData _Data,
            IModelCharacter _Character,
            IGameTimeProvider _GameTimeProvider) 
            : base(_Settings, _Data, _Character)
        {
            GameTimeProvider = _GameTimeProvider;
        }
        
        #endregion
        
        #region api
        
        public event TurretShotEventHandler TurretShoot;
        
        public void OnGameLoopUpdate()
        {
            ProceedTurrets();
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void ProceedTurrets()
        {
            var infos = GetProceedInfos(Types);
            foreach (var info in infos.Values.Where(_Info => _Info.IsProceeding && _Info.ReadyToSwitchStage))
            {
                Coroutines.Run(ProceedTurretCoroutine(info));
            }
        }

        private IEnumerator ProceedTurretCoroutine(IMazeItemProceedInfo _Info)
        {
            _Info.ReadyToSwitchStage = false;
            _Info.ProceedingStage = _Info.ProceedingStage == StageIdle ? StageShoot : StageIdle;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            float time = _Info.ProceedingStage == 0 ? Settings.turretPreShootInterval : Settings.turretShootInterval;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTimeProvider.Time,
                () =>
                {
                    ProceedTurret(_Info.Item, _Info.ProceedingStage == StageIdle);
                    _Info.ReadyToSwitchStage = true;
                });
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
                return shredinger.ProceedingStage != ShredingerBlocksProceeder.StageClosed;
            return isNode && !isMazeItem;
        }
        
        private float GetStageDuration(int _Stage)
        {
            switch (_Stage)
            {
                case 0:
                    return Settings.turretPreShootInterval;
                case 1:
                    return Settings.turretShootInterval;
                default: return 0;
            }
        }
        
        #endregion
    }
}