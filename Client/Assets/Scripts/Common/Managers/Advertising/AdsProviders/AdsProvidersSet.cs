using System.Collections.Generic;

namespace Common.Managers.Advertising.AdsProviders
{
    public class AdsProvidersSet : IAdsProvidersSet
    {
#if ADMOB_API
        [Zenject.Inject] private IAdMobAdsProvider AdMobAdsProvider { get; }
#endif
#if UNITY_ADS_API
        [Zenject.Inject] private IUnityAdsProvider UnityAdsProvider { get; }
#endif
#if APPODEAL_3
        [Zenject.Inject] private IAppodealAdsProvider AppodealAdsProvider { get; }
#endif

        public List<IAdsProvider> GetProviders()
        {
            var result = new List<IAdsProvider>();
#if ADMOB_API
            result.Add(AdMobAdsProvider);
#endif
#if UNITY_ADS_API
            result.Add(UnityAdsProvider);
#endif
#if APPODEAL_3
            result.Add(AppodealAdsProvider);
#endif
            return result;
        }
    }
}