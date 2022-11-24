using Common.Enums;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewBetweenLevelAdShower
    {
        bool ShowAd { get; set; }
        
        void TryShowAd(
            long        _LevelIndex,
            bool        _IsBonus,
            UnityAction _OnAdClosed);
    }
    
    public class ViewBetweenLevelAdShower : IViewBetweenLevelAdShower
    {
        #region inject

        private IAdsManager                         AdsManager                     { get; }
        private GlobalGameSettings                  GameSettings                   { get; }
        private IViewTimePauser                     TimePauser                     { get; }
        private IAudioManager                       AudioManager                   { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        public ViewBetweenLevelAdShower(
            IAdsManager                         _AdsManager,
            GlobalGameSettings                  _GameSettings,
            IViewTimePauser                     _TimePauser,
            IAudioManager                       _AudioManager,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
        {
            AdsManager                     = _AdsManager;
            GameSettings                   = _GameSettings;
            TimePauser                     = _TimePauser;
            AudioManager                   = _AudioManager;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion

        #region api

        public bool ShowAd { get; set; }

        public void TryShowAd(
            long        _LevelIndex,
            bool        _IsBonus,
            UnityAction _OnAdClosed)
        {
            void OnBeforeAdShown()
            {
                TimePauser.PauseTimeInGame();
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.PauseLevel, true);
            }
            void OnAdClosedOrFailedToShow()
            {
                TimePauser.UnpauseTimeInGame();
                _OnAdClosed?.Invoke();
            }
            bool DoTryShowAd()
            {
                int levelIndexInGroup = RmazorUtils.GetIndexInGroup(_LevelIndex);
                return (levelIndexInGroup == 0 || levelIndexInGroup == 2)
                       && _LevelIndex >= GameSettings.firstLevelToShowAds
                       && !_IsBonus
                       && ShowAd;
            }
            if (DoTryShowAd())
            {
                bool showRewardedOnUnload = UnityEngine.Random.value > GameSettings.interstitialAdsRatio;
                if (showRewardedOnUnload && AdsManager.RewardedAdReady)
                {
                    AdsManager.ShowRewardedAd(
                        OnBeforeAdShown, 
                        _OnClosed: OnAdClosedOrFailedToShow,
                        _OnFailedToShow: OnAdClosedOrFailedToShow);
                }
                else if (!showRewardedOnUnload && AdsManager.InterstitialAdReady)
                {
                    AdsManager.ShowInterstitialAd(
                        OnBeforeAdShown, 
                        _OnClosed: OnAdClosedOrFailedToShow,
                        _OnFailedToShow: OnAdClosedOrFailedToShow);

                }
                if (showRewardedOnUnload && !AdsManager.RewardedAdReady
                    || !showRewardedOnUnload && !AdsManager.InterstitialAdReady)
                {
                    _OnAdClosed?.Invoke();
                }
            }
            else
            {
                _OnAdClosed?.Invoke();
            }
            ShowAd = true;
        }

        #endregion
    }
}