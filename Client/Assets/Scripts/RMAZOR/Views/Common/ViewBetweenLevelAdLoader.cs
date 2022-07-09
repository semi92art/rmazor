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
        private CommonGameSettings  Settings     { get; }

        public ViewBetweenLevelAdLoader(
            IAdsManager         _AdsManager,
            IViewUILevelSkipper _LevelSkipper,
            CommonGameSettings  _Settings)
        {
            AdsManager   = _AdsManager;
            LevelSkipper = _LevelSkipper;
            Settings     = _Settings;
        }

        #endregion

        #region api
        
        public void TryShowAd(
            long        _LevelIndex,
            UnityAction _OnBeforeAdShown,
            UnityAction _OnAfterAdShown,
            UnityAction _OnAdWasNotShown)
        {
            bool doTryToShowAd = _LevelIndex >= Settings.firstLevelToShowAds
                                 && _LevelIndex % Settings.showAdsEveryLevel == 0
                                 && !LevelSkipper.LevelSkipped;
            if (doTryToShowAd)
            {
                if (m_ShowRewardedOnUnload && AdsManager.RewardedAdReady)
                {
                    //FIXME костыль заместо временно нерабочего ShowRewardedAd(_OnBeforeAdShown, _OnAfterAdShown)
                    {
                        _OnBeforeAdShown?.Invoke();
                        AdsManager.ShowRewardedAd(null, null);
                        _OnAfterAdShown?.Invoke();
                    }
                    // AdsManager.ShowRewardedAd(_OnBeforeAdShown, _OnAfterAdShown);
                    m_ShowRewardedOnUnload = !m_ShowRewardedOnUnload;
                }
                else if (!m_ShowRewardedOnUnload && AdsManager.InterstitialAdReady)
                {
                    //FIXME костыль заместо временно нерабочего ShowInterstitialAd(_OnBeforeAdShown, _OnAfterAdShown)
                    {
                        _OnBeforeAdShown?.Invoke();
                        AdsManager.ShowInterstitialAd(null, null);
                        _OnAfterAdShown?.Invoke();
                    }
                    // AdsManager.ShowInterstitialAd(_OnBeforeAdShown, _OnAfterAdShown);
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