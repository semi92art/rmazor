using Common.Helpers;
using Common.Managers.Advertising;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewBetweenLevelAdLoader
    {
        bool ShowAd { get; set; }
        
        void TryShowAd(
            long _LevelIndex,
            UnityAction _OnBeforeAdShown,
            UnityAction _OnAdClosed);
    }
    
    public class ViewBetweenLevelAdLoader : IViewBetweenLevelAdLoader
    {
        #region inject

        private IAdsManager         AdsManager   { get; }
        private GlobalGameSettings  GameSettings { get; }

        public ViewBetweenLevelAdLoader(
            IAdsManager         _AdsManager,
            GlobalGameSettings  _GameSettings)
        {
            AdsManager   = _AdsManager;
            GameSettings = _GameSettings;
        }

        #endregion

        #region api

        public bool ShowAd { get; set; }

        public void TryShowAd(
            long        _LevelIndex,
            UnityAction _OnBeforeAdShown,
            UnityAction _OnAdClosed)
        {
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
                    AdsManager.ShowRewardedAd(_OnBeforeAdShown, _OnClosed: _OnAdClosed, _OnFailedToShow: _OnAdClosed);
                }
                else if (!showRewardedOnUnload && AdsManager.InterstitialAdReady)
                {
                    AdsManager.ShowInterstitialAd(_OnBeforeAdShown, _OnClosed: _OnAdClosed, _OnFailedToShow: _OnAdClosed);

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