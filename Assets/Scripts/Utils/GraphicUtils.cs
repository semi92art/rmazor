using Entities;
using UnityEngine;

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

        public static int GetMenuTargetFps()
        {
            return IsGoodQuality() ? 60 : 30;
        }

        public static int GetGameTargetFps()
        {
            return IsGoodQuality() ? 120 : 60;
        }

        public static float AspectRatio
        {
            get
            {
                float aspectRatio;
#if UNITY_EDITOR
                Vector2 gameViewSize = GetMainGameViewSize();
                aspectRatio = gameViewSize.x / gameViewSize.y;
#else
                aspectRatio = (float)Screen.width / (float)Screen.height;
#endif
                return aspectRatio;   
            }
        }
        
#if UNITY_EDITOR
        private static Vector2 GetMainGameViewSize()
        {
            System.Type t = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo getSizeOfMainGameView = 
                t?.GetMethod(
                    "GetSizeOfMainGameView",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object res = getSizeOfMainGameView?.Invoke(null,null);
            return (Vector2?) res ?? Vector2.zero;
        }
#endif
    }
}