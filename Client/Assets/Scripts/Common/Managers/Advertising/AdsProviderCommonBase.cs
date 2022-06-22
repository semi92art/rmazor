using Common.Exceptions;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public abstract class AdsProviderCommonBase : AdsProviderBase
    {
        private readonly IAdBase         m_InterstitialAd;
        private readonly IAdBase         m_RewardedAd;
        
        protected AdsProviderCommonBase(
            IAdBase          _InterstitialAd,
            IAdBase          _RewardedAd)
        {
            m_InterstitialAd = _InterstitialAd;
            m_RewardedAd     = _RewardedAd;
        }

        public override void LoadAd(AdvertisingType _AdvertisingType)
        {
            switch (_AdvertisingType)
            {
                case AdvertisingType.Interstitial: m_InterstitialAd.LoadAd(); break;
                case AdvertisingType.Rewarded:     m_RewardedAd.LoadAd();     break;
                default:                           throw new SwitchCaseNotImplementedException(_AdvertisingType);
            }
        }

        protected override void ShowRewardedAdCore(UnityAction _OnShown)
        {
            if (RewardedAdReady)
            {
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