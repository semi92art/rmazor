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
        AdMob    = 1,
        UnityAds = 2,
        IronSource = 3
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
        void Init(XElement _AdsData);
        void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds);
        void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds);
    }
    
    public abstract class AdsProviderBase : IAdsProvider
    {
        protected readonly bool        TestMode;
        protected          XElement    AdsData;
        protected          UnityAction OnRewardedAdShown;
        protected          UnityAction OnInterstitialAdShown;

        protected AdsProviderBase(bool _TestMode, float _ShowRate)
        {
            TestMode = _TestMode;
            ShowRate = _ShowRate;
        }
        
        public abstract EAdsProvider Provider            { get; }
        public abstract bool         RewardedAdReady     { get; }
        public abstract bool         InterstitialAdReady { get; }
        public          float        ShowRate            { get; }

        public virtual void Init(XElement _AdsData)
        {
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
                    && Compare(_El.Attribute("os")?.Value, CommonUtils.GetOsName())
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