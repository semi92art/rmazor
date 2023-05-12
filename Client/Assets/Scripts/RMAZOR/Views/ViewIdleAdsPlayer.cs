using Common;
using Common.Managers.Advertising;
using mazing.common.Runtime;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views
{
    public interface IViewIdleAdsPlayer : IInit { }
    
    public class ViewIdleAdsPlayer : InitBase, IViewIdleAdsPlayer, IUpdateTick
    {
        #region constants

        private const float TimeThresholdInSecs = 30f;

        #endregion
        
        #region inject
        
        private ICommonTicker               CommonTicker      { get; }
        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IAdsManager                 AdsManager        { get; }
        private IAudioManager               AudioManager      { get; }

        public ViewIdleAdsPlayer(
            ICommonTicker               _CommonTicker,
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IAdsManager                 _AdsManager,
            IAudioManager               _AudioManager)
        {
            CommonTicker      = _CommonTicker;
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
            AdsManager        = _AdsManager;
            AudioManager      = _AudioManager;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            CommonTicker.Register(this);
            base.Init();
        }

        public void UpdateTick()
        {
            if (!Initialized)
                return;
            ShowAdIfNoActionsForLongTime();
        }

        #endregion

        #region nonpublic methods
        
        private void ShowAdIfNoActionsForLongTime()
        {
            if (!IsValidLevelStage() 
                || !IsBeenEnoughTime()
                || !AdsManager.RewardedAdReady
                || !CommonDataMazor.Release)
            {
                return;
            }
            CommandsProceeder.TimeFromLastCommandInSecs = 0f;
            void OnBeforeAdShown() => AudioManager.MuteAudio(EAudioClipType.Music);
            void OnAdClosed()      => AudioManager.UnmuteAudio(EAudioClipType.Music);
            AdsManager.ShowRewardedAd(OnBeforeAdShown, _OnClosed: OnAdClosed);
        }

        private bool IsValidLevelStage()
        {
            var levelStage = Model.LevelStaging.LevelStage;
            switch (levelStage)
            {
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Paused:
                    return true;
                case ELevelStage.Finished when !RmazorUtils.IsLastLevelInGroup(Model.LevelStaging.LevelIndex):
                    return true;
                case ELevelStage.None:
                case ELevelStage.Finished:
                case ELevelStage.Loaded:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    return false;
                default: throw new SwitchCaseNotImplementedException(levelStage);
            }
        }

        private bool IsBeenEnoughTime()
        {
            return CommandsProceeder.TimeFromLastCommandInSecs > TimeThresholdInSecs
                   && AdsManager.RewardedAdReady;
        }

        #endregion
    }
}