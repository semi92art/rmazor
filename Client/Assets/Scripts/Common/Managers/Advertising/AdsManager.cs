using System;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers.Advertising.AdsProviders;
using Common.Managers.Analytics;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Common.Managers.Advertising
{
    public interface IAdsManager : IInit
    {
        bool         RewardedAdReady     { get; }
        bool         InterstitialAdReady { get; }
        Entity<bool> ShowAds             { get; set; }
        
        void ShowRewardedAd(UnityAction     _OnBeforeShown, UnityAction _OnShown, string _AdsNetwork = null, bool _Forced = false);
        void ShowInterstitialAd(UnityAction _OnBeforeShown, UnityAction _OnShown, string _AdsNetwork = null, bool _Forced = false);
    }
    
    public class AdsManager : InitBase, IAdsManager
    {
        #region nonpublic members

        private readonly Dictionary<string, IAdsProvider> m_Providers = new Dictionary<string, IAdsProvider>();
        
        #endregion

        #region inject

        private CommonGameSettings      GameSettings     { get; }
        private IRemotePropertiesCommon RemoteProperties { get; }
        private IAdsProvidersSet        AdsProvidersSet  { get; }
        private IAnalyticsManager       AnalyticsManager { get; }

        private AdsManager(
            CommonGameSettings      _GameSettings,
            IRemotePropertiesCommon _RemoteProperties,
            IAdsProvidersSet        _AdsProvidersSet,
            IAnalyticsManager       _AnalyticsManager)
        {
            GameSettings     = _GameSettings;
            RemoteProperties = _RemoteProperties;
            AdsProvidersSet  = _AdsProvidersSet;
            AnalyticsManager = _AnalyticsManager;
        }

        #endregion

        #region api
        
        public Entity<bool> ShowAds 
        {
            get => GetShowAdsCached();
            set => SaveUtils.PutValue(SaveKeysCommon.DisableAds, !value.Value);
        }

        public bool RewardedAdReady     => m_Providers.Values.Any(_P => _P.RewardedAdReady);
        public bool InterstitialAdReady => m_Providers.Values.Any(_P => _P.InterstitialAdReady);
        
        public override void Init()
        {
            if (Initialized)
                return;
            InitProviders();
            base.Init();
        }

        public void ShowRewardedAd(
            UnityAction _OnBeforeShown,
            UnityAction _OnShown,
            string      _AdsNetwork = null,
            bool        _Forced     = false)
        {
            var providers = (string.IsNullOrEmpty(_AdsNetwork)
                    ? m_Providers.Values
                    : m_Providers.Where(_Kvp => _Kvp.Key == _AdsNetwork)
                        .Select(_Kvp => _Kvp.Value)).ToList();
            var readyProviders = providers
                .Where(_P =>  _P.Initialized && _P.RewardedAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Rewarded ad was not ready to be shown.");
                foreach (var provider in providers.Where(_P => _P.Initialized && _P.RewardedAdReady))
                    provider.LoadAd(AdvertisingType.Rewarded);
                return;
            }
            ShowAd(providers, _OnBeforeShown, _OnShown, AdvertisingType.Rewarded, _Forced);
        }

        public void ShowInterstitialAd(
            UnityAction _OnBeforeShown,
            UnityAction _OnShown,
            string      _AdsNetwork = null,
            bool        _Forced     = false)
        {
            var providers = (string.IsNullOrEmpty(_AdsNetwork)
                ? m_Providers.Values
                : m_Providers.Where(_Kvp => _Kvp.Key == _AdsNetwork)
                    .Select(_Kvp => _Kvp.Value)).ToList();
            var readyProviders = providers
                .Where(_P => _P.Initialized && _P.InterstitialAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Interstitial ad was not ready to be shown.");
                foreach (var provider in providers.Where(_P => _P != null && _P.Initialized && _P.InterstitialAdReady))
                    provider.LoadAd(AdvertisingType.Interstitial);
                return;
            }
            ShowAd(readyProviders, _OnBeforeShown, _OnShown, AdvertisingType.Interstitial, _Forced);
        }

        #endregion

        #region nonpublic methods

        private void InitProviders()
        {
            bool testMode = GameSettings.testAds;
            var adsConfig = ResLoader.FromResources(@"configs\ads");
            foreach (var adsProvider in AdsProvidersSet.GetProviders())
            {
                var info = RemoteProperties.AdsProviders?.FirstOrDefault(_P =>
                    _P.Source.EqualsIgnoreCase(adsProvider.Source)) ?? new AdProviderInfo
                {
                    Source = adsProvider.Source,
                    Enabled = false,
                    ShowRate = 0f
                };
                if (!info.Enabled)
                    continue;
                adsProvider.Init(testMode, info.ShowRate, adsConfig);
                m_Providers.Add(adsProvider.Source, adsProvider);
            }
        }

        private void ShowAd(
            IReadOnlyCollection<IAdsProvider> _Providers,
            UnityAction                       _OnBeforeShown,
            UnityAction                       _OnShown,
            AdvertisingType                   _Type,
            bool                              _Forced)
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
            if (Application.isEditor)
            {
                selectedProvider = _Providers.First(
                    _P => _P.Source == AdvertisingNetworks.Admob);
            }

            if (selectedProvider == null)
                return;

            var eventData = new Dictionary<string, object>
            {
                {AnalyticIds.AdSource, selectedProvider.Source},
                {AnalyticIds.AdType, Enum.GetName(typeof(AdvertisingType), _Type)}
            };
            void OnShownExtended()
            {
                _OnShown?.Invoke();
                AnalyticsManager.SendAnalytic(AnalyticIds.AdShown, eventData);
            }
            void OnClicked()
            {
                AnalyticsManager.SendAnalytic(AnalyticIds.AdClicked, eventData);
            }
            switch (_Type)
            {
                case AdvertisingType.Interstitial:
                    selectedProvider.ShowInterstitialAd(OnShownExtended, OnClicked, ShowAds, _Forced);
                    break;
                case AdvertisingType.Rewarded:
                    selectedProvider.ShowRewardedAd(OnShownExtended, OnClicked, ShowAds, _Forced);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
        }
        
        private static Entity<bool> GetShowAdsCached()
        {
            var val = SaveUtils.GetValue(SaveKeysCommon.DisableAds);
            var result = new Entity<bool> {Result = EEntityResult.Success};
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