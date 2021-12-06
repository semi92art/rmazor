using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Constants;
using DI.Extensions;
using Entities;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public interface IAdsManager : IInit
    {
        BoolEntity        ShowAds             { get; set; }
        bool              RewardedAdReady     { get; }
        bool              InterstitialAdReady { get; }
        event UnityAction RewardedAdLoaded;
        void              ShowRewardedAd(UnityAction _OnShown);
        void              ShowInterstitialAd(UnityAction _OnShown);
    }
    
    public abstract class AdsManagerBase : IAdsManager
    {
        protected XElement    m_Ads;
        protected bool        m_Initialized;
        protected UnityAction m_OnRewardedAdShown;
        protected UnityAction m_OnInterstitialShown;
        
        protected IShopManager ShopManager { get; }

        protected AdsManagerBase(IShopManager _ShopManager)
        {
            ShopManager = _ShopManager;
        }


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

        public abstract bool RewardedAdReady     { get; }
        public abstract bool InterstitialAdReady { get; }

        public event UnityAction RewardedAdLoaded;
        public event UnityAction Initialized;

        
        public virtual void Init()
        {
            m_Ads = ResLoader.FromResources(@"configs\ads");
            InitConfigs(() =>
            {
                InitRewardedAd();
                InitInterstitialAd();
            });
            Initialized?.Invoke();
            m_Initialized = true;
        }

        public abstract void ShowRewardedAd(UnityAction _OnShown);
        public abstract void ShowInterstitialAd(UnityAction _OnShown);

        protected abstract void InitConfigs(UnityAction _OnSuccess);
        protected abstract void InitRewardedAd();
        protected abstract void InitInterstitialAd();
        
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
        
        protected string GetAdsNodeValue(string _Source, string _Type)
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
    }
}