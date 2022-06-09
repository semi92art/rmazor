using Common.Exceptions;
using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public abstract class AdsProviderCommonBase : AdsProviderBase
    {
        private readonly IAdBase         m_InterstitialAd;
        private readonly IAdBase         m_RewardedAd;
        
        private IViewGameTicker  ViewGameTicker  { get; }
        private IModelGameTicker ModelGameTicker { get; }

        protected AdsProviderCommonBase(
            IAdBase          _InterstitialAd,
            IAdBase          _RewardedAd,
            IViewGameTicker  _ViewGameTicker,
            IModelGameTicker _ModelGameTicker)
        {
            m_InterstitialAd = _InterstitialAd;
            m_RewardedAd     = _RewardedAd;
            ViewGameTicker   = _ViewGameTicker;
            ModelGameTicker  = _ModelGameTicker;
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
                if (ModelGameTicker.Pause || CommonUtils.Platform != RuntimePlatform.IPhonePlayer)
                    OnRewardedAdShown = _OnShown;
                else
                {
                    ModelGameTicker.Pause = true;
                    ViewGameTicker.Pause = true;
                    OnRewardedAdShown = () =>
                    {
                        _OnShown?.Invoke();
                        ModelGameTicker.Pause = false;
                        ViewGameTicker.Pause = false;
                    };
                }
                m_RewardedAd.ShowAd(OnRewardedAdShown);
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
                if (ModelGameTicker.Pause || CommonUtils.Platform != RuntimePlatform.IPhonePlayer)
                    OnInterstitialAdShown = _OnShown;
                else
                {
                    ModelGameTicker.Pause = true;
                    ViewGameTicker.Pause = true;
                    OnInterstitialAdShown = () =>
                    {
                        _OnShown?.Invoke();
                        ModelGameTicker.Pause = false;
                        ViewGameTicker.Pause = false;
                    };
                }
                m_InterstitialAd.ShowAd(OnInterstitialAdShown);
            }
            else
            {
                Dbg.Log("Interstitial ad is not ready.");
                m_InterstitialAd.LoadAd();
            }
        }
    }
}