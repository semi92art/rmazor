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
        protected override int ShowStyle => AppodealShowStyle.RewardedVideo;
        protected override int AdType    => AppodealAdType.RewardedVideo;

        public AppodealRewardedAd(GlobalGameSettings _GameSettings, ICommonTicker _CommonTicker) 
            : base(_GameSettings, _CommonTicker) { }

        public override void Init(string _AppId, string _UnitId)
        {
            base.Init(_AppId, _UnitId);
            Appodeal.SetRewardedVideoCallbacks(this);
        }
        
        public void OnRewardedVideoLoaded(bool _IsPrecache) 
        { 
            Dbg.Log("Appodeal: Video loaded"); 
        } 
        
        public void OnRewardedVideoFailedToLoad() 
        { 
            Dbg.Log("Appodeal: Video failed"); 
            DoLoadAdWithDelay = true;
        }
        
        public void OnRewardedVideoShowFailed() 
        { 
            Dbg.Log ("Appodeal: Video show failed"); 
            DoLoadAdWithDelay = true;
        } 

        public void OnRewardedVideoShown() 
        { 
            Dbg.Log("Appodeal: Video shown"); 
            DoInvokeOnShown = true;
        } 

        public void OnRewardedVideoClicked()
        { 
            Dbg.Log("Appodeal: Video clicked");
            DoInvokeOnClicked = true;
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