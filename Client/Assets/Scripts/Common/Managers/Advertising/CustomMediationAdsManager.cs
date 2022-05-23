using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Exceptions;
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
        private IAdMobAdsProvider      AdMobAdsProvider      { get; }
#if UNITY_ADS_API
        [Zenject.Inject] private IUnityAdsProvider      UnityAdsProvider      { get; }
#endif
        private IAppodealAdsProvider   AppodealAdsProvider   { get; }

        public CustomMediationAdsManager(
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
            ShowAd(readyProviders, _OnBeforeShown, _OnShown, AdvertisingType.Rewarded);
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
            ShowAd(readyProviders, _OnBeforeShown, _OnShown, AdvertisingType.Interstitial);
        }

        #endregion

        #region nonpublic methods

        private void InitProviders()
        {
            bool testMode = GameSettings.testAds;
            var adsConfig = ResLoader.FromResources(@"configs\ads");
            Dictionary<string, float> providersInfo = GameSettings.adsProviders.Split(',')
                .ToDictionary(_I => _I.Split(':')[0],
                    _I => float.Parse(_I.Split(':')[1]));
            
            if (providersInfo.ContainsKey(AdvertisingNetworks.Admob))
            {
                AdMobAdsProvider.Init(testMode, providersInfo[AdvertisingNetworks.Admob], adsConfig);
                m_Providers.Add(AdMobAdsProvider);
            }
#if UNITY_ADS_API
            if (providersInfo.ContainsKey(AdvertisingNetworks.UnityAds))
            {
                UnityAdsProvider.Init(testMode, providersInfo[AdvertisingNetworks.UnityAds], adsConfig);
                m_Providers.Add(UnityAdsProvider);
            }
#endif
            if (providersInfo.ContainsKey(AdvertisingNetworks.Appodeal))
            {
                AppodealAdsProvider.Init(testMode, providersInfo[AdvertisingNetworks.Appodeal], adsConfig);
                m_Providers.Add(AppodealAdsProvider);
            }
        }

        private void ShowAd(
            IReadOnlyCollection<IAdsProvider> _Providers,
            UnityAction                       _OnBeforeShown,
            UnityAction                       _OnShown,
            AdvertisingType                   _Type)
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
                    selectedProvider?.ShowInterstitialAd(_OnShown, ShowAds);
                    break;
                case AdvertisingType.Rewarded:
                    selectedProvider?.ShowRewardedAd(_OnShown, ShowAds);
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