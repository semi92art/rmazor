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
        #region nonpublic members

        private bool m_ShowRewardedOnUnload;

        #endregion

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
            bool doTryToShowAd = _LevelIndex >= GameSettings.firstLevelToShowAds
                                 && _LevelIndex % GameSettings.showAdsEveryLevel == 0
                                 && ShowAd;
            ShowAd = true;
            if (doTryToShowAd)
            {
                if (m_ShowRewardedOnUnload)
                {
                    AdsManager.ShowRewardedAd(
                        AdsManager.RewardedAdReady ?_OnBeforeAdShown : null, 
                        _OnAfterAdShown);
                    if (AdsManager.RewardedAdReady)
                        m_ShowRewardedOnUnload = !m_ShowRewardedOnUnload;
                }
                else if (!m_ShowRewardedOnUnload)
                {
                    AdsManager.ShowInterstitialAd(
                        AdsManager.InterstitialAdReady ? _OnBeforeAdShown : null,
                        _OnAfterAdShown);
                    if (AdsManager.InterstitialAdReady)
                        m_ShowRewardedOnUnload = !m_ShowRewardedOnUnload;
                }
                if (m_ShowRewardedOnUnload && !AdsManager.RewardedAdReady
                    || !m_ShowRewardedOnUnload && !AdsManager.InterstitialAdReady)
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