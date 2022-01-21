using Common.Ticker;
using GameHelpers;

namespace Managers.Advertising
{
    public interface IUnityAdsInterstitialAd : IUnityAdsAd { }
    
    public class UnityAdsInterstitialAd : UnityAdsAdBase, IUnityAdsInterstitialAd
    {
        public UnityAdsInterstitialAd(CommonGameSettings _Settings, ICommonTicker _CommonTicker) 
            : base(_Settings, _CommonTicker) { }
    }
}