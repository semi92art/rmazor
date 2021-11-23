using System;
using System.Reflection;
using DI.Extensions;
using Entities;
using UnityEngine;
using Object = System.Object;

namespace Utils
{
    public static class GraphicUtils
    {

        public static bool IsGoodQuality()
        {
#if UNITY_EDITOR
            return SaveUtils.GetValue(SaveKeys.GoodQuality);
#elif UNITY_ANDROID
            return CommonUtils.GetAndroidSdkLevel() >= 27; // Android 8.1 (API level 27)
#elif UNITY_IPHONE || UNITY_IOS
            // TODO
            return true;
#endif
        }

        public static int GetTargetFps()
        {
            return IsGoodQuality() ? 120 : 60;
        }

        public static Vector2 ScreenSize
        {
            get
            {
#if UNITY_EDITOR
                return GetMainGameViewSize();
#else
                return new Vector2(Screen.width, Screen.height);
#endif
            }
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

        public static Bounds GetVisibleBounds(Camera _Camera = null)
        {
            var cam = _Camera ?? Camera.main;
            if (cam.IsNull())
                return default;
            float vertExtent = cam.orthographicSize * 2f;
            float horzExtent = vertExtent * AspectRatio;
            var size = new Vector3(horzExtent, vertExtent, 0);
            return new Bounds(cam.transform.position.SetZ(0), size);
        }
        
#if UNITY_EDITOR
        private static Vector2 GetMainGameViewSize()
        {
            Type t = Type.GetType("UnityEditor.GameView,UnityEditor");
            MethodInfo getSizeOfMainGameView = 
                t?.GetMethod(
                    "GetSizeOfMainGameView",
                    BindingFlags.NonPublic | BindingFlags.Static);
            Object res = getSizeOfMainGameView?.Invoke(null,null);
            return (Vector2?) res ?? Vector2.zero;
        }
#endif
    }
}