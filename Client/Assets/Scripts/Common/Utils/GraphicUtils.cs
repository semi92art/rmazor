using System;
using System.Reflection;
using Common.Extensions;
using UnityEngine;

namespace Common.Utils
{
    public static class GraphicUtils
    {
        public static int GetTargetFps()
        {
            return 120;
        }

        public static Vector2 ScreenSize
        {
            get
            {
                if (!Application.isEditor)
                    return new Vector2(Screen.width, Screen.height);
                Type t = Type.GetType("UnityEditor.GameView,UnityEditor");
                MethodInfo getSizeOfMainGameView = 
                    t?.GetMethod(
                        "GetSizeOfMainGameView",
                        BindingFlags.NonPublic | BindingFlags.Static);
                object res = getSizeOfMainGameView?.Invoke(null,null);
                return (Vector2?) res ?? Vector2.zero;
            }
        }

        public static float AspectRatio
        {
            get
            {
                var screenSize = ScreenSize;
                return screenSize.x / screenSize.y;
            }
        }

        public static Bounds GetVisibleBounds(Camera _Camera = null)
        {
            var cam = _Camera.IsNotNull() ? _Camera : Camera.main;
            if (cam.IsNull())
                return default;
            float vertExtent = cam!.orthographicSize * 2f;
            float horzExtent = vertExtent * AspectRatio;
            var size = new Vector3(horzExtent, vertExtent, 0);
            return new Bounds(Vector3.zero, size);
        }
    }
}