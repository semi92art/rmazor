#if APPODEAL_3
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
using Common.Helpers;
using Common.Ticker;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IAppodealRewardedAd : IAdBase, IRewardedVideoAdListener { }
    
    public class AppodealRewardedAd : AppodealAdBase, IAppodealRewardedAd
    {
        protected override int    ShowStyle  => AppodealShowStyle.RewardedVideo;
        protected override string AdType     => AdTypeRewarded;
        protected override int    AppoAdType => AppodealAdType.RewardedVideo;

        public AppodealRewardedAd(GlobalGameSettings _GameSettings, ICommonTicker _CommonTicker) 
            : base(_GameSettings, _CommonTicker) { }

        public override void Init(string _AppId, string _UnitId)
        {
            base.Init(_AppId, _UnitId);
            Appodeal.SetRewardedVideoCallbacks(this);
        }
        
        public void OnRewardedVideoLoaded(bool _IsPrecache) 
        { 
            OnAdLoaded();
        } 
        
        public void OnRewardedVideoFailedToLoad() 
        {
            OnAdFailedToLoad();
        }
        
        public void OnRewardedVideoShowFailed() 
        {
            OnAdFailedToShow();
        } 

        public void OnRewardedVideoShown() 
        {
            OnAdShown();
        } 

        public void OnRewardedVideoClicked()
        {
            OnAdClicked();
        } 

        public void OnRewardedVideoClosed(bool _Finished) 
        {
            Dbg.Log("Appodeal: Video closed"); 
        }

        public void OnRewardedVideoFinished(double _Amount, string _Name) 
        { 
            Dbg.Log("Appodeal: Reward: " + _Amount + " " + _Name); 
        }

        public void OnRewardedVideoExpired() 
        { 
            Dbg.Log("Appodeal: Video expired"); 
        }
    }
}
#endif