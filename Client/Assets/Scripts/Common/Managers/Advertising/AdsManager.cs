using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Common.Managers.Advertising
{
    public interface IAdsManager : IInit
    {
        bool       RewardedAdReady     { get; }
        bool       InterstitialAdReady { get; }
        BoolEntity ShowAds             { get; set; }
        void       ShowRewardedAd(UnityAction     _OnBeforeShown, UnityAction _OnShown, string _AdsNetwork = null, bool _Forced = false);
        void       ShowInterstitialAd(UnityAction _OnBeforeShown, UnityAction _OnShown, string _AdsNetwork = null, bool _Forced = false);
    }
    
    public class AdsManager : InitBase, IAdsManager
    {
        #region nonpublic members

        private readonly Dictionary<string, IAdsProvider> m_Providers = new Dictionary<string, IAdsProvider>();
        
        #endregion

        #region inject

        private CommonGameSettings     GameSettings          { get; }
        private IAdMobAdsProvider      AdMobAdsProvider      { get; }
#if UNITY_ADS_API
        [Zenject.Inject] private IUnityAdsProvider      UnityAdsProvider      { get; }
#endif
        private IAppodealAdsProvider   AppodealAdsProvider   { get; }

        private AdsManager(
            CommonGameSettings     _GameSettings,
            IAdMobAdsProvider      _AdMobAdsProvider,
            IAppodealAdsProvider   _AppodealAdsProvider)
        {
            GameSettings          = _GameSettings;
            AdMobAdsProvider      = _AdMobAdsProvider;
            AppodealAdsProvider   = _AppodealAdsProvider;
        }

        #endregion

        #region api
        
        public BoolEntity ShowAds 
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
            Dbg.Log(nameof(ShowRewardedAd) + ": " + providers.Count);
            var readyProviders = providers
                .Where(_P => _P != null && _P.RewardedAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Rewarded ad was not ready to be shown.");
                foreach (var provider in providers.Where(_P => _P != null && _P.RewardedAdReady))
                {
                    provider.LoadAd(AdvertisingType.Rewarded);
                }
                return;
            }
            ShowAd(readyProviders, _OnBeforeShown, _OnShown, AdvertisingType.Rewarded, _Forced);
        }

        public void ShowInterstitialAd(
            UnityAction _OnBeforeShown,
            UnityAction _OnShown,
            string      _AdsNetwork = null,
            bool        _Forced     = false)
        {
            var readyProviders = (string.IsNullOrEmpty(_AdsNetwork)
                    ? m_Providers.Values
                    : m_Providers.Where(_Kvp => _Kvp.Key == _AdsNetwork)
                        .Select(_Kvp => _Kvp.Value))
                .Where(_P => _P != null && _P.InterstitialAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Interstitial ad was not ready to be shown.");
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
            Dictionary<string, float> providersRate = GameSettings.adsProviders.Split(',')
                .ToDictionary(_I => _I.Split(':')[0],
                    _I => float.Parse(_I.Split(':')[1]));
            AdMobAdsProvider.Init(testMode, providersRate.GetSafe(AdvertisingNetworks.Admob, out _), adsConfig);
            m_Providers.Add(AdvertisingNetworks.Admob, AdMobAdsProvider);
#if UNITY_ADS_API
            UnityAdsProvider.Init(testMode, providersRate.GetSafe(AdvertisingNetworks.UnityAds, out _), adsConfig);
            m_Providers.Add(AdvertisingNetworks.UnityAds, UnityAdsProvider);
#endif
            AppodealAdsProvider.Init(testMode, providersRate.GetSafe(AdvertisingNetworks.Appodeal, out _), adsConfig);
            m_Providers.Add(AdvertisingNetworks.Appodeal, AppodealAdsProvider);
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
            CommonData.DoNotShowAdvertisingAfterAppUnpause = true;
            switch (_Type)
            {
                case AdvertisingType.Interstitial:
                    selectedProvider?.ShowInterstitialAd(_OnShown, ShowAds, _Forced);
                    break;
                case AdvertisingType.Rewarded:
                    selectedProvider?.ShowRewardedAd(_OnShown, ShowAds, _Forced);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
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