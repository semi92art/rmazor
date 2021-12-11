using UnityEngine;

namespace Utils
{
    public static class NetworkUtils
    {
        public static bool IsPacketSuccess(long _ResponseCode)
        {
            return MathUtils.IsInRange(_ResponseCode, 200, 299);
        }

        public static bool IsInternetConnectionAvailable()
        {
#if UNITY_EDITOR
            return Application.internetReachability != NetworkReachability.NotReachable;
#elif UNITY_ANDROID
            return MTAssets.NativeAndroidToolkit.NativeAndroid.Utils.isInternetConnectivityAvailable();
#elif UNITY_IOS || UNITY_IPHONE
            return GameClientUtils.InternetConnection;
#endif
        }
        

    }
}