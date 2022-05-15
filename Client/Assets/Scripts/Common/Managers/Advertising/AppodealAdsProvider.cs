using Common.Ticker;
using UnityEngine.Events;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;

namespace Common.Managers.Advertising
{
    public interface IAppodealAdsProvider : IAdsProvider { }
    
    public class AppodealAdsProvider :  AdsProviderCommonBase, IAppodealAdsProvider
    {
        #region inject
        
        private IAppodealInterstitialAd InterstitialAd { get; }
        private IAppodealRewardedAd     RewardedAd     { get; }

        public AppodealAdsProvider(
            IAppodealInterstitialAd _InterstitialAd,
            IAppodealRewardedAd     _RewardedAd,
            IViewGameTicker      _ViewGameTicker) 
            : base(_InterstitialAd, _RewardedAd, _ViewGameTicker)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd     = _RewardedAd;
        }

        #endregion

        #region api

        public override EAdsProvider Provider            => EAdsProvider.AdMob;
        public override bool         RewardedAdReady     => RewardedAd.Ready;
        public override bool         InterstitialAdReady => InterstitialAd.Ready;

        #endregion

        #region nonpublic methods

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            
        }

        protected override void InitRewardedAd()
        {
            string testId = null; // TODO
            string unitId = TestMode ? testId : GetAdsNodeValue("appodeal", "reward");
            RewardedAd.Init(unitId);
        }

        protected override void InitInterstitialAd()
        {
            string testId = null; // TODO
            string unitId = TestMode ? testId : GetAdsNodeValue("appodeal", "interstitial");
            InterstitialAd.Init(unitId);
        }

        #endregion
    }
}