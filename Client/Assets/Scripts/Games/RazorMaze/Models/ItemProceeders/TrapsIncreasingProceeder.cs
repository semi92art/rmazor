using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using TimeProviders;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class MazeItemTrapIncreasingEventArgs : EventArgs
    {
        public MazeItem Item { get; }
        public int Stage { get; }
        public float Duration { get; }

        public MazeItemTrapIncreasingEventArgs(MazeItem _Item, int _Stage, float _Duration)
        {
            Item = _Item;
            Stage = _Stage;
            Duration = _Duration;
        }
    }
    
    public delegate void MazeItemTrapIncreasingEventHandler(MazeItemTrapIncreasingEventArgs Args);
    
    public interface ITrapsIncreasingProceeder : IItemsProceeder, ICharacterMoveContinued
    {
        event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;
    }
    
    public class TrapsIncreasingProceeder : ItemsProceederBase, IOnGameLoopUpdate, ITrapsIncreasingProceeder
    {
        #region constants
        
        public const int StageIncreased = 1;
        
        #endregion
        
        #region nonpublic members
        
        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapIncreasing};
        private readonly Queue<IEnumerator> m_Coroutines = new Queue<IEnumerator>();

        #endregion
        
        #region inject
        
        private IGameTimeProvider GameTimeProvider { get; }
        
        public TrapsIncreasingProceeder(
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

        public event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;
        
        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args) { }
        
        public void OnGameLoopUpdate()
        {
            ProceedTraps();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.ReadyToContinue)
            {
                foreach (var coroutine in m_Coroutines)
                    Coroutines.Stop(coroutine);
                m_Coroutines.Clear();
            }
        }
        
        #endregion
        
        #region nonpublic methods

        private void ProceedTraps()
        {
            var infos = GetProceedInfos(Types).Values;
            foreach (var info in infos
                .Where(_Info => _Info.IsProceeding && _Info.ReadyToSwitchStage))
            {
                if (info.ReadyToSwitchStage)
                {
                    info.ReadyToSwitchStage = false;
                    var coroutine = ProceedTrap(info);
                    m_Coroutines.Enqueue(coroutine);
                    Coroutines.Run(coroutine);
                }
                if (info.ProceedingStage == StageIncreased)
                    CheckForCharacterDeath(info, info.Item.Position);
            }
        }
        
        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            Dbg.Log("ProceedTrap coroutine");
            _Info.ProceedingStage = _Info.ProceedingStage == StageIdle ? StageIncreased : StageIdle;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            float time = GameTimeProvider.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTimeProvider.Time,
                () =>
                {
                    TrapIncreasingStageChanged?.Invoke(
                    new MazeItemTrapIncreasingEventArgs(_Info.Item, _Info.ProceedingStage, duration));
                    _Info.ReadyToSwitchStage = true;
                });
        }
        
        private float GetStageDuration(int _Stage)
        {
            switch (_Stage)
            {
                case StageIdle:
                    return Settings.trapIncreasingIdleTime;
                case StageIncreased:
                    return Settings.trapIncreasingIncreasedTime;
                default: return 0;
            }
        }

        private void CheckForCharacterDeath(IMazeItemProceedInfo _Info, V2Int _Position)
        {
            if (!Character.Alive)
                return;
            if (_Info.ProceedingStage != StageIncreased)
                return;
            var positions = new[]
            {
                _Position + V2Int.down,
                _Position + V2Int.up,
                _Position + V2Int.left,
                _Position + V2Int.right,
                _Position + V2Int.down + V2Int.left,
                _Position + V2Int.down + V2Int.right,
                _Position + V2Int.up + V2Int.left,
                _Position + V2Int.up + V2Int.right
            }.Select(_P => _P.ToVector2());
            var cPos = Character.IsMoving ? 
                Character.MovingInfo.PrecisePosition : Character.Position.ToVector2();
            if (positions.Any(_P => Vector2.Distance(_P, cPos) + RazorMazeUtils.Epsilon > 1f)) 
                return;
            KillerProceedInfo = _Info;
            Character.RaiseDeath();
        }

        #endregion
    }
}