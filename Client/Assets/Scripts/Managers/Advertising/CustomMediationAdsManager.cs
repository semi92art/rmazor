using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using GameHelpers;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public interface IAdsManager : IAdsProviderBase, IInit
    {
        BoolEntity ShowAds { get; set; }
        void       ShowRewardedAd(UnityAction _OnShown);
        void       ShowInterstitialAd(UnityAction _OnShown);
    }
    
    public class CustomMediationAdsManager : IAdsManager
    {
        #region nonpublic members

        private readonly List<IAdsProvider> m_Providers = new List<IAdsProvider>();
        private          IAdsProvider       m_LastRewardedAdProvider;
        private          IAdsProvider       m_LastInterstitialAdProvider;
        
        #endregion

        #region inject

        private IShopManager ShopManager  { get; }
        private CommonGameSettings GameSettings { get; }
        
        public CustomMediationAdsManager(
            IShopManager _ShopManager,
            CommonGameSettings _GameSettings)
        {
            ShopManager = _ShopManager;
            GameSettings = _GameSettings;
        }

        #endregion

        #region api
        
        public BoolEntity ShowAds 
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return GetShowAdsCached();
#elif UNITY_ANDROID
                return GetShowAdsAndroid();
#elif UNITY_IPHONE || UNITY_IOS
                return GetShowAdsIos();
#endif
            }
            set
            {
                SaveUtils.PutValue(SaveKeys.DisableAds, !value.Value);
            }
        }

        public bool RewardedAdReady     => m_Providers.Any(_P => _P.RewardedAdReady);
        public bool InterstitialAdReady => m_Providers.Any(_P => _P.InterstitialAdReady);

        public event UnityAction Initialized;

        public void Init()
        {
            InitProviders();
            Initialized?.Invoke();
        }

        public void ShowRewardedAd(UnityAction _OnShown)
        {
            var readyProviders = m_Providers
                .Where(_P => _P.RewardedAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Rewarded ad was not ready to be shown.");
                return;
            }
            if (readyProviders.Count == 1)
                m_LastRewardedAdProvider = readyProviders.FirstOrDefault();
            else
            {
                int idx = readyProviders.IndexOf(m_LastRewardedAdProvider);
                if (idx == -1)
                    m_LastRewardedAdProvider = readyProviders.FirstOrDefault();
                else
                {
                    idx = MathUtils.ClampInverse(idx + 1, 0, readyProviders.Count - 1);
                    m_LastRewardedAdProvider = readyProviders[idx];
                }
            }
            m_LastRewardedAdProvider?.ShowRewardedAd(_OnShown, ShowAds);
        }

        public void ShowInterstitialAd(UnityAction _OnShown)
        {
            var readyProviders = m_Providers
                .Where(_P => _P.InterstitialAdReady)
                .ToList();
            if (!readyProviders.Any())
            {
                Dbg.LogWarning("Interstitial ad was not ready to be shown.");
                return;
            }
            if (readyProviders.Count == 1)
                m_LastInterstitialAdProvider = readyProviders.FirstOrDefault();
            else
            {
                int idx = readyProviders.IndexOf(m_LastInterstitialAdProvider);
                if (idx == -1)
                    m_LastInterstitialAdProvider = readyProviders.FirstOrDefault();
                else
                {
                    idx = MathUtils.ClampInverse(idx + 1, 0, readyProviders.Count);
                    m_LastInterstitialAdProvider = readyProviders[idx];
                }
            }
            m_LastInterstitialAdProvider?.ShowInterstitialAd(_OnShown, ShowAds);
        }

        #endregion

        #region nonpublic methods

        private void InitProviders()
        {
            var adsConfig = ResLoader.FromResources(@"configs\ads");
            var adsProvider = GameSettings.AdsProvider;
            if (adsProvider.HasFlag(EAdsProvider.GoogleAds))
            {
                var man = new GoogleAdMobAdsProvider(ShopManager);
                man.Init(adsConfig);
                m_Providers.Add(man);
            }
            if (adsProvider.HasFlag(EAdsProvider.UnityAds))
            {
                var intAd = new UnityAdsInterstitialAd();
                var rewAd = new UnityAdsRewardedAd();
                var man = new UnityAdsProvider(intAd, rewAd, ShopManager);
                man.Init(adsConfig);
                m_Providers.Add(man);
            }
            if (adsProvider.HasFlag(EAdsProvider.UnityMediation))
            {
                var man = new UnityMediationAdsProvider(ShopManager);
                man.Init(adsConfig);
                m_Providers.Add(man);
            }
        }
        
        private static BoolEntity GetShowAdsCached()
        {
            var val = SaveUtils.GetValue(SaveKeys.DisableAds);
            var result = new BoolEntity {Result = EEntityResult.Success};
            if (val.HasValue)
            {
                result.Value = !val.Value;
                return result;
            }
            SaveUtils.PutValue(SaveKeys.DisableAds, false);
            result.Value = true;
            return result;
        }
        
#if UNITY_ANDROID && !UNITY_EDITOR
        
        private BoolEntity GetShowAdsAndroid()
        {
            return GetShowAdsCore();
        }
        
#elif (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR

        private BoolEntity GetShowAdsIos()
        {
            return GetShowAdsCore();
        }
        
#endif
        
        private BoolEntity GetShowAdsCore()
        {
            var cached = GetShowAdsCached();
            if (!cached.Value)
                return cached;
            var args = new BoolEntity();
            var itemInfo = ShopManager.GetItemInfo(PurchaseKeys.NoAds);
            Coroutines.Run(Coroutines.WaitWhile(
                () => itemInfo.Result() == EShopProductResult.Pending,
                () =>
                {
                    switch (itemInfo.Result())
                    {
                        case EShopProductResult.Success:
                            args.Result = EEntityResult.Success; 
                            break;
                        case EShopProductResult.Fail:
                            args.Result = EEntityResult.Fail;
                            break;
                    }
                    args.Value = !itemInfo.HasReceipt;
                }));
            return args;
        }

        #endregion
    }
}