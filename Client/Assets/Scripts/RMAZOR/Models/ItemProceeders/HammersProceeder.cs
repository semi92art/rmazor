using System.Collections;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class HammerShotEventArgs : MazeItemProceedInfoEventArgs
    {
        public bool Back { get; }

        public HammerShotEventArgs(IMazeItemProceedInfo _Info, bool _Back) 
            : base(_Info)
        {
            Back = _Back;
        }
    }

    public delegate void HammerEventHandler(HammerShotEventArgs _Args);

    public interface IHammersProceeder : IItemsProceeder
    {
        event HammerEventHandler HammerShot;
    }
    
    public class HammersProceeder : ItemsProceederBase, IHammersProceeder, IUpdateTick
    {
        #region nonpublic members

        protected override EMazeItemType[] Types => new[] {EMazeItemType.Hammer};
        
        #endregion

        #region inject
        
        public HammersProceeder(
            ModelSettings    _Settings,
            IModelData       _Data,
            IModelCharacter  _Character,
            IModelGameTicker _GameTicker)
            : base(_Settings, _Data, _Character, _GameTicker) { }

        #endregion

        #region api
        
        public event HammerEventHandler HammerShot;
        
        public void UpdateTick()
        {
            ProceedHammers();
        }

        #endregion

        #region nonpublic methods

        private void ProceedHammers()
        {
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                var info = ProceedInfos[i];
                if (!info.ReadyToSwitchStage || !info.IsProceeding)
                    continue;
                info.ReadyToSwitchStage = false;
                ProceedCoroutine(info, ProceedHammer(info));
            }
        }

        private IEnumerator ProceedHammer(IMazeItemProceedInfo _Info)
        {
            const int shotBack = ModelCommonData.HammerStageShotBack;
            const int shotForward = ModelCommonData.HammerStageShotForward;
            _Info.ProceedingStage = _Info.ProceedingStage == shotBack ? 
                shotForward : shotBack;
            float duration = GetStageDuration(_Info.ProceedingStage);
            float time = GameTicker.Time;
            yield return Cor.WaitWhile(
                () => time + duration > GameTicker.Time,
                () =>
                {
                    HammerShot?.Invoke(new HammerShotEventArgs(
                        _Info, 
                        _Info.ProceedingStage == shotBack));
                    _Info.ReadyToSwitchStage = true;
                });
        }
        
        private float GetStageDuration(int _Stage)
        {
            return _Stage switch
            {
                ModelCommonData.HammerStageShotBack    => Settings.hammerShotPause,
                ModelCommonData.HammerStageShotForward => Settings.hammerShotPause,
                _                                                        => 0
            };
        }

        #endregion
    }
}