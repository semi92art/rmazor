using System;
using System.Linq;
using System.Xml.Linq;
using DI.Extensions;
using Entities;
using UnityEngine.Events;
using Unity.Services.Core;
using Unity.Services.Mediation;
using Utils;

namespace Managers
{
    public interface IAdsManager : IInit
    {
        bool ShowAds { get; set; }
        bool RewardedAdReady { get; }
        event UnityAction RewardedAdLoaded;
        void ShowRewardedAd(UnityAction _OnPaid);
    }
    
    public class AdsManager : IAdsManager
    {
        #region singleton
    
        private static AdsManager _instance;
        public static AdsManager Instance => _instance ?? (_instance = new AdsManager());
    
        #endregion

        #region nonpublic members


        private static XElement _ads;

        private UnityAction m_OnPaid;
        private bool        m_ShowAds;
        private bool        m_Initialized;
        IRewardedAd         m_RewardedAd;
        

        #endregion

        #region api
        
        public bool ShowAds
        {
            get
            {
#if UNITY_EDITOR
                return !SaveUtils.GetValue<bool>(SaveKey.DisableAds);
#elif UNITY_ANDROID
                return GetShowAdsAndroid();
#elif UNITY_IPHONE
                return GetShowAdsIos();
#endif
            }
            set
            {
                
            }
        }

        public bool              RewardedAdReady => m_RewardedAd.AdState == AdState.Loaded;
        public event UnityAction Initialized;
        public event UnityAction RewardedAdLoaded;
        
        public async void Init()
        {
            _ads = ResLoader.FromResources(@"configs\ads");
            await UnityServices.InitializeAsync();
            InitRewardedAd();
            Initialized?.Invoke();
            m_Initialized = true;
        }
        
        public void ShowRewardedAd(UnityAction _OnPaid)
        {
            if (!ShowAds)
            {
                Dbg.LogWarning("Cannot show rewarded ad because ads were disabled.");
                return;
            }
            m_OnPaid = _OnPaid;
            if (RewardedAdReady)
                m_RewardedAd.Show();
            else
                m_RewardedAd.Load();
        }
        
        #endregion

        #region nonpublic methods

        private void InitRewardedAd()
        {
            m_RewardedAd = MediationService.Instance.CreateRewardedAd(GetUnityAdsRewardedAdId());
            m_RewardedAd.OnLoaded += OnRewardedAdLoaded;
            m_RewardedAd.OnShowed += OnRewardedAdShowed;
            m_RewardedAd.OnFailedLoad += OnRewardedAdFailedLoad;
            m_RewardedAd.OnClicked += OnRewardedAdClicked;
            m_RewardedAd.OnFailedShow += OnRewardedAdFailedShow;
            m_RewardedAd.OnClosed += OnRewardedAdClosed;
            m_RewardedAd.Load();
        }
        
        private void OnRewardedAdLoaded(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdLoaded).WithSpaces());
        }
        
        private void OnRewardedAdFailedLoad(object _Sender, LoadErrorEventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdFailedLoad).WithSpaces());
        }

        private void OnRewardedAdShowed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdShowed).WithSpaces());
        }
        
        private void OnRewardedAdClicked(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdClicked).WithSpaces());
        }
        
        private void OnRewardedAdFailedShow(object _Sender, ShowErrorEventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdFailedShow).WithSpaces());
        }
        
        private void OnRewardedAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdClosed).WithSpaces());
            m_RewardedAd.Load();
        }


#if UNITY_ANDROID && !UNITY_EDITOR
        
        private bool GetShowAdsAndroid()
        {
            //TODO
            return true;
        }
        
#elif (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
        

        private bool GetShowAdsIos()
        {
            //TODO
            return true;
        }
        
#endif

        private static string GetUnityAdsRewardedAdId()
        {
            return GetAdsNodeValue("unity_ads", "reward");
        }
        
        private static string GetAdsNodeValue(string _Source, string _Type)
        {
            
            return _ads.Elements("ad")
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