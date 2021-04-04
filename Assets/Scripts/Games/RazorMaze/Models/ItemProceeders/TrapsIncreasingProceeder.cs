using System;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
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
    
    public interface ITrapsIncreasingProceeder : IOnMazeChanged
    {
        event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;
        void OnCharacterMoveContinued(CharacterMovingEventArgs _Args);
    }
    
    public class TrapsIncreasingProceeder : ItemsProceederBase, IUpdateTick, ITrapsIncreasingProceeder
    {
        #region constants
        
        public const int StageIdle = 0;
        public const int StageIncreased = 1;
        
        #endregion
        
        #region nonpublic members
        
        private V2Int m_CharacterPosCheck;

        #endregion
        
        #region inject

        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapIncreasing};
        private RazorMazeModelSettings Settings { get; }

        public TrapsIncreasingProceeder(
            IModelMazeData _Data,
            RazorMazeModelSettings _Settings) : base(_Data)
        {
            Settings = _Settings;
        }

        #endregion
        
        #region api

        public event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;
        
        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectProceeds();
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
        
        private void CollectProceeds()
        {
            var proceeds = Data.Info.MazeItems
                .Where(_Item => _Item.Type == EMazeItemType.TrapIncreasing)
                .Select(_Item => new MazeItemTrapReactProceedInfo
                {
                    IsProceeding = false,
                    ProceedingStage = 0,
                    Item = _Item,
                    PauseTimer = 0
                });
            foreach (var proceed in proceeds)
            {
                if (Data.ProceedInfos.ContainsKey(proceed.Item))
                    Data.ProceedInfos[proceed.Item] = proceed;
                else
                    Data.ProceedInfos.Add(proceed.Item, proceed);
            }
        }
        
        private void ProceedTraps()
        {
            foreach (var proceed in Data.ProceedInfos.Values.Where(_P => 
                _P.Item.Type == EMazeItemType.TrapIncreasing && !_P.IsProceeding))
            {
                proceed.IsProceeding = true;
                Coroutines.Run(ProceedTrap(proceed));
            }
        }
        
        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.IsProceeding = true;
            _Info.ProceedingStage = _Info.ProceedingStage == 0 ? 1 : 0;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            TrapIncreasingStageChanged?.Invoke(new MazeItemTrapIncreasingEventArgs(_Info.Item, _Info.ProceedingStage, duration));
            float time = GameTimeProvider.Instance.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTimeProvider.Instance.Time,
                () => _Info.IsProceeding = false);
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