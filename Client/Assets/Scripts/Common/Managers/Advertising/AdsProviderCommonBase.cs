using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public abstract class AdsProviderCommonBase : AdsProviderBase
    {
        private IAdBase         m_InterstitialAd;
        private IAdBase         m_RewardedAd;
        private IViewGameTicker GameTicker { get; }

        protected AdsProviderCommonBase(
            IAdBase         _InterstitialAd,
            IAdBase         _RewardedAd,
            IViewGameTicker _GameTicker)
        {
            m_InterstitialAd = _InterstitialAd;
            m_RewardedAd     = _RewardedAd;
            GameTicker       = _GameTicker;
        }

        protected override void ShowRewardedAdCore(UnityAction _OnShown)
        {
            if (RewardedAdReady)
            {
                if (GameTicker.Pause || CommonUtils.Platform != RuntimePlatform.IPhonePlayer)
                    OnRewardedAdShown = _OnShown;
                else
                {
                    GameTicker.Pause = true;
                    OnRewardedAdShown = () =>
                    {
                        _OnShown?.Invoke();
                        GameTicker.Pause = false;
                    };
                }
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
                if (GameTicker.Pause || CommonUtils.Platform != RuntimePlatform.IPhonePlayer)
                    OnInterstitialAdShown = _OnShown;
                else
                {
                    GameTicker.Pause = true;
                    OnInterstitialAdShown = () =>
                    {
                        _OnShown?.Invoke();
                        GameTicker.Pause = false;
                    };
                }
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