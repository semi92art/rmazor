#if UNITY_ADS_API

using Common.Helpers;
using Common.Ticker;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IUnityAdsInterstitialAd : IUnityAdsAd { }
    
    public class UnityAdsInterstitialAd : UnityAdsAdBase, IUnityAdsInterstitialAd
    {
        protected override string AdType => AdTypeInterstitial;
        
        public UnityAdsInterstitialAd(
            GlobalGameSettings _Settings,
            ICommonTicker      _CommonTicker) 
            : base(_Settings, _CommonTicker) { }
    }
}

#endif