using Common;
using Common.Helpers;
using Common.Managers.Advertising;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewBetweenLevelAdLoader
    {
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
        private IViewUILevelSkipper LevelSkipper { get; }
        private GlobalGameSettings  GameSettings { get; }

        public ViewBetweenLevelAdLoader(
            IAdsManager         _AdsManager,
            IViewUILevelSkipper _LevelSkipper,
            GlobalGameSettings  _GameSettings)
        {
            AdsManager   = _AdsManager;
            LevelSkipper = _LevelSkipper;
            GameSettings = _GameSettings;
        }

        #endregion

        #region api
        
        public void TryShowAd(
            long        _LevelIndex,
            UnityAction _OnBeforeAdShown,
            UnityAction _OnAfterAdShown,
            UnityAction _OnAdWasNotShown)
        {
            bool doTryToShowAd = _LevelIndex >= GameSettings.firstLevelToShowAds
                                 && _LevelIndex % GameSettings.showAdsEveryLevel == 0
                                 && !LevelSkipper.LevelSkipped
                                 && !CommonData.DoNotShowAdsAfterDeathAndReturnToPrevLevel;
            CommonData.DoNotShowAdsAfterDeathAndReturnToPrevLevel = false;
            if (doTryToShowAd)
            {
                if (m_ShowRewardedOnUnload && AdsManager.RewardedAdReady)
                {
                    AdsManager.ShowRewardedAd(_OnBeforeAdShown, _OnAfterAdShown);
                    m_ShowRewardedOnUnload = !m_ShowRewardedOnUnload;
                }
                else if (!m_ShowRewardedOnUnload && AdsManager.InterstitialAdReady)
                {
                    AdsManager.ShowInterstitialAd(_OnBeforeAdShown, _OnAfterAdShown);
                    m_ShowRewardedOnUnload = !m_ShowRewardedOnUnload;
                }
                else
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