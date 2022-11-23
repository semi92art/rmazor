using Common.Enums;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewBetweenLevelAdLoader
    {
        bool ShowAd { get; set; }
        
        void TryShowAd(
            long _LevelIndex,
            UnityAction _OnAdClosed);
    }
    
    public class ViewBetweenLevelAdLoader : IViewBetweenLevelAdLoader
    {
        #region inject

        private IAdsManager        AdsManager   { get; }
        private GlobalGameSettings GameSettings { get; }
        private IViewTimePauser    TimePauser   { get; }
        private IAudioManager      AudioManager { get; }

        public ViewBetweenLevelAdLoader(
            IAdsManager         _AdsManager,
            GlobalGameSettings  _GameSettings,
            IViewTimePauser     _TimePauser,
            IAudioManager       _AudioManager)
        {
            AdsManager   = _AdsManager;
            GameSettings = _GameSettings;
            TimePauser = _TimePauser;
            AudioManager = _AudioManager;
        }

        #endregion

        #region api

        public bool ShowAd { get; set; }

        public void TryShowAd(
            long        _LevelIndex,
            UnityAction _OnAdClosed)
        {
            void OnBeforeAdShown()
            {
                TimePauser.PauseTimeInGame();
                AudioManager.MuteAudio(EAudioClipType.Music);
            }
            void OnAdClosedOrFailedToShow()
            {
                TimePauser.UnpauseTimeInGame();
                AudioManager.UnmuteAudio(EAudioClipType.Music);
                _OnAdClosed?.Invoke();
            }
            bool doTryToShowAd = 
                _LevelIndex >= GameSettings.firstLevelToShowAds
                && (_LevelIndex + GameSettings.firstLevelToShowAds) % GameSettings.showAdsEveryLevel == 0
                && ShowAd;
            ShowAd = true;
            if (doTryToShowAd)
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
        }

        #endregion
    }
}