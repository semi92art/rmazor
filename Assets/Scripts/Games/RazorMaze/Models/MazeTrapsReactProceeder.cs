﻿using System;
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
    
    public interface IMazeTrapsReactProceeder : IOnMazeChanged
    {
        event MazeItemTrapReactEventHandler TrapReactStageChanged;
        void OnCharacterMoveContinued(CharacterMovingEventArgs _Args);
    }
    
    public class MazeTrapsReactProceeder : IMazeTrapsReactProceeder
    {
        #region constants
        
        public const int StageIdle = 0;
        public const int StagePreReact = 1;
        public const int StageReact = 2;
        public const int StageAfterReact = 3;
        
        #endregion
        
        #region nonpublic members
        
        private V2Int m_CharacterPosCheck;

        #endregion

        #region inject

        private IModelMazeData Data { get; }
        private RazorMazeModelSettings Settings { get; }

        public MazeTrapsReactProceeder(
            IModelMazeData _Data,
            RazorMazeModelSettings _Settings)
        {
            Data = _Data;
            Settings = _Settings;
        }

        #endregion
        
        #region api
        
        public event MazeItemTrapReactEventHandler TrapReactStageChanged;

        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectProceeds();
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var addictRaw = (_Args.To.ToVector2() - _Args.From.ToVector2()) * _Args.Progress;
            var addict = new V2Int(addictRaw);
            var newPos = _Args.From + addict;
            if (m_CharacterPosCheck == newPos)
                return;
            m_CharacterPosCheck = newPos;
            ProceedTraps();
        }

        #endregion
        
        #region nonpublic methods

        private void CollectProceeds()
        {
            var proceeds = Data.Info.MazeItems
                .Where(_Item => _Item.Type == EMazeItemType.TrapReact)
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
                _P.Item.Type == EMazeItemType.TrapReact && !_P.IsProceeding))
            {
                var item = proceed.Item;
                var trapExtractPos = item.Position + item.Direction;
                if (trapExtractPos != m_CharacterPosCheck)
                    continue;
                proceed.IsProceeding = true;
                Coroutines.Run(ProceedTrap(proceed));
            }
        }

        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.IsProceeding = true;
            _Info.ProceedingStage++;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            TrapReactStageChanged?.Invoke(new MazeItemTrapReactEventArgs(_Info.Item, _Info.ProceedingStage, duration));
            float time = GameTimeProvider.Instance.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTimeProvider.Instance.Time,
                () =>
                {
                    if (_Info.ProceedingStage == StageAfterReact)
                    {
                        _Info.ProceedingStage = StageIdle;
                        _Info.IsProceeding = false;
                        return;
                    }
                    Coroutines.Run(ProceedTrap(_Info));
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
        
        #endregion
    }
}