using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
using Common.Helpers;
using Common.Ticker;

namespace Common.Managers.Advertising
{
    public interface IAppodealRewardedAd : IAdBase, IRewardedVideoAdListener { }
    
    public class AppodealRewardedAd : AppodealAdBase, IAppodealRewardedAd
    {
        protected override int AdType => Appodeal.REWARDED_VIDEO;
        
        public AppodealRewardedAd(CommonGameSettings _Settings, ICommonTicker _CommonTicker) 
            : base(_Settings, _CommonTicker) { }

        public override void Init(string _AppId, string _UnitId)
        {
            base.Init(_AppId, _UnitId);
            Appodeal.setRewardedVideoCallbacks(this);
        }
        
        public void onRewardedVideoLoaded(bool _IsPrecache) 
        { 
            Dbg.Log("Appodeal: Video loaded"); 
        } 
        
        public void onRewardedVideoFailedToLoad() 
        { 
            Dbg.Log("Appodeal: Video failed"); 
            DoLoadAdWithDelay = true;
        }
        
        public void onRewardedVideoShowFailed() 
        { 
            Dbg.Log ("Appodeal: Video show failed"); 
            DoLoadAdWithDelay = true;
        } 

        public void onRewardedVideoShown() 
        { 
            Dbg.Log("Appodeal: Video shown"); 
            DoInvokeOnShown = true;
        } 

        public void onRewardedVideoClicked()
        { 
            Dbg.Log("Appodeal: Video clicked"); 
        } 

        public void onRewardedVideoClosed(bool _Finished) 
        { 
            Dbg.Log("Appodeal: Video closed"); 
        }

        public void onRewardedVideoFinished(double _Amount, string _Name) 
        { 
            Dbg.Log("Appodeal: Reward: " + _Amount + " " + _Name); 
        }

        public void onRewardedVideoExpired() 
        { 
            Dbg.Log("Appodeal: Video expired"); 
        }
    }
}