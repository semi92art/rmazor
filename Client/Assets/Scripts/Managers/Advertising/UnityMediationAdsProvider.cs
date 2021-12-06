using Entities;
using Unity.Services.Core;
using UnityEngine.Events;
using Utils; 
// using Unity.Services.Mediation;

namespace Managers.Advertising
{
    
    public class UnityMediationAdsProvider : AdsProviderBase
    {

        #region nonpublic members

        // private IRewardedAd     m_RewardedAd;
        // private IInterstitialAd m_InterstitialAd;
        
        #endregion

        #region inject

        public UnityMediationAdsProvider(IShopManager _ShopManager) : base(_ShopManager) { }

        #endregion

        #region api
        
        public override bool              RewardedAdReady     => false; // m_RewardedAd.AdState == AdState.Loaded;
        public override bool              InterstitialAdReady => false;// m_InterstitialAd.AdState == AdState.Loaded;

        public override void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
                    if (RewardedAdReady)
                    {
                        m_OnRewardedAdShown = _OnShown;
                        // m_RewardedAd.Show();
                    }
                    else
                    {
                        Dbg.Log("Rewarded ad is not ready.");
                        // m_RewardedAd.Load();
                    }
                }));
        }

        public override void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
                    if (InterstitialAdReady)
                    {
                        m_OnInterstitialShown = _OnShown;
                        // m_InterstitialAd.Show();
                    }
                    else
                    {
                        Dbg.Log("Interstitial ad is not ready.");
                        // m_InterstitialAd.Load();
                    }
                }));
        }

        protected override async void InitConfigs(UnityAction _OnSuccess)
        {
            await UnityServices.InitializeAsync();
            _OnSuccess?.Invoke();
            // MediationService.Instance.ImpressionEventPublisher.OnImpression += OnImpression;
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitRewardedAd()
        {
            string adUnitId = GetAdsNodeValue("unity_ads", "reward");
            // m_RewardedAd = MediationService.Instance.CreateRewardedAd(adUnitId);
            // m_RewardedAd.OnLoaded     += OnRewardedAdLoaded;
            // m_RewardedAd.OnShowed     += OnRewardedAdShowed;
            // m_RewardedAd.OnFailedLoad += OnRewardedAdFailedLoad;
            // m_RewardedAd.OnClicked    += OnRewardedAdClicked;
            // m_RewardedAd.OnFailedShow += OnRewardedAdFailedShow;
            // m_RewardedAd.OnClosed     += OnRewardedAdClosed;
            // m_RewardedAd.Load();
        }

        protected override void InitInterstitialAd()
        {
            string adUnitId = GetAdsNodeValue("unity_ads", "interstitial");
            // m_InterstitialAd = MediationService.Instance.CreateInterstitialAd(adUnitId);
            // m_InterstitialAd.OnLoaded     += OnInterstitialAdLoaded;
            // m_InterstitialAd.OnClicked    += OnInterstitialAdClicked;
            // m_InterstitialAd.OnClosed     += OnInterstitialAdClosed;
            // m_InterstitialAd.OnShowed     += OnInterstitialAdShowed;
            // m_InterstitialAd.OnFailedShow += OnInterstitialAdFailedShow;
            // m_InterstitialAd.OnFailedLoad += OnInterstitialAdFailedLoad;
            // m_InterstitialAd.Load();
        }
        
        // private void OnImpression(object _Sender, ImpressionEventArgs _Args)
        // {
        //     var data = _Args.ImpressionData == null ? 
        //         "null" : JsonConvert.SerializeObject(_Args.ImpressionData, Formatting.Indented);
        //     Dbg.Log("Impression event from ad unit id " + _Args.AdUnitId + " " + data);
        // }
        //
        // private void OnInterstitialAdLoaded(object _Sender, EventArgs _E)
        // {
        //     Dbg.Log(nameof(OnInterstitialAdLoaded).WithSpaces());
        // }
        //
        // private void OnInterstitialAdClicked(object _Sender, EventArgs _E)
        // {
        //     Dbg.Log(nameof(OnInterstitialAdClicked).WithSpaces());
        // }
        //
        // private void OnInterstitialAdClosed(object _Sender, EventArgs _E)
        // {
        //     Dbg.Log(nameof(OnInterstitialAdClosed).WithSpaces());
        //     m_InterstitialAd.Load();
        // }
        //
        // private void OnInterstitialAdShowed(object _Sender, EventArgs _E)
        // {
        //     Dbg.Log(nameof(OnInterstitialAdShowed).WithSpaces());
        //     m_OnInterstitialShown?.Invoke();
        //     m_InterstitialAd.Load();
        // }
        //
        // private void OnInterstitialAdFailedShow(object _Sender, ShowErrorEventArgs _E)
        // {
        //     Dbg.Log(nameof(OnInterstitialAdFailedShow).WithSpaces());
        // }
        //
        // private void OnInterstitialAdFailedLoad(object _Sender, LoadErrorEventArgs _E)
        // {
        //     Dbg.Log(nameof(OnInterstitialAdFailedLoad).WithSpaces());
        //     m_InterstitialAd.Load();
        // }
        //
        // private void OnRewardedAdLoaded(object _Sender, EventArgs _E)
        // {
        //     Dbg.Log(nameof(OnRewardedAdLoaded).WithSpaces());
        // }
        //
        // private void OnRewardedAdShowed(object _Sender, EventArgs _E)
        // {
        //     Dbg.Log(nameof(OnRewardedAdShowed).WithSpaces());
        //     m_RewardedAd.Load();
        //     m_OnPaid?.Invoke();
        // }
        //
        // private void OnRewardedAdClicked(object _Sender, EventArgs _E)
        // {
        //     Dbg.Log(nameof(OnRewardedAdClicked).WithSpaces());
        // }
        //
        // private void OnRewardedAdClosed(object _Sender, EventArgs _E)
        // {
        //     Dbg.Log(nameof(OnRewardedAdClosed).WithSpaces());
        //     m_RewardedAd.Load();
        // }
        //
        // private void OnRewardedAdFailedShow(object _Sender, ShowErrorEventArgs _E)
        // {
        //     Dbg.Log(nameof(OnRewardedAdFailedShow).WithSpaces());
        // }
        //
        // private void OnRewardedAdFailedLoad(object _Sender, LoadErrorEventArgs _E)
        // {
        //     Dbg.Log(nameof(OnRewardedAdFailedLoad).WithSpaces());
        //     m_RewardedAd.Load();
        // }

        #endregion
    }
}