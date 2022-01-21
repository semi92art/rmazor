using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class TurretShotEventArgs : EventArgs
    {
        public IMazeItemProceedInfo Info      { get; }
        public V2Int                From      { get; }
        public V2Int                To        { get; }
        public V2Int                Direction { get; }
        public bool                 PreShoot  { get; }

        public TurretShotEventArgs(
            IMazeItemProceedInfo _Info,
            V2Int _From,
            V2Int _To, 
            V2Int _Direction,
            bool _PreShoot)
        {
            Info = _Info;
            From = _From;
            To = _To;
            Direction = _Direction;
            PreShoot = _PreShoot;
        }
    }

    public delegate void TurretShotEventHandler(TurretShotEventArgs _Args);
    
    public interface ITurretsProceeder : IItemsProceeder
    {
        event TurretShotEventHandler TurretShoot;
    }
    
    public class TurretsProceeder : ItemsProceederBase, IUpdateTick, ITurretsProceeder, IGetAllProceedInfos
    {
        #region constants

        public const int StageShoot = 1;
        
        #endregion

        #region inject
        
        private IPathItemsProceeder PathItemsProceeder { get; }
        
        public TurretsProceeder(
            ModelSettings _Settings,
            IModelData _Data,
            IModelCharacter _Character,
            IModelGameTicker _GameTicker,
            IPathItemsProceeder _PathItemsProceeder) 
            : base(_Settings, _Data, _Character, _GameTicker)
        {
            PathItemsProceeder = _PathItemsProceeder;
        }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[]     Types => new[] {EMazeItemType.Turret};
        public event TurretShotEventHandler TurretShoot;
        public Func<IMazeItemProceedInfo[]> GetAllProceedInfos { private get; set; }
        
        public void UpdateTick()
        {
            ProceedTurrets();
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void ProceedTurrets()
        {
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                var info = ProceedInfos[i];
                if (!info.IsProceeding)
                    continue;
                if (!info.ReadyToSwitchStage)
                    continue;
                info.ReadyToSwitchStage = false;
                ProceedCoroutine(info, ProceedTurretCoroutine(info));
            }
        }

        private IEnumerator ProceedTurretCoroutine(IMazeItemProceedInfo _Info)
        {
            float duration = GetStageDuration(_Info.ProceedingStage); 
            float time = GameTicker.Time;
            yield return Cor.WaitWhile(
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
            var proceedInfos = GetAllProceedInfos();
            var to = _Info.CurrentPosition + _Info.Direction;
            while (ValidPosition(to, proceedInfos))
                to += _Info.Direction;
            to -= _Info.Direction;
            TurretShoot?.Invoke(new TurretShotEventArgs(
                _Info, 
                _Info.CurrentPosition, 
                to,
                _Info.Direction,
                _PreShoot));
        }
        
        private bool ValidPosition(V2Int _Position, IReadOnlyCollection<IMazeItemProceedInfo> _ProceedInfos)
        {
            bool isOnNode = PathItemsProceeder.PathProceeds.Keys.Any(_Pos => _Pos == _Position);
            bool isMazeItem = _ProceedInfos.Any(_O => 
                _O.CurrentPosition == _Position
                && (_O.Type == EMazeItemType.Block
                    || _O.Type == EMazeItemType.TrapIncreasing
                    || _O.Type == EMazeItemType.Turret));
            var shredinger = _ProceedInfos
                .FirstOrDefault(_Info =>
                    _Info.Type == EMazeItemType.ShredingerBlock
                    && _Info.CurrentPosition == _Position);

            if (shredinger != null)
                return shredinger.ProceedingStage != ShredingerBlocksProceeder.StageClosed;
            return isOnNode && !isMazeItem;
        }
        
        private float GetStageDuration(int _Stage)
        {
            switch (_Stage)
            {
                case 0:
                    return Settings.TurretPreShootInterval;
                case 1:
                    return Settings.TurretShootInterval;
                default: return 0;
            }
        }
        
        #endregion
    }
}