
using Ticker;

namespace Managers.Advertising
{
    public interface IUnityAdsInterstitialAd : IUnityAdsAd { }
    
    public class UnityAdsInterstitialAd : UnityAdsAdBase, IUnityAdsInterstitialAd
    {
        public UnityAdsInterstitialAd(IViewGameTicker _GameTicker) : base(_GameTicker) { }
    }
}