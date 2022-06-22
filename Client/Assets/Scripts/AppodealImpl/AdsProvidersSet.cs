using System.Collections.Generic;
using Common.Managers.Advertising;

namespace AppodealImpl
{
    public class AdsProvidersSet : IAdsProvidersSet
    {
        private IAdMobAdsProvider AdMobAdsProvider { get; }

#if UNITY_ADS_API
        [Zenject.Inject] private IUnityAdsProvider      UnityAdsProvider      { get; }
#endif
#if APPODEAL
        [Zenject.Inject] private IAppodealAdsProvider AppodealAdsProvider { get; }
#endif
        
        private AdsProvidersSet(IAdMobAdsProvider _AdMobAdsProvider)
        {
            AdMobAdsProvider = _AdMobAdsProvider;
        }
        
        public List<IAdsProvider> GetProviders()
        {
            var result = new List<IAdsProvider>();
            result.Add(AdMobAdsProvider);
#if UNITY_ADS_API
            result.Add(UnityAdsProvider);
#endif
#if APPODEAL
            result.Add(AppodealAdsProvider);
#endif
            return result;
        }
    }
}