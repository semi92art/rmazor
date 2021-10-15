using System;
using System.Collections;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class MazeItemTrapIncreasingEventArgs : EventArgs
    {
        public IMazeItemProceedInfo Item { get; }
        public int Stage { get; }
        public float Duration { get; }

        public MazeItemTrapIncreasingEventArgs(IMazeItemProceedInfo _Item, int _Stage, float _Duration)
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
    
    public class TrapsIncreasingProceeder : ItemsProceederBase, IUpdateTick, ITrapsIncreasingProceeder
    {
        #region constants
        
        public const int StageIncreased = 1;
        
        #endregion

        #region inject
        
        public TrapsIncreasingProceeder(
            ModelSettings _Settings,
            IModelData _Data,
            IModelCharacter _Character,
            IModelLevelStaging _LevelStaging,
            IGameTicker _GameTicker) 
            : base(_Settings, _Data, _Character, _LevelStaging, _GameTicker) { }

        #endregion
        
        #region api

        public override EMazeItemType[] Types => new[] {EMazeItemType.TrapIncreasing};
        public event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args) { }

        public void UpdateTick()
        {
            ProceedTraps();
        }

        #endregion
        
        #region nonpublic methods

        private void ProceedTraps()
        {
            foreach (var info in GetProceedInfos(Types)
                .Where(_Info => _Info.IsProceeding))
            {
                if (info.ReadyToSwitchStage)
                {
                    info.ReadyToSwitchStage = false;
                    ProceedCoroutine(ProceedTrap(info));
                }
            }
        }
        
        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.ProceedingStage = _Info.ProceedingStage == StageIdle ? StageIncreased : StageIdle;
            float duration = GetStageDuration(_Info.ProceedingStage);
            float time = GameTicker.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTicker.Time,
                () =>
                {
                    TrapIncreasingStageChanged?.Invoke(
                    new MazeItemTrapIncreasingEventArgs(_Info, _Info.ProceedingStage, duration));
                    _Info.ReadyToSwitchStage = true;
                });
        }
        
        private float GetStageDuration(int _Stage)
        {
            switch (_Stage)
            {
                case StageIdle:
                    return Settings.TrapIncreasingIdleTime;
                case StageIncreased:
                    return Settings.TrapIncreasingIncreasedTime;
                default: return 0;
            }
        }

        #endregion
    }
}