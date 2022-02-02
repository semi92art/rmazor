using System;
using System.Collections;
using System.Linq;
using Common.Entities;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class MazeItemTrapReactEventArgs : EventArgs
    {
        public IMazeItemProceedInfo Info { get; }
        public int Stage { get; }

        public MazeItemTrapReactEventArgs(IMazeItemProceedInfo _Info, int _Stage)
        {
            Info = _Info;
            Stage = _Stage;
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
            IModelGameTicker _GameTicker) 
            : base(_Settings, _Data, _Character, _GameTicker) { }

        #endregion
        
        #region api
        
        public override    EMazeItemType[]         Types                    => new[] {EMazeItemType.TrapReact};
        protected override bool                    StopProceedOnLevelFinish => false;
        public event MazeItemTrapReactEventHandler TrapReactStageChanged;

        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            ProceedTraps(_Args);
        }

        #endregion
        
        #region nonpublic methods

        private void ProceedTraps(CharacterMovingContinuedEventArgs _Args)
        {
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                var info = ProceedInfos[i];
                if (!info.IsProceeding)
                    continue;
                if (!info.ReadyToSwitchStage)
                    continue;
                var trapReactFinalPoint = (info.CurrentPosition + info.Direction);
                var path = RazorMazeUtils.GetFullPath(
                    (V2Int) _Args.PreviousPrecisePosition, (V2Int) _Args.PrecisePosition);
                if (path.Contains(trapReactFinalPoint))
                    ProceedCoroutine(info, ProceedTrap(info));
            }
        }

        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.ReadyToSwitchStage = false;
            _Info.ProceedingStage++;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            TrapReactStageChanged?.Invoke(
                new MazeItemTrapReactEventArgs(_Info, _Info.ProceedingStage));
            float time = GameTicker.Time;
            yield return Cor.WaitWhile(
                () => time + duration > GameTicker.Time,
                () =>
                {
                    if (_Info.ProceedingStage == StageAfterReact)
                    {
                        _Info.ProceedingStage = StageIdle;
                        _Info.ReadyToSwitchStage = true;
                        return;
                    }
                    Cor.Run(ProceedTrap(_Info));
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