using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Constants;
using DI.Extensions;
using Entities;
using Newtonsoft.Json;
using UnityEngine.Events;
using Unity.Services.Core;
using Unity.Services.Mediation;
using Utils;

namespace Managers
{
    public interface IAdsManager : IInit
    {
        BoolEntity      ShowAds             { get; set; }
        bool              RewardedAdReady     { get; }
        bool              InterstitialAdReady { get; }
        event UnityAction RewardedAdLoaded;
        void              ShowRewardedAd(UnityAction _OnPaid);
        void              ShowInterstitialAd(UnityAction _OnShown);
    }
    
    public class UnityMediationAdsManager : IAdsManager
    {

        #region nonpublic members
        
        private XElement        m_Ads;
        private UnityAction     m_OnPaid;
        private UnityAction     m_OnInterstitialShown;
        private bool            m_Initialized;
        private IRewardedAd     m_RewardedAd;
        private IInterstitialAd m_InterstitialAd;
        
        #endregion

        #region inject
        
        private IShopManager ShopManager { get; }

        public UnityMediationAdsManager(IShopManager _ShopManager)
        {
            ShopManager = _ShopManager;
        }

        #endregion

        #region api
        
        public BoolEntity ShowAds
        {
            get
            {

#if UNITY_EDITOR
                return GetShowAdsCached();
#elif UNITY_ANDROID
                return GetShowAdsAndroid();
#elif UNITY_IPHONE
                return GetShowAdsIos();
#endif
            }
            set
            {
                SaveUtils.PutValue(SaveKeys.DisableAds, !value.Value);
            }
        }

        public bool              RewardedAdReady     => m_RewardedAd.AdState == AdState.Loaded;
        public bool              InterstitialAdReady => m_InterstitialAd.AdState == AdState.Loaded;
        public event UnityAction Initialized;
        public event UnityAction RewardedAdLoaded;
        
        public async void Init()
        {
            await InitAdsConfig();
            InitRewardedAd();
            InitInterstitialAd();
            Initialized?.Invoke();
            m_Initialized = true;
        }

        public void ShowRewardedAd(UnityAction _OnPaid)
        {
            var showAds = ShowAds;
            Coroutines.Run(Coroutines.WaitWhile(
                () => showAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (showAds.Result != EEntityResult.Success)
                        return;
                    if (!showAds.Value)
                        return;
                    if (RewardedAdReady)
                    {
                        m_OnPaid = _OnPaid;
                        m_RewardedAd.Show();
                    }
                    else
                    {
                        Dbg.Log("Rewarded ad is not ready.");
                        m_RewardedAd.Load();
                    }
                }));
        }

        public void ShowInterstitialAd(UnityAction _OnShown)
        {
            var showAds = ShowAds;
            Coroutines.Run(Coroutines.WaitWhile(
                () => showAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (showAds.Result != EEntityResult.Success)
                        return;
                    if (!showAds.Value)
                        return;
                    if (InterstitialAdReady)
                    {
                        m_OnInterstitialShown = _OnShown;
                        m_InterstitialAd.Show();
                    }
                    else
                    {
                        Dbg.Log("Interstitial ad is not ready.");
                        m_InterstitialAd.Load();
                    }
                }));
        }

        #endregion

        #region nonpublic methods

        private async Task InitAdsConfig()
        {
            m_Ads = ResLoader.FromResources(@"configs\ads");
            await UnityServices.InitializeAsync();
            MediationService.Instance.ImpressionEventPublisher.OnImpression += OnImpression;
        }

        private void InitRewardedAd()
        {
            string adUnitId = GetAdsNodeValue("unity_ads", "reward");
            m_RewardedAd = MediationService.Instance.CreateRewardedAd(adUnitId);
            m_RewardedAd.OnLoaded     += OnRewardedAdLoaded;
            m_RewardedAd.OnShowed     += OnRewardedAdShowed;
            m_RewardedAd.OnFailedLoad += OnRewardedAdFailedLoad;
            m_RewardedAd.OnClicked    += OnRewardedAdClicked;
            m_RewardedAd.OnFailedShow += OnRewardedAdFailedShow;
            m_RewardedAd.OnClosed     += OnRewardedAdClosed;
            m_RewardedAd.Load();
        }

        private void InitInterstitialAd()
        {
            string adUnitId = GetAdsNodeValue("unity_ads", "interstitial");
            m_InterstitialAd = MediationService.Instance.CreateInterstitialAd(adUnitId);
            m_InterstitialAd.OnLoaded     += OnInterstitialAdLoaded;
            m_InterstitialAd.OnClicked    += OnInterstitialAdClicked;
            m_InterstitialAd.OnClosed     += OnInterstitialAdClosed;
            m_InterstitialAd.OnShowed     += OnInterstitialAdShowed;
            m_InterstitialAd.OnFailedShow += OnInterstitialAdFailedShow;
            m_InterstitialAd.OnFailedLoad += OnInterstitialAdFailedLoad;
            m_InterstitialAd.Load();
        }
        
        private void OnImpression(object _Sender, ImpressionEventArgs _Args)
        {
            var data = _Args.ImpressionData == null ? 
                "null" : JsonConvert.SerializeObject(_Args.ImpressionData, Formatting.Indented);
            Dbg.Log("Impression event from ad unit id " + _Args.AdUnitId + " " + data);
        }

        private void OnInterstitialAdLoaded(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdLoaded).WithSpaces());
        }

        private void OnInterstitialAdClicked(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdClicked).WithSpaces());
        }

        private void OnInterstitialAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdClosed).WithSpaces());
            m_InterstitialAd.Load();
        }

        private void OnInterstitialAdShowed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdShowed).WithSpaces());
            m_OnInterstitialShown?.Invoke();
            m_InterstitialAd.Load();
        }
        
        private void OnInterstitialAdFailedShow(object _Sender, ShowErrorEventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdFailedShow).WithSpaces());
        }

        private void OnInterstitialAdFailedLoad(object _Sender, LoadErrorEventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdFailedLoad).WithSpaces());
            m_InterstitialAd.Load();
        }

        private void OnRewardedAdLoaded(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdLoaded).WithSpaces());
        }

        private void OnRewardedAdShowed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdShowed).WithSpaces());
            m_RewardedAd.Load();
            m_OnPaid?.Invoke();
        }
        
        private void OnRewardedAdClicked(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdClicked).WithSpaces());
        }

        private void OnRewardedAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdClosed).WithSpaces());
            m_RewardedAd.Load();
        }
        
        private void OnRewardedAdFailedShow(object _Sender, ShowErrorEventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdFailedShow).WithSpaces());
        }
        
        private void OnRewardedAdFailedLoad(object _Sender, LoadErrorEventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdFailedLoad).WithSpaces());
            m_RewardedAd.Load();
        }


        private BoolEntity GetShowAdsCached()
        {
            var val = SaveUtils.GetValue(SaveKeys.DisableAds);
            var result = new BoolEntity {Result = EEntityResult.Success};
            if (val.HasValue)
            {
                result.Value = val.Value;
                return result;
            }
            SaveUtils.PutValue(SaveKeys.DisableAds, false);
            result.Value = false;
            return result;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        
        private BoolEntity GetShowAdsAndroid()
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
        
#elif (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
        

        private BoolEntity GetShowAdsIos()
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
        
#endif

        private string GetAdsNodeValue(string _Source, string _Type)
        {
            return m_Ads.Elements("ad")
                .First(_El =>
                    Compare(_El.Attribute("source")?.Value, _Source)
                    && Compare(_El.Attribute("os")?.Value, CommonUtils.GetOsName())
                    && Compare(_El.Attribute("type")?.Value, _Type)).Value;
        }
        
        private static bool Compare(string _S1, string _S2)
        {
            return _S1.EqualsIgnoreCase(_S2);
        }

        #endregion
    }
}
