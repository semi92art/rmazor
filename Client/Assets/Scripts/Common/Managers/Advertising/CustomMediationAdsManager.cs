using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IAdsManager : IInit
    {
        bool       RewardedAdReady     { get; }
        bool       InterstitialAdReady { get; }
        BoolEntity ShowAds             { get; set; }
        void       ShowRewardedAd(UnityAction     _OnShown);
        void       ShowInterstitialAd(UnityAction _OnShown);
    }
    
    public class CustomMediationAdsManager : InitBase, IAdsManager
    {
        #region nonpublic members

        private readonly List<IAdsProvider> m_Providers = new List<IAdsProvider>();
        
        #endregion

        #region inject

        private CommonGameSettings GameSettings { get; }
        private ICommonTicker      Ticker       { get; }

        public CustomMediationAdsManager(
            CommonGameSettings _GameSettings,
            ICommonTicker      _Ticker)
        {
            GameSettings = _GameSettings;
            Ticker = _Ticker;
        }

        #endregion

        #region api
        
        public BoolEntity ShowAds 
        {
            get => GetShowAdsCached();
            set => SaveUtils.PutValue(SaveKeysCommon.DisableAds, !value.Value);
        }

        public bool RewardedAdReady     => m_Providers.Any(_P => _P.RewardedAdReady);
        public bool InterstitialAdReady => m_Providers.Any(_P => _P.InterstitialAdReady);
        
        public override void Init()
        {
            InitProviders();
            base.Init();
        }

        public void ShowRewardedAd(UnityAction _OnShown)
        {
            var readyProviders = m_Providers
                .Where(_P => _P != null && _P.RewardedAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Rewarded ad was not ready to be shown.");
                return;
            }
            ShowAd(readyProviders, _OnShown, false);
        }

        public void ShowInterstitialAd(UnityAction _OnShown)
        {
            var readyProviders = m_Providers
                .Where(_P => _P != null && _P.InterstitialAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Interstitial ad was not ready to be shown.");
                return;
            }
            ShowAd(readyProviders, _OnShown, true);
        }

        #endregion

        #region nonpublic methods

        private void InitProviders()
        {
            bool testMode = GameSettings.TestAds;
            var adsConfig = ResLoader.FromResources(@"configs\ads");
            var adsProvider = GameSettings.AdsProvider;
            if (adsProvider.HasFlag(EAdsProvider.AdMob))
            {
                var man = new GoogleAdMobAdsProvider(testMode, GameSettings.admobRate);
                man.Init(adsConfig);
                m_Providers.Add(man);
            }
            if (adsProvider.HasFlag(EAdsProvider.UnityAds))
            {
                var intAd = new UnityAdsInterstitialAd(GameSettings, Ticker);
                var rewAd = new UnityAdsRewardedAd(GameSettings, Ticker);
                var man = new UnityAdsProvider(intAd, rewAd, testMode, GameSettings.unityAdsRate);
                man.Init(adsConfig);
                m_Providers.Add(man);
            }
        }

        private void ShowAd(IReadOnlyCollection<IAdsProvider> _Providers, UnityAction _OnShown, bool _Interstitial)
        {
            IAdsProvider selectedProvider = null;
            if (_Providers.Count == 1)
                selectedProvider = _Providers.First();
            else
            {
                float showRateSum = _Providers.Sum(_P => _P.ShowRate);
                float randValue = Random.value;
                float showRateSumIteration = 0f;
                foreach (var provider in _Providers)
                {
                    if (MathUtils.IsInRange(
                        randValue, 
                        showRateSumIteration / showRateSum, 
                        (showRateSumIteration + provider.ShowRate) / showRateSum))
                    {
                        selectedProvider = provider;
                        break;
                    }
                    showRateSumIteration += provider.ShowRate;
                }
            }
            if (_Interstitial)
                selectedProvider?.ShowInterstitialAd(_OnShown, ShowAds);
            else
                selectedProvider?.ShowRewardedAd(_OnShown, ShowAds);
        }
        
        private static BoolEntity GetShowAdsCached()
        {
            var val = SaveUtils.GetValue(SaveKeysCommon.DisableAds);
            var result = new BoolEntity {Result = EEntityResult.Success};
            if (val.HasValue)
            {
                result.Value = !val.Value;
                return result;
            }
            SaveUtils.PutValue(SaveKeysCommon.DisableAds, false);
            result.Value = true;
            return result;
        }

        #endregion
    }
}