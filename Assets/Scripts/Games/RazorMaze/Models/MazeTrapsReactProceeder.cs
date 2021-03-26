using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Utils;

namespace Games.RazorMaze.Models
{
    public class MazeItemTrapReactEventArgs : EventArgs
    {
        public MazeItem Item { get; }
        public int Stage { get; }
        public float Duration { get; }

        public MazeItemTrapReactEventArgs(MazeItem _Item, int _Stage, float _Duration)
        {
            Item = _Item;
            Stage = _Stage;
            Duration = _Duration;
        }
    }

    public delegate void MazeItemTrapReactEventHandler(MazeItemTrapReactEventArgs _Args);
    
    public interface IMazeTrapsReactProceeder
    {
        event MazeItemTrapReactEventHandler TrapReactStageChanged;
        void OnMazeChanged(MazeInfo _Info);
        void OnCharacterMoveContinued(CharacterMovingEventArgs _Args);
    }
    
    public class MazeTrapsReactProceeder : IMazeTrapsReactProceeder
    {
        #region nonpublic members

        public const int StageIdle = 0;
        public const int StagePreReact = 1;
        public const int StageReact = 2;
        public const int StageAfterReact = 3;
        
        private V2Int m_CharacterPos;
        private MazeInfo m_MazeInfo;
        private Dictionary<MazeItem, MazeItemTrapReactProceedInfo> m_Proceeds;
        
        #endregion

        #region inject

        private RazorMazeModelSettings Settings { get; }

        public MazeTrapsReactProceeder(RazorMazeModelSettings _Settings)
        {
            Settings = _Settings;
        }

        #endregion
        
        
        public event MazeItemTrapReactEventHandler TrapReactStageChanged;

        public void OnMazeChanged(MazeInfo _Info)
        {
            m_MazeInfo = _Info;
            CollectProceeds();
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var addictRaw = (_Args.To.ToVector2() - _Args.From.ToVector2()) * _Args.Progress;
            var addict = new V2Int(addictRaw);
            var newPos = _Args.From + addict;
            if (m_CharacterPos == newPos)
                return;
            m_CharacterPos = newPos;
            ProceedTrapsReact();
        }

        private void CollectProceeds()
        {
            m_Proceeds = m_MazeInfo.MazeItems
                .Where(_Item => _Item.Type == EMazeItemType.TrapReact)
                .ToDictionary(_Item => _Item, _Item => new MazeItemTrapReactProceedInfo
                {
                    IsProceeding = false,
                    ProceedingStage = 0,
                    Item = _Item,
                    PauseTimer = 0
                });
        }

        private void ProceedTrapsReact()
        {
            foreach (var proceed in m_Proceeds.Values.Where(_P => !_P.IsProceeding))
            {
                var item = proceed.Item;
                var trapExtractPos = item.Position + item.Direction;
                if (trapExtractPos != m_CharacterPos)
                    continue;
                Coroutines.Run(ProceedTrapStage(proceed));
            }
        }

        private IEnumerator ProceedTrapStage(MazeItemTrapReactProceedInfo _Info)
        {
            _Info.ProceedingStage++;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            TrapReactStageChanged?.Invoke(new MazeItemTrapReactEventArgs(_Info.Item, _Info.ProceedingStage, duration));
            float time = GameTimeProvider.Instance.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration < GameTimeProvider.Instance.Time,
                () =>
                {
                    if (_Info.ProceedingStage == StageAfterReact)
                    {
                        _Info.ProceedingStage = StageIdle;
                        return;
                    }
                    Coroutines.Run(ProceedTrapStage(_Info));
                });
        }

        private float GetStageDuration(int _Stage)
        {
            switch (_Stage)
            {
                case StagePreReact:
                    return Settings.trapPreReactTime;
                case StageReact:
                    return Settings.trapReactTime;
                case StageAfterReact:
                    return Settings.trapAfterReactTime;
                default: return 0;
            }
        }
    }
}