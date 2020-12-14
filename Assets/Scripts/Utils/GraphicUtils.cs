using Entities;

namespace Utils
{
    public static class GraphicUtils
    {
        public static bool IsGoodQuality()
        {
#if UNITY_EDITOR
            return SaveUtils.GetValue<bool>(SaveKeyDebug.GoodQuality);
#elif UNITY_ANDROID
            return CommonUtils.GetAndroidSdkLevel() >= 27; // Android 8.1 (API level 27)
#elif UNITY_IPHONE
            // TODO
            return true;
#endif
        }
    }
}