using System;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using TimeProviders;
using UnityGameLoopDI;
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
    
    public interface ITrapsIncreasingProceeder : IOnMazeChanged, ICharacterMoveContinued
    {
        event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;
    }
    
    public class TrapsIncreasingProceeder : ItemsProceederBase, IUpdateTick, ITrapsIncreasingProceeder
    {
        #region constants
        
        public const int StageIdle = 0;
        public const int StageIncreased = 1;
        
        #endregion
        
        #region nonpublic members
        
        private V2Int m_CharacterPosCheck;
        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapIncreasing};

        #endregion
        
        #region inject
        
        public TrapsIncreasingProceeder(ModelSettings _Settings, IModelMazeData _Data) 
            : base(_Settings, _Data) { }

        #endregion
        
        #region api

        public event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;
        
        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }
        
        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            if (!Data.ProceedingMazeItems)
                return;
            var addictRaw = (_Args.To.ToVector2() - _Args.From.ToVector2()) * _Args.Progress;
            var addict = new V2Int(addictRaw);
            var newPos = _Args.From + addict;
            if (m_CharacterPosCheck == newPos)
                return;
            m_CharacterPosCheck = newPos;
            ProceedTraps();
        }
        
        public void UpdateTick()
        {
            ProceedTraps();
        }
        
        #endregion
        
        #region nonpublic methods

        private void ProceedTraps()
        {
            foreach (var type in Types)
            {
                var infos = GetProceedInfos(type);
                foreach (var info in infos.Values.Where(_Info => !_Info.IsProceeding))
                {
                    info.IsProceeding = true;
                    Coroutines.Run(ProceedTrap(info));
                }
            }
        }
        
        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.IsProceeding = true;
            _Info.ProceedingStage = _Info.ProceedingStage == StageIdle ? StageIncreased : StageIdle;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            float time = GameTimeProvider.Instance.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTimeProvider.Instance.Time,
                () =>
                {
                    TrapIncreasingStageChanged?.Invoke(
                    new MazeItemTrapIncreasingEventArgs(_Info.Item, _Info.ProceedingStage, duration));
                    _Info.IsProceeding = false;
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

        #endregion
    }
}