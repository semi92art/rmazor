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
        protected override int    ShowStyle  => AppodealShowStyle.Interstitial;
        protected override string AdType     => AdTypeInterstitial;
        protected override int    AppoAdType => AppodealAdType.Interstitial;

        public AppodealInterstitialAd(GlobalGameSettings _GameSettings, ICommonTicker _CommonTicker)
            : base(_GameSettings, _CommonTicker) { }
        
        public override void Init(string _AppId, string _UnitId)
        {
            base.Init(_AppId, _UnitId);
            Appodeal.SetInterstitialCallbacks(this);
        }

        public void OnInterstitialLoaded(bool _IsPrecache) 
        { 
            OnAdLoaded();
        } 

        public void OnInterstitialFailedToLoad() 
        {
            OnAdFailedToLoad();
        } 

        public void OnInterstitialShowFailed() 
        {
            OnAdFailedToShow();
        } 

        public void OnInterstitialShown() 
        {
            OnAdShown();
        } 

        public void OnInterstitialClosed() 
        { 
            Dbg.Log("Appodeal: Interstitial closed"); 
        } 

        public void OnInterstitialClicked() 
        {
            OnAdClicked();
        } 

        public void OnInterstitialExpired() 
        {
            Dbg.Log("Appodeal: Interstitial expired"); 
        }
    }
}
#endif