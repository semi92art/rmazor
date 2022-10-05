using UnityEngine;

namespace Common.Helpers
{
    [CreateAssetMenu(fileName = "game_settings", menuName = "Configs and Sets/Game Settings", order = 1)]
    public class GlobalGameSettings : ScriptableObject
    {
        public float     adsLoadDelay;
        public bool      debugAnyway;
        public bool      testAds;
        public ELogLevel logLevel;
        public int       showAdsEveryLevel;
        public int       firstLevelToShowAds;
        public int       payToContinueMoneyCount;
        public int       moneyItemCoast;
        public bool      apkForAppodeal;
        public bool      rewardedAdReadyForTest;
        public bool      interstitialAdReadyForTest;
        public float     interstitialAdsRatio;
        public float     moneyItemsRate;
    }
}