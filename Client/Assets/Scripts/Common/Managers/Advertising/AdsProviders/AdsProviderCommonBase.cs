using Common.Exceptions;
using Common.Helpers;
using Common.Managers.Advertising.AdBlocks;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdsProviders
{
    public abstract class AdsProviderCommonBase : AdsProviderBase
    {
        protected readonly IAdBase InterstitialAd;
        protected readonly IAdBase RewardedAd;
        
        protected AdsProviderCommonBase(
            GlobalGameSettings _GlobalGameSettings,
            IAdBase            _InterstitialAd,
            IAdBase            _RewardedAd) 
            : base(_GlobalGameSettings)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd     = _RewardedAd;
        }

        public override void LoadAd(AdvertisingType _AdvertisingType)
        {
            switch (_AdvertisingType)
            {
                case AdvertisingType.Interstitial: InterstitialAd.LoadAd(); break;
                case AdvertisingType.Rewarded:     RewardedAd.LoadAd();     break;
                default:                           throw new SwitchCaseNotImplementedException(_AdvertisingType);
            }
        }

        protected override void ShowRewardedAdCore(UnityAction _OnShown, UnityAction _OnClicked)
        {
            if (RewardedAdReady)
            {
                RewardedAd.ShowAd(_OnShown, _OnClicked);
            }
            else
            {
                Dbg.Log("Rewarded ad is not ready.");
                RewardedAd.LoadAd();
            }
        }
        
        protected override void ShowInterstitialAdCore(UnityAction _OnShown, UnityAction _OnClicked)
        {
            if (InterstitialAdReady)
            {
                InterstitialAd.ShowAd(_OnShown, _OnClicked);
            }
            else
            {
                Dbg.Log("Interstitial ad is not ready.");
                InterstitialAd.LoadAd();
            }
        }
    }
}