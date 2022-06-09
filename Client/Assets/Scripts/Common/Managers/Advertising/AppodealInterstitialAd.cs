using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
using Common.Helpers;
using Common.Ticker;

namespace Common.Managers.Advertising
{
    public interface IAppodealInterstitialAd : IAdBase, IInterstitialAdListener { }
    
    public class AppodealInterstitialAd : AppodealAdBase, IAppodealInterstitialAd
    {
        protected override int AdType => Appodeal.INTERSTITIAL;
        
        public AppodealInterstitialAd(CommonGameSettings _Settings, ICommonTicker _CommonTicker)
            : base(_Settings, _CommonTicker) { }
        
        public override void Init(string _AppId, string _UnitId)
        {
            base.Init(_AppId, _UnitId);
            Appodeal.setInterstitialCallbacks(this);
        }

        // Called when interstitial was loaded (precache flag shows if the loaded ad is precache)
        public void onInterstitialLoaded(bool _IsPrecache) 
        { 
            Dbg.Log("Appodeal: Interstitial loaded"); 
        } 

        // Called when interstitial failed to load
        public void onInterstitialFailedToLoad() 
        { 
            Dbg.Log("Appodeal: Interstitial load failed"); 
            DoLoadAdWithDelay = true;
        } 

        // Called when interstitial was loaded, but cannot be shown (internal network errors, placement settings, or incorrect creative)
        public void onInterstitialShowFailed() 
        { 
            Dbg.Log("Appodeal: Interstitial show failed"); 
            DoLoadAdWithDelay = true;
        } 

        // Called when interstitial is shown
        public void onInterstitialShown() 
        { 
            Dbg.Log("Appodeal: Interstitial opened");
            DoInvokeOnShown = true;
        } 

        // Called when interstitial is closed
        public void onInterstitialClosed() 
        { 
            Dbg.Log("Appodeal: Interstitial closed"); 
        } 

        // Called when interstitial is clicked
        public void onInterstitialClicked() 
        {
            Dbg.Log("Appodeal: Interstitial clicked"); 
        } 

        // Called when interstitial is expired and can not be shown
        public void onInterstitialExpired() 
        {
            Dbg.Log("Appodeal: Interstitial expired"); 
        }
    }
}