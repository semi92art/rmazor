using System;
using System.Collections;
using System.Linq;
using Common.Entities;
using Common.Utils;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models.ItemProceeders.Additional;
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
        #region inject
        
        private TrapsReactProceeder(
            ModelSettings      _Settings,
            IModelData         _Data,
            IModelCharacter    _Character,
            IModelGameTicker   _GameTicker,
            IModelMazeRotation _Rotation) 
            : base(
                _Settings,
                _Data, 
                _Character,
                _GameTicker, 
                _Rotation) { }

        #endregion
        
        #region api

        protected override EMazeItemType[]         Types                    => new[] {EMazeItemType.TrapReact};
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
                var path = RmazorUtils.GetFullPath(
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
                    if (_Info.ProceedingStage == ModelCommonData.TrapReactStageAfterReact)
                    {
                        _Info.ProceedingStage = ModelCommonData.StageIdle;
                        _Info.ReadyToSwitchStage = true;
                        return;
                    }
                    Cor.Run(ProceedTrap(_Info));
                });
        }

        private float GetStageDuration(int _Stage)
        {
            return _Stage switch
            {
                ModelCommonData.TrapReactStagePreReact   => Settings.trapPreReactTime,
                ModelCommonData.TrapReactStageReact      => Settings.trapReactTime,
                ModelCommonData.TrapReactStageAfterReact => Settings.trapAfterReactTime,
                _                                        => 0
            };
        }
        
        #endregion
    }
}