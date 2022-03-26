using Common.Helpers;
using Common.Ticker;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public class IronSourceAdsProvider
        : AdsProviderCommonBase, IApplicationPause
    {
        private ICommonTicker              Ticker          { get; }
        private IIronSourceInterstitialAd  InterstitialAd  { get; }
        private IIronSourceRewardedVideoAd RewardedAd      { get; }
        private CommonGameSettings         Settings        { get; }

        public IronSourceAdsProvider(
            CommonGameSettings         _Settings,
            ICommonTicker              _Ticker,
            IIronSourceInterstitialAd  _InterstitialAd,
            IIronSourceRewardedVideoAd _RewardedAd,
            bool                       _TestMode,
            float                      _ShowRate) 
            : base(
                _InterstitialAd,
                _RewardedAd,
                _TestMode,
                _ShowRate)
        {
            Ticker         = _Ticker;
            InterstitialAd = _InterstitialAd;
            RewardedAd     = _RewardedAd;
            Settings       = _Settings;
        }


        
        public void OnApplicationPause(bool _Pause)
        {
            IronSource.Agent.onApplicationPause(_Pause);
        }

        public override EAdsProvider Provider => EAdsProvider.IronSource;
        public override bool RewardedAdReady
        {
            get
            {
                if (Application.isEditor)
                    return true;
                return RewardedAd != null && RewardedAd.Ready;
            }
        }
        public override bool InterstitialAdReady
        {
            get
            {
                if (Application.isEditor)
                    return true;
                return InterstitialAd != null && InterstitialAd.Ready;
            }
        }

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            Ticker.Register(this);
            IronSourceConfig.Instance.setClientSideCallbacks(true);
            string id = IronSource.Agent.getAdvertiserId ();
            Dbg.Log ("IronSource.Agent.getAdvertiserId : " + id);
            IronSource.Agent.validateIntegration();
            Dbg.Log ("IronSource.Agent.init");
            string appKey = Application.isEditor
                ? "unexpected_platform"
                : (Application.platform == RuntimePlatform.Android
                    ? Settings.ironSourceAppKeyAndroid
                    : Settings.ironSourceAppKeyIos);
            IronSource.Agent.init (appKey);
        }

        protected override void InitRewardedAd()
        {
            RewardedAd.Init(string.Empty);
        }

        protected override void InitInterstitialAd()
        {
            InterstitialAd.Init(string.Empty);
        }

        protected override void ShowRewardedAdCore(UnityAction _OnShown)
        {
            if (RewardedAdReady)
            {
                OnRewardedAdShown = _OnShown;
                RewardedAd.ShowAd(_OnShown);
            }
            else
            {
                Dbg.Log("Rewarded ad is not ready.");
                RewardedAd.LoadAd();
            }
        }

        protected override void ShowInterstitialAdCore(UnityAction _OnShown)
        {
            if (InterstitialAdReady)
            {
                OnInterstitialAdShown = _OnShown;
                InterstitialAd.ShowAd(_OnShown);
            }
            else
            {
                Dbg.Log("Interstitial ad is not ready.");
                InterstitialAd.LoadAd();
            }
        }
    }
}