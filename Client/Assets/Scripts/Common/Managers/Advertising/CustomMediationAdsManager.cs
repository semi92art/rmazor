﻿using System.Collections.Generic;
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
        void       ShowRewardedAd(UnityAction     _OnBeforeShown, UnityAction _OnShown);
        void       ShowInterstitialAd(UnityAction _OnBeforeShown, UnityAction _OnShown);
    }
    
    public class CustomMediationAdsManager : InitBase, IAdsManager
    {
        #region nonpublic members

        private readonly List<IAdsProvider> m_Providers = new List<IAdsProvider>();
        
        #endregion

        #region inject

        private CommonGameSettings     GameSettings          { get; }
        private ICommonTicker          Ticker                { get; }
        private IAdMobAdsProvider      AdMobAdsProvider      { get; }
        private IUnityAdsProvider      UnityAdsProvider      { get; }
        private IIronSourceAdsProvider IronSourceAdsProvider { get; }

        public CustomMediationAdsManager(
            CommonGameSettings     _GameSettings,
            ICommonTicker          _Ticker,
            IAdMobAdsProvider      _AdMobAdsProvider,
            IUnityAdsProvider      _UnityAdsProvider,
            IIronSourceAdsProvider _IronSourceAdsProvider)
        {
            GameSettings          = _GameSettings;
            Ticker                = _Ticker;
            AdMobAdsProvider      = _AdMobAdsProvider;
            UnityAdsProvider      = _UnityAdsProvider;
            IronSourceAdsProvider = _IronSourceAdsProvider;
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
            if (Initialized)
                return;
            InitProviders();
            base.Init();
        }

        public void ShowRewardedAd(UnityAction _OnBeforeShown, UnityAction _OnShown)
        {
            var readyProviders = m_Providers
                .Where(_P => _P != null && _P.RewardedAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Rewarded ad was not ready to be shown.");
                return;
            }
            ShowAd(readyProviders, _OnBeforeShown, _OnShown, false);
        }

        public void ShowInterstitialAd(UnityAction _OnBeforeShown, UnityAction _OnShown)
        {
            var readyProviders = m_Providers
                .Where(_P => _P != null && _P.InterstitialAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Interstitial ad was not ready to be shown.");
                return;
            }
            ShowAd(readyProviders, _OnBeforeShown, _OnShown, true);
        }

        #endregion

        #region nonpublic methods

        private void InitProviders()
        {
            bool testMode = GameSettings.testAds;
            var adsConfig = ResLoader.FromResources(@"configs\ads");
            var adsProvider = GameSettings.adsProvider;
            if (adsProvider.HasFlag(EAdsProvider.AdMob))
            {
                AdMobAdsProvider.Init(testMode, GameSettings.admobRate, adsConfig);
                m_Providers.Add(AdMobAdsProvider);
            }
            if (adsProvider.HasFlag(EAdsProvider.UnityAds))
            {
                UnityAdsProvider.Init(testMode, GameSettings.unityAdsRate, adsConfig);
                m_Providers.Add(UnityAdsProvider);
            }
            if (adsProvider.HasFlag(EAdsProvider.IronSource))
            {
                IronSourceAdsProvider.Init(testMode, GameSettings.ironSourceRate, adsConfig);
                m_Providers.Add(IronSourceAdsProvider);
            }
        }

        private void ShowAd(
            IReadOnlyCollection<IAdsProvider> _Providers,
            UnityAction _OnBeforeShown,
            UnityAction _OnShown,
            bool _Interstitial)
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
            _OnBeforeShown?.Invoke();
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