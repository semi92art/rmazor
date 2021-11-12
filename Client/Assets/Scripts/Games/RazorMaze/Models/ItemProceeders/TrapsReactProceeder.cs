using System;
using System.Collections;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class MazeItemTrapReactEventArgs : EventArgs
    {
        public IMazeItemProceedInfo Info { get; }
        public int Stage { get; }
        public float Duration { get; }

        public MazeItemTrapReactEventArgs(IMazeItemProceedInfo _Info, int _Stage, float _Duration)
        {
            Info = _Info;
            Stage = _Stage;
            Duration = _Duration;
        }
    }

    public delegate void MazeItemTrapReactEventHandler(MazeItemTrapReactEventArgs _Args);
    
    public interface ITrapsReactProceeder : IItemsProceeder, ICharacterMoveContinued
    {
        event MazeItemTrapReactEventHandler TrapReactStageChanged;
    }
    
    public class TrapsReactProceeder : ItemsProceederBase, ITrapsReactProceeder
    {
        #region constants
        
        public const int StagePreReact = 1;
        public const int StageReact = 2;
        public const int StageAfterReact = 3;
        
        #endregion

        #region inject
        
        public TrapsReactProceeder(
            ModelSettings _Settings,
            IModelData _Data,
            IModelCharacter _Character,
            IModelLevelStaging _LevelStaging,
            IGameTicker _GameTicker) 
            : base(_Settings, _Data, _Character, _LevelStaging, _GameTicker) { }

        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.TrapReact};
        public event MazeItemTrapReactEventHandler TrapReactStageChanged;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            ProceedTraps(_Args);
        }

        #endregion
        
        #region nonpublic methods

        private void ProceedTraps(CharacterMovingEventArgs _Args)
        {
            foreach (var info in ProceedInfos
                .Where(_Info => _Info.IsProceeding && _Info.ReadyToSwitchStage))
            {
                var trapReactFinalPoint = (info.CurrentPosition + info.Direction).ToVector2();
                if (Vector2.Distance(trapReactFinalPoint, _Args.PrecisePosition) < 0.9f)
                    ProceedCoroutine(info, ProceedTrap(info));
            }
        }

        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.ReadyToSwitchStage = false;
            _Info.ProceedingStage++;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            TrapReactStageChanged?.Invoke(
                new MazeItemTrapReactEventArgs(_Info, _Info.ProceedingStage, duration));
            float time = GameTicker.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTicker.Time,
                () =>
                {
                    if (_Info.ProceedingStage == StageAfterReact)
                    {
                        _Info.ProceedingStage = StageIdle;
                        _Info.ReadyToSwitchStage = true;
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
                    return Settings.TrapPreReactTime;
                case StageReact:
                    return Settings.TrapReactTime;
                case StageAfterReact:
                    return Settings.TrapAfterReactTime;
                default: return 0;
            }
        }
        
        #endregion
    }
}