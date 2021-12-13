
using GameHelpers;
using Ticker;

namespace Managers.Advertising
{
    public interface IUnityAdsInterstitialAd : IUnityAdsAd { }
    
    public class UnityAdsInterstitialAd : UnityAdsAdBase, IUnityAdsInterstitialAd
    {
        public UnityAdsInterstitialAd(CommonGameSettings _Settings, IViewGameTicker _GameTicker) 
            : base(_Settings, _GameTicker) { }
    }
}