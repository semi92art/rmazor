using System;
using System.Linq;
using System.Xml.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    [Flags]
    public enum EAdsProvider
    {
        AdMob      = 1 << 0,
        UnityAds   = 1 << 1,
        IronSource = 1 << 2
    }
    
    public interface IAdsProviderBase
    {
        EAdsProvider Provider            { get; }
        bool         RewardedAdReady     { get; }
        bool         InterstitialAdReady { get; }
        float        ShowRate            { get; }
    }
    
    public interface IAdsProvider : IAdsProviderBase
    {
        void Init(bool                      _TestMode, float      _ShowRate, XElement _AdsData);
        void ShowRewardedAd(UnityAction     _OnShown,  BoolEntity _ShowAds);
        void ShowInterstitialAd(UnityAction _OnShown,  BoolEntity _ShowAds);
    }
    
    public abstract class AdsProviderBase : IAdsProvider
    {
        protected bool        TestMode;
        protected XElement    AdsData;
        protected UnityAction OnRewardedAdShown;
        protected UnityAction OnInterstitialAdShown;

        public abstract EAdsProvider Provider            { get; }
        public abstract bool         RewardedAdReady     { get; }
        public abstract bool         InterstitialAdReady { get; }
        public          float        ShowRate            { get; private set; }

        public void Init(bool _TestMode, float _ShowRate, XElement _AdsData)
        {
            TestMode = _TestMode;
            ShowRate = _ShowRate;
            AdsData = _AdsData;
            InitConfigs(() =>
            {
                InitRewardedAd();
                InitInterstitialAd();
            });
        }

        public void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            Cor.Run(Cor.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
                    ShowRewardedAdCore(_OnShown);
                }));
        }

        public void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            Cor.Run(Cor.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
                    ShowInterstitialAdCore(_OnShown);
                }));
        }

        protected abstract void InitConfigs(UnityAction _OnSuccess);
        protected abstract void InitRewardedAd();
        protected abstract void InitInterstitialAd();
        
        protected string GetAdsNodeValue(string _Source, string _Type)
        {
            return AdsData.Elements("ad")
                .First(_El =>
                    Compare(_El.Attribute("source")?.Value, _Source)
                    && Compare(_El.Attribute("os")?.Value, CommonUtils.GetOsName().ToLower())
                    && Compare(_El.Attribute("type")?.Value, _Type)).Value;
        }
        
        private static bool Compare(string _S1, string _S2)
        {
            return _S1.EqualsIgnoreCase(_S2);
        }

        protected abstract void ShowRewardedAdCore(UnityAction _OnShown);
        protected abstract void ShowInterstitialAdCore(UnityAction _OnShown);
    }
}