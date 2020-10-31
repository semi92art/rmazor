using GoogleMobileAds.Api;
using UICreationSystem;

public class GoogleAdsManager : ISingleton
{
    private static GoogleAdsManager _instance;

    public static GoogleAdsManager Instance => _instance ?? (_instance = new GoogleAdsManager());

    private BannerView m_BannerAd;
    private InterstitialAd m_FullscreenAd;
    private RewardedAd m_RewardedAd;
    private AdLoader m_NativeAd;
    private AdRequest m_AdRequest;

    private GoogleAdsManager()
    {
        if (_instance != null)
            return;

        _instance = this;
    }

    public void Init()
    {
        RegisterTestDevices();
        MobileAds.Initialize(initStatus => { });

        m_BannerAd = new BannerView(ResLoader.GoogleAdsBannerId, AdSize.SmartBanner,
            AdPosition.Bottom);
        m_FullscreenAd = new InterstitialAd(ResLoader.GoogleAdsFullscreenId);
        m_RewardedAd = new RewardedAd(ResLoader.GoogleAdsRewardId);
        m_NativeAd = new AdLoader.Builder(ResLoader.GoogleAdsNativeAdId).Build();
        m_AdRequest = new AdRequest.Builder().Build();

        m_FullscreenAd.LoadAd(m_AdRequest);
        m_RewardedAd.LoadAd(m_AdRequest);

        m_FullscreenAd.OnAdClosed += (_, args) => { m_FullscreenAd.LoadAd(m_AdRequest); };

        m_FullscreenAd.OnAdLeavingApplication += (_, args) => { };
        m_FullscreenAd.OnAdFailedToLoad += (_, args) => { m_RewardedAd.LoadAd(m_AdRequest); };

        m_RewardedAd.OnAdClosed += (_, args) => { m_RewardedAd.LoadAd(m_AdRequest); };
    }

    public bool ShowFullscreenAd()
    {
        bool isLoaded = m_FullscreenAd.IsLoaded();
        if (isLoaded)
            m_FullscreenAd.Show();
        return isLoaded;
    }

    public bool ShowRewardedAd()
    {
        bool isLoaded = m_RewardedAd.IsLoaded();
        if (isLoaded)
            m_RewardedAd.Show();
        return isLoaded;
    }

    private static void RegisterTestDevices()
    {
        RequestConfiguration.Builder builder = new RequestConfiguration.Builder();
        builder.SetTestDeviceIds(ResLoader.GoogleTestDeviceIds);
    }

}