using System;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class TurretShotEventArgs : EventArgs
    {
        public IMazeItemProceedInfo Info { get; }
        public V2Int From { get; }
        public V2Int To { get; }
        public V2Int Direction { get; }
        public float ProjectileSpeed { get; }
        public bool PreShoot { get; }

        public TurretShotEventArgs(
            IMazeItemProceedInfo _Info,
            V2Int _From,
            V2Int _To, 
            V2Int _Direction,
            float _ProjectileSpeed,
            bool _PreShoot)
        {
            Info = _Info;
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

        #region constants and nonpublic members

        public const int StageShoot = 1;
        private IShredingerBlocksProceeder ShredingersProceeder { get; }
        
        #endregion

        #region inject
        
        public TurretsProceeder(
            ModelSettings _Settings,
            IModelData _Data,
            IModelCharacter _Character,
            IGameTicker _GameTicker,
            IShredingerBlocksProceeder _ShredingersProceeder) 
            : base(_Settings, _Data, _Character, _GameTicker)
        {
            ShredingersProceeder = _ShredingersProceeder;
        }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.Turret};
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
            foreach (var info in infos.Where(_Info => _Info.IsProceeding && _Info.ReadyToSwitchStage))
            {
                info.ReadyToSwitchStage = false;
                ProceedCoroutine(ProceedTurretCoroutine(info));
            }
        }

        private IEnumerator ProceedTurretCoroutine(IMazeItemProceedInfo _Info)
        {
            float duration = GetStageDuration(_Info.ProceedingStage); 
            float time = GameTicker.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTicker.Time,
                () =>
                {
                    ProceedTurret(_Info, _Info.ProceedingStage == StageIdle);
                    _Info.ReadyToSwitchStage = true;
                    _Info.ProceedingStage = _Info.ProceedingStage == StageIdle ? StageShoot : StageIdle;
                });
        }

        private void ProceedTurret(IMazeItemProceedInfo _Info, bool _PreShoot)
        {
            var to = _Info.CurrentPosition + _Info.Direction;
            while (ValidPosition(to, Data.Info))
                to += _Info.Direction;
            TurretShoot?.Invoke(new TurretShotEventArgs(
                _Info, 
                _Info.CurrentPosition, 
                to,
                _Info.Direction,
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
            var shredinger = ShredingersProceeder.ProceedInfos[EMazeItemType.ShredingerBlock]
                .FirstOrDefault(_Inf => _Inf.CurrentPosition == _Position);

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