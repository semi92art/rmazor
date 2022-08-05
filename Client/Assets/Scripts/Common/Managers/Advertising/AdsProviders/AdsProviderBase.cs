using System.Linq;
using System.Xml.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdsProviders
{
    public interface IAdsProviderBase
    {
        string Source                      { get; }
        bool   RewardedAdReady             { get; }
        bool   RewardedNonSkippableAdReady { get; }
        bool   InterstitialAdReady         { get; }
        float  ShowRate                    { get; }
    }
    
    public interface IAdsProvider : IAdsProviderBase
    {
        UnityAction<bool> MuteAudio { get; set; }
        bool Initialized { get; }
        void Init(bool                      _TestMode, float _ShowRate, XElement _AdsData);
        void LoadAd(AdvertisingType         _AdvertisingType);
        void ShowRewardedAd(UnityAction     _OnShown, UnityAction _OnClicked, Entity<bool>   _ShowAds, bool _Forced, bool _Skippable);
        void ShowInterstitialAd(UnityAction _OnShown, UnityAction _OnClicked, Entity<bool> _ShowAds, bool _Forced, bool _Skippable);
    }
    
    public abstract class AdsProviderBase : IAdsProvider
    {

        #region nonpublic members

        protected virtual string AppId                      => GetAdsNodeValue(Source, "app_id");
        protected virtual string InterstitialUnitId         => GetAdsNodeValue(Source, "interstitial");
        protected virtual string RewardedUnitId             => GetAdsNodeValue(Source, "reward");
        protected virtual string RewardedNonSkippableUnitId => GetAdsNodeValue(Source, "reward_ns");

        protected bool        TestMode;
        protected XElement    AdsData;

        #endregion
        

        #region api
        
        public abstract string Source { get; }

        public abstract bool              RewardedAdReady             { get; }
        public abstract bool              RewardedNonSkippableAdReady { get; }
        public abstract bool              InterstitialAdReady         { get; }
        public          float             ShowRate                    { get; private set; }
        public          UnityAction<bool> MuteAudio                   { get; set; }
        public          bool              Initialized                 { get; private set; }

        public void Init(bool _TestMode, float _ShowRate, XElement _AdsData)
        {
            TestMode = _TestMode;
            ShowRate = _ShowRate;
            AdsData  = _AdsData;
            InitConfigs(() =>
            {
                InitRewardedAd();
                InitRewardedAdNonSkippable();
                InitInterstitialAd();
                Initialized = true;
            });
        }

        public abstract void LoadAd(AdvertisingType _AdvertisingType);

        public void ShowRewardedAd(
            UnityAction  _OnShown,
            UnityAction  _OnClicked,
            Entity<bool> _ShowAds,
            bool         _Forced, 
            bool         _Skippable)
        {
            if (_Forced)
            {
                ShowRewardedAdCore(_OnShown, _OnClicked, _Skippable);
                return;
            }
            Cor.Run(Cor.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
                    ShowRewardedAdCore(_OnShown, _OnClicked, _Skippable);
                }));
        }

        public void ShowInterstitialAd(
            UnityAction  _OnShown,
            UnityAction  _OnClicked,
            Entity<bool> _ShowAds,
            bool         _Forced, 
            bool         _Skippable)
        {
            if (_Forced)
            {
                ShowInterstitialAdCore(_OnShown, _OnClicked, _Skippable);
                return;
            }
            Cor.Run(Cor.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
                    ShowInterstitialAdCore(_OnShown, _OnClicked, _Skippable);
                }));
        }

        #endregion

        #region nonpublic methods

        protected abstract void InitConfigs(UnityAction _OnSuccess);
        protected abstract void InitRewardedAd();
        protected abstract void InitRewardedAdNonSkippable();
        protected abstract void InitInterstitialAd();

        private string GetAdsNodeValue(string _Source, string _Type)
        {
            return AdsData.Elements("ad")
                .First(_El =>
                {
                    string source = _El.Attribute("source")?.Value;
                    string os = _El.Attribute("os")?.Value;
                    string type = _El.Attribute("type")?.Value;
                    return source.EqualsIgnoreCase(_Source)
                           && os.EqualsIgnoreCase(CommonUtils.GetOsName().ToLower())
                           && type.EqualsIgnoreCase(_Type);
                }).Value;
        }
        
        protected abstract void ShowRewardedAdCore(UnityAction     _OnShown, UnityAction _OnClicked, bool _Skippable);
        protected abstract void ShowInterstitialAdCore(UnityAction _OnShown, UnityAction _OnClicked, bool _Skippable);
        
        #endregion
    }
}