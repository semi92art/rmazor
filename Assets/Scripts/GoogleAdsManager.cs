using GoogleMobileAds.Api;

public class AdvertisingManager
{
    private BannerView m_BannerAd;
    private InterstitialAd m_FullscreenAd;
    private RewardedAd m_RewardedAd;
    private AdLoader m_NativeAd;
    
    public void InitGoogleAds()
    {
        CommonUtils.RegisterTestDevices();
        MobileAds.Initialize(initStatus => { });
        
        m_BannerAd = new BannerView(ResourcesLoader.Instance.GoogleAdsBannerId, AdSize.SmartBanner, AdPosition.Bottom);
        m_FullscreenAd = new InterstitialAd(ResourcesLoader.Instance.GoogleAdsFullscreenId);
        m_RewardedAd = new RewardedAd(ResourcesLoader.Instance.GoogleAdsRewardId);
        m_NativeAd = new AdLoader.Builder(ResourcesLoader.Instance.GoogleAdsNativeAdId).Build();
    }
    
    
}
