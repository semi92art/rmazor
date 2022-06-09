using System;
using System.Collections;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class MazeItemTrapIncreasingEventArgs : EventArgs
    {
        public IMazeItemProceedInfo Item  { get; }
        public int                  Stage { get; }

        public MazeItemTrapIncreasingEventArgs(IMazeItemProceedInfo _Item, int _Stage)
        {
            Item = _Item;
            Stage = _Stage;
        }
    }
    
    public delegate void MazeItemTrapIncreasingEventHandler(MazeItemTrapIncreasingEventArgs _Args);
    
    public interface ITrapsIncreasingProceeder : IItemsProceeder
    {
        event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;
    }
    
    public class TrapsIncreasingProceeder : ItemsProceederBase, IUpdateTick, ITrapsIncreasingProceeder
    {
        #region nonpublic members

        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapIncreasing};

        #endregion

        #region inject
        
        private TrapsIncreasingProceeder(
            ModelSettings    _Settings,
            IModelData       _Data,
            IModelCharacter  _Character,
            IModelGameTicker _GameTicker) 
            : base(_Settings, _Data, _Character, _GameTicker) { }

        #endregion
        
        #region api

        public event MazeItemTrapIncreasingEventHandler TrapIncreasingStageChanged;

        public void UpdateTick()
        {
            ProceedTraps();
        }

        #endregion
        
        #region nonpublic methods

        private void ProceedTraps()
        {
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                var info = ProceedInfos[i];
                if (!info.ReadyToSwitchStage || !info.IsProceeding)
                    continue;
                info.ReadyToSwitchStage = false;
                ProceedCoroutine(info, ProceedTrap(info));
            }
        }
        
        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.ProceedingStage = _Info.ProceedingStage == ModelCommonData.StageIdle
                ? ModelCommonData.TrapIncreasingStageIncreased : ModelCommonData.StageIdle;
            float duration = GetStageDuration(_Info.ProceedingStage);
            float time = GameTicker.Time;
            yield return Cor.WaitWhile(
                () => time + duration > GameTicker.Time,
                () =>
                {
                    TrapIncreasingStageChanged?.Invoke(
                    new MazeItemTrapIncreasingEventArgs(_Info, _Info.ProceedingStage));
                    _Info.ReadyToSwitchStage = true;
                });
        }
        
        private float GetStageDuration(int _Stage)
        {
            return _Stage switch
            {
                ModelCommonData.StageIdle                    => Settings.trapIncreasingIdleTime,
                ModelCommonData.TrapIncreasingStageIncreased => Settings.trapIncreasingIncreasedTime,
                _                                            => 0
            };
        }

        #endregion
    }
}