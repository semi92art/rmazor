using UnityEngine;

namespace Utils
{
    public static class GameUtils
    {
        public static Bounds GetVisibleBounds()
        {
            float aspectRatio;
#if UNITY_EDITOR
            Vector2 gameViewSize = GetMainGameViewSize();
            aspectRatio = gameViewSize.x / gameViewSize.y;
#else
            aspectRatio = Screen.width / Screen.height
#endif
            float vertExtent = Camera.main.orthographicSize;    
            float horzExtent = vertExtent * aspectRatio;
            Vector3 size = new Vector3(horzExtent, vertExtent, 0);
            return new Bounds(Vector3.zero, size);
        }
        
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
    }
}