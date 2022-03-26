using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public abstract class AdsProviderCommonBase : AdsProviderBase
    {
        private IAdBase m_InterstitialAd;
        private IAdBase m_RewardedAd;

        protected AdsProviderCommonBase(
            IAdBase _InterstitialAd,
            IAdBase _RewardedAd,
            bool    _TestMode,
            float   _ShowRate)
            : base(_TestMode, _ShowRate)
        {
            m_InterstitialAd = _InterstitialAd;
            m_RewardedAd = _RewardedAd;
        }

        protected override void ShowRewardedAdCore(UnityAction _OnShown)
        {
            if (RewardedAdReady)
            {
                OnRewardedAdShown = _OnShown;
                m_RewardedAd.ShowAd(_OnShown);
            }
            else
            {
                Dbg.Log("Rewarded ad is not ready.");
                m_RewardedAd.LoadAd();
            }
        }
        
        protected override void ShowInterstitialAdCore(UnityAction _OnShown)
        {
            if (InterstitialAdReady)
            {
                OnInterstitialAdShown = _OnShown;
                m_InterstitialAd.ShowAd(_OnShown);
            }
            else
            {
                Dbg.Log("Interstitial ad is not ready.");
                m_InterstitialAd.LoadAd();
            }
        }

    }
}