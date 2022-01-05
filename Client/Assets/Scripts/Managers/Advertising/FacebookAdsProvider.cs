// using AudienceNetwork;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Views.ContainerGetters;
using Managers.IAP;
using UnityEngine;
using UnityEngine.Events;

namespace Managers.Advertising
{
    public class FacebookAdsProvider : AdsProviderBase
    {
        #region nonpublic members

        private IFacebookAd       m_InterstitialAd;
        private IFacebookAd       m_RewardedAd;

        #endregion

        #region inject

        private IContainersGetter ContainersGetter { get; }
        
        public FacebookAdsProvider(
            IShopManager _ShopManager,
            IContainersGetter _ContainersGetter,
            bool _TestMode) 
            : base(_ShopManager, _TestMode)
        {
            ContainersGetter = _ContainersGetter;
        }

        #endregion


        public override    bool RewardedAdReady     { get; }
        public override    bool InterstitialAdReady { get; }
        public override    void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            throw new System.NotImplementedException();
        }

        public override    void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            throw new System.NotImplementedException();
        }

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            // AudienceNetworkAds.Initialize();
        }

        protected override void InitRewardedAd()
        {
            var go = new GameObject("Facebook Rewarded Ad");
            go.SetParent(ContainersGetter.GetContainer(ContainerNames.Ads));
            m_InterstitialAd = go.AddComponent<FacebookInterstitialAd>();
            string placementId = GetAdsNodeValue("facebook", "rewarded");
            m_InterstitialAd.Init(placementId);
            m_InterstitialAd.LoadAd();
        }

        protected override void InitInterstitialAd()
        {
            var go = new GameObject("Facebook Interstitial Ad");
            go.SetParent(ContainersGetter.GetContainer(ContainerNames.Ads));
            m_InterstitialAd = go.AddComponent<FacebookInterstitialAd>();
            string placementId = GetAdsNodeValue("facebook", "interstitial");
            m_InterstitialAd.Init(placementId);
            m_InterstitialAd.LoadAd();
        }
    }
}