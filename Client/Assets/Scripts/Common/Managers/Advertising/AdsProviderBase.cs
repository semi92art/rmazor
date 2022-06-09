using System.Linq;
using System.Xml.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IAdsProviderBase
    {
        string Source              { get; }
        bool   RewardedAdReady     { get; }
        bool   InterstitialAdReady { get; }
        float  ShowRate            { get; }
    }
    
    public interface IAdsProvider : IAdsProviderBase
    {
        void Init(bool                      _TestMode, float _ShowRate, XElement _AdsData);
        void LoadAd(AdvertisingType         _AdvertisingType);
        void ShowRewardedAd(UnityAction     _OnShown, BoolEntity _ShowAds, bool _Forced);
        void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds, bool _Forced);
    }
    
    public abstract class AdsProviderBase : IAdsProvider
    {
        #region nonpublic members

        protected virtual   string AppId              => GetAdsNodeValue(Source, "app_id");
        protected virtual   string InterstitialUnitId => GetAdsNodeValue(Source, "interstitial");
        protected virtual   string RewardedUnitId     => GetAdsNodeValue(Source, "reward");

        protected bool        TestMode;
        protected XElement    AdsData;
        protected UnityAction OnRewardedAdShown;
        protected UnityAction OnInterstitialAdShown;

        #endregion

        #region api
        
        public abstract string Source { get; }

        public abstract bool  RewardedAdReady     { get; }
        public abstract bool  InterstitialAdReady { get; }
        public          float ShowRate            { get; private set; }

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

        public abstract void LoadAd(AdvertisingType _AdvertisingType);

        public void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds, bool _Forced)
        {
            if (_Forced)
            {
                ShowRewardedAdCore(_OnShown);
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
                    ShowRewardedAdCore(_OnShown);
                }));
        }

        public void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds, bool _Forced)
        {
            if (_Forced)
            {
                ShowInterstitialAdCore(_OnShown);
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
                    ShowInterstitialAdCore(_OnShown);
                }));
        }

        #endregion

        #region nonpublic methods

        protected abstract void InitConfigs(UnityAction _OnSuccess);
        protected abstract void InitRewardedAd();
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
        
        private static bool Compare(string _S1, string _S2)
        {
            return _S1.EqualsIgnoreCase(_S2);
        }

        protected abstract void ShowRewardedAdCore(UnityAction     _OnShown);
        protected abstract void ShowInterstitialAdCore(UnityAction _OnShown);
        
        #endregion
    }
}