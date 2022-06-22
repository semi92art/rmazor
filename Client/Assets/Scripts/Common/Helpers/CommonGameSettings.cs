using UnityEngine;

namespace Common.Helpers
{
    [CreateAssetMenu(fileName = "game_settings", menuName = "Configs and Sets/Game Settings", order = 1)]
    public class CommonGameSettings : ScriptableObject
    {
        public string    adsProviders;
        public float     adsLoadDelay;
        public bool      debugAnyway;
        public bool      testAds;
        public ELogLevel logLevel;
        public int       showAdsEveryLevel;
        public int       firstLevelToShowAds;
        public int       payToContinueMoneyCount;
        public int       moneyItemCoast;
        public int       gameId;
    }
}