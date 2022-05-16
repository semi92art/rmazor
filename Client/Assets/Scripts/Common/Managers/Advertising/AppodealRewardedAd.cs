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

        //Called when rewarded video was loaded (precache flag shows if the loaded ad is precache).
        public void onRewardedVideoLoaded(bool isPrecache) 
        { 
            Dbg.Log("Appodeal: Video loaded"); 
        } 

        // Called when rewarded video failed to load
        public void onRewardedVideoFailedToLoad() 
        { 
            Dbg.Log("Appodeal: Video failed"); 
            DoLoadAdWithDelay = true;
        } 

        // Called when rewarded video was loaded, but cannot be shown (internal network errors, placement settings, or incorrect creative)
        public void onRewardedVideoShowFailed() 
        { 
            Dbg.Log ("Appodeal: Video show failed"); 
            DoLoadAdWithDelay = true;
        } 

        // Called when rewarded video is shown
        public void onRewardedVideoShown() 
        { 
            Dbg.Log("Appodeal: Video shown"); 
            m_DoInvokeOnShown = true;
        } 

        // Called when reward video is clicked
        public void onRewardedVideoClicked()
        { 
            Dbg.Log("Appodeal: Video clicked"); 
        } 

        // Called when rewarded video is closed
        public void onRewardedVideoClosed(bool _Finished) 
        { 
            Dbg.Log("Appodeal: Video closed"); 
        }

        // Called when rewarded video is viewed until the end
        public void onRewardedVideoFinished(double _Amount, string _Name) 
        { 
            Dbg.Log("Appodeal: Reward: " + _Amount + " " + _Name); 
        }

        //Called when rewarded video is expired and can not be shown
        public void onRewardedVideoExpired() 
        { 
            Dbg.Log("Appodeal: Video expired"); 
        }
    }
}