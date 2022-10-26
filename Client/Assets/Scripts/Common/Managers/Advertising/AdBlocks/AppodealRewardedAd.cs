#if APPODEAL_3
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
using Common.Helpers;
using Common.Ticker;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IAppodealRewardedAd : IRewardedAdBase, IRewardedVideoAdListener { }
    
    public class AppodealRewardedAd : RewardedAdBase, IAppodealRewardedAd
    {
        #region nonpublic members
        
        protected override string AdSource   => AdvertisingNetworks.Appodeal;
        protected override string AdType     => AdTypeRewarded;
        private            int    ShowStyle  => AppodealShowStyle.RewardedVideo;
        private            int    AppoAdType => AppodealAdType.RewardedVideo;

        #endregion

        #region inject
        
        public AppodealRewardedAd(
            GlobalGameSettings _GameSettings,
            ICommonTicker      _CommonTicker) 
            : base(_GameSettings, _CommonTicker) { }

        #endregion

        #region api
        
        public override bool Ready => Appodeal.IsLoaded(AppoAdType);

        public override void Init(string _AppId, string _UnitId)
        {
            base.Init(_AppId, _UnitId);
            Appodeal.SetRewardedVideoCallbacks(this);
        }

        public override void LoadAd()
        {
            Appodeal.Cache(AppoAdType);
        }
        
        public override void ShowAd(
            UnityAction _OnShown, 
            UnityAction _OnClicked, 
            UnityAction _OnReward,
            UnityAction _OnClosed)
        {
            OnShown   = _OnShown;
            OnClicked = _OnClicked;
            OnReward  = _OnReward;
            OnClosed  = _OnClosed;
            if (!Ready) 
                return;
            const string placement = "default";
            Appodeal.Show(ShowStyle, placement);
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
            OnAdClosed(); 
        }

        public void OnRewardedVideoFinished(double _Amount, string _Name)
        {
            OnAdRewardGot();
        }

        public void OnRewardedVideoExpired() 
        { 
            Dbg.Log("Appodeal: Video expired"); 
        }

        #endregion
    }
}
#endif