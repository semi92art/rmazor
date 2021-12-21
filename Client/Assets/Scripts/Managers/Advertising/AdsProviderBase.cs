using System.Linq;
using System.Xml.Linq;
using Constants;
using DI.Extensions;
using Entities;
using Managers.IAP;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public interface IAdsProviderBase
    {
        bool RewardedAdReady     { get; }
        bool InterstitialAdReady { get; }

    }
    
    public interface IAdsProvider : IAdsProviderBase
    {
        void Init(XElement _AdsData);
        void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds);
        void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds);
    }
    
    public abstract class AdsProviderBase : IAdsProvider
    {
        protected bool        m_TestMode;
        protected XElement    m_AdsData;
        protected bool        m_Initialized;
        protected UnityAction m_OnRewardedAdShown;
        protected UnityAction m_OnInterstitialShown;
        
        protected IShopManager ShopManager { get; }

        protected AdsProviderBase(IShopManager _ShopManager, bool _TestMode)
        {
            ShopManager = _ShopManager;
            m_TestMode = _TestMode;
        }
        

        public abstract bool RewardedAdReady     { get; }
        public abstract bool InterstitialAdReady { get; }

        public event UnityAction Initialized;

        public virtual void Init(XElement _AdsData)
        {
            m_AdsData = _AdsData;
            InitConfigs(() =>
            {
                InitRewardedAd();
                InitInterstitialAd();
            });
            Initialized?.Invoke();
            m_Initialized = true;
        }

        public abstract void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds);
        public abstract void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds);

        protected abstract void InitConfigs(UnityAction _OnSuccess);
        protected abstract void InitRewardedAd();
        protected abstract void InitInterstitialAd();
        
        protected string GetAdsNodeValue(string _Source, string _Type)
        {
            return m_AdsData.Elements("ad")
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