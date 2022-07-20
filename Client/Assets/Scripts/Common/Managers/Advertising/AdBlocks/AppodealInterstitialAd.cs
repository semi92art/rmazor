#if APPODEAL_3
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
using Common.Helpers;
using Common.Ticker;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IAppodealInterstitialAd : IAdBase, IInterstitialAdListener { }
    
    public class AppodealInterstitialAd : AppodealAdBase, IAppodealInterstitialAd
    {
        protected override int ShowStyle => AppodealShowStyle.Interstitial;
        protected override int AdType    => AppodealAdType.Interstitial;

        public AppodealInterstitialAd(GlobalGameSettings _GameSettings, ICommonTicker _CommonTicker)
            : base(_GameSettings, _CommonTicker) { }
        
        public override void Init(string _AppId, string _UnitId)
        {
            base.Init(_AppId, _UnitId);
            Appodeal.SetInterstitialCallbacks(this);
        }

        public void OnInterstitialLoaded(bool _IsPrecache) 
        { 
            Dbg.Log("Appodeal: Interstitial loaded"); 
        } 

        public void OnInterstitialFailedToLoad() 
        { 
            Dbg.Log("Appodeal: Interstitial load failed"); 
            DoLoadAdWithDelay = true;
        } 

        public void OnInterstitialShowFailed() 
        { 
            Dbg.Log("Appodeal: Interstitial show failed"); 
            DoLoadAdWithDelay = true;
        } 

        public void OnInterstitialShown() 
        { 
            Dbg.Log("Appodeal: Interstitial opened");
            DoInvokeOnShown = true;
        } 

        public void OnInterstitialClosed() 
        { 
            Dbg.Log("Appodeal: Interstitial closed"); 
        } 

        public void OnInterstitialClicked() 
        {
            Dbg.Log("Appodeal: Interstitial clicked");
            DoInvokeOnClicked = true;
        } 

        public void OnInterstitialExpired() 
        {
            Dbg.Log("Appodeal: Interstitial expired"); 
        }
    }
}
#endif