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
        bool         RewardedAdReady             { get; }
        bool         RewardedAdNonSkippableReady { get; }
        bool         InterstitialAdReady         { get; }
        Entity<bool> ShowAds                     { get; set; }

        void ShowRewardedAd(
            UnityAction _OnBeforeShown = null,
            UnityAction _OnShown       = null,
            UnityAction _OnClicked     = null,
            UnityAction _OnReward      = null,
            UnityAction _OnClosed      = null,
            string      _AdsNetwork    = null,
            bool        _Forced        = false,
            bool        _Skippable     = true);

        void ShowInterstitialAd(
            UnityAction _OnBeforeShown = null,
            UnityAction _OnShown       = null,
            UnityAction _OnClicked     = null,
            UnityAction _OnClosed      = null,
            string      _AdsNetwork    = null,
            bool        _Forced        = false);
    }

    public class AdsManager : InitBase, IAdsManager
    {
        #region nonpublic members

        private readonly Dictionary<string, IAdsProvider> m_Providers = new Dictionary<string, IAdsProvider>();

        #endregion

        #region inject

        private GlobalGameSettings      GameGameSettings { get; }
        private IRemotePropertiesCommon RemoteProperties { get; }
        private IAdsProvidersSet        AdsProvidersSet  { get; }
        private IAnalyticsManager       AnalyticsManager { get; }

        private AdsManager(
            GlobalGameSettings      _GameGameSettings,
            IRemotePropertiesCommon _RemoteProperties,
            IAdsProvidersSet        _AdsProvidersSet,
            IAnalyticsManager       _AnalyticsManager)
        {
            GameGameSettings = _GameGameSettings;
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

        public bool RewardedAdReady             => m_Providers.Values.Any(_P => _P.RewardedAdReady);
        public bool RewardedAdNonSkippableReady => m_Providers.Values.Any(_P => _P.RewardedAdReady);
        public bool InterstitialAdReady         => m_Providers.Values.Any(_P => _P.InterstitialAdReady);

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
            UnityAction _OnClicked,
            UnityAction _OnReward,
            UnityAction _OnClosed,
            string      _AdsNetwork = null,
            bool        _Forced     = false,
            bool        _Skippable  = true)
        {
            var providers = (string.IsNullOrEmpty(_AdsNetwork)
                ? m_Providers.Values
                : m_Providers.Where(_Kvp => _Kvp.Key == _AdsNetwork)
                    .Select(_Kvp => _Kvp.Value)).ToList();
            bool IsProviderReady(IAdsProvider _Provider)
            {
                if (_Provider == null
                    || !_Provider.Initialized
                    || !_Provider.RewardedAdReady)
                {
                    return false;
                }
                return true;
            }
            var readyProviders = providers
                .Where(_P => _P.Initialized && _P.RewardedAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Rewarded ad was not ready to be shown.");
                foreach (var provider in providers
                    .Where(_P => _P != null && _P.Initialized)
                    .Where(_P => !IsProviderReady(_P)))
                {
                    provider.LoadRewardedAd();
                }
                return;
            }
            ShowAd(
                readyProviders,
                _OnBeforeShown, 
                _OnShown,
                _OnClicked, 
                _OnReward,
                _OnClosed,
                AdvertisingType.Rewarded,
                _Forced);
        }

        public void ShowInterstitialAd(
            UnityAction _OnBeforeShown,
            UnityAction _OnShown,
            UnityAction _OnClicked,
            UnityAction _OnClosed,
            string      _AdsNetwork = null,
            bool        _Forced     = false)
        {
            var providers = (string.IsNullOrEmpty(_AdsNetwork)
                ? m_Providers.Values
                : m_Providers.Where(_Kvp => _Kvp.Key == _AdsNetwork)
                    .Select(_Kvp => _Kvp.Value)).ToList();
            bool IsProviderReady(IAdsProvider _Provider)
            {
                if (_Provider == null
                    || !_Provider.Initialized
                    || !_Provider.InterstitialAdReady)
                {
                    return false;
                }
                return true;
            }
            var readyProviders = providers
                .Where(_P => _P.Initialized && _P.InterstitialAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Interstitial ad was not ready to be shown.");
                foreach (var provider in providers.Where(_P => _P != null && _P.Initialized)
                    .Where(_P => !IsProviderReady(_P)))
                {
                    provider.LoadInterstitialAd();
                }
                return;
            }
            ShowAd(
                readyProviders,
                _OnBeforeShown,
                _OnShown,
                _OnClicked, 
                null,
                _OnClosed,
                AdvertisingType.Interstitial,
                _Forced);
        }

        #endregion

        #region nonpublic methods

        private void InitProviders()
        {
            bool testMode = GameGameSettings.testAds || GameGameSettings.apkForAppodeal;
            var adsConfig = ResLoader.FromResources(@"configs\ads");
            foreach (var adsProvider in AdsProvidersSet.GetProviders())
            {
                var info = RemoteProperties.AdsProviders?.FirstOrDefault(_P =>
                    _P.Source.EqualsIgnoreCase(adsProvider.Source)) ?? new AdProviderInfo
                {
                    Source = adsProvider.Source,
                    Enabled = Application.isEditor,
                    ShowRate = 1f
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
            UnityAction                       _OnClicked,
            UnityAction                       _OnReward,
            UnityAction                       _OnClosed,
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
                {AnalyticIds.ParameterAdSource, selectedProvider.Source},
                {AnalyticIds.ParameterAdType, Enum.GetName(typeof(AdvertisingType), _Type)}
            };
            void OnShownExtended()
            {
                _OnShown?.Invoke();
                AnalyticsManager.SendAnalytic(AnalyticIds.AdShown, eventData);
            }
            void OnClicked()
            {
                _OnClicked?.Invoke();
                AnalyticsManager.SendAnalytic(AnalyticIds.AdClicked, eventData);
            }
            void OnReward()
            {
                _OnReward?.Invoke();
                AnalyticsManager.SendAnalytic(AnalyticIds.AdReward, eventData);
            }
            void OnClosed()
            {
                _OnClosed?.Invoke();
                AnalyticsManager.SendAnalytic(AnalyticIds.AdClosed, eventData);
            }
            Dbg.Log($"Selected ads provider to show {_Type} ad: { selectedProvider.Source}");
            switch (_Type)
            {
                case AdvertisingType.Interstitial:
                    selectedProvider.ShowInterstitialAd(OnShownExtended, OnClicked, OnClosed, ShowAds, _Forced);
                    break;
                case AdvertisingType.Rewarded:
                    selectedProvider.ShowRewardedAd(OnShownExtended, OnClicked, OnReward, OnClosed, ShowAds, _Forced);
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