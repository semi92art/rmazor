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
            UnityAction _OnAfterAdShown,
            UnityAction _OnAdWasNotShown);
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
            UnityAction _OnAfterAdShown,
            UnityAction _OnAdWasNotShown)
        {
            bool doTryToShowAd = 
                _LevelIndex >= GameSettings.firstLevelToShowAds
                && (_LevelIndex + GameSettings.firstLevelToShowAds) % GameSettings.showAdsEveryLevel == 0
                && ShowAd;
            ShowAd = true;
            if (doTryToShowAd)
            {
                bool showRewardedOnUnload = UnityEngine.Random.value > GameSettings.interstitialAdsRatio;
                if (showRewardedOnUnload)
                {
                    AdsManager.ShowRewardedAd(
                        AdsManager.RewardedAdReady ? _OnBeforeAdShown : null, 
                        _OnAfterAdShown);
                }
                else
                {
                    AdsManager.ShowInterstitialAd(
                        AdsManager.InterstitialAdReady ? _OnBeforeAdShown : null,
                        _OnAfterAdShown);

                }
                if (showRewardedOnUnload && !AdsManager.RewardedAdReady
                    || !showRewardedOnUnload && !AdsManager.InterstitialAdReady)
                {
                    _OnAdWasNotShown?.Invoke();
                }
            }
            else
            {
                _OnAdWasNotShown?.Invoke();
            }
        }

        #endregion
    }
}