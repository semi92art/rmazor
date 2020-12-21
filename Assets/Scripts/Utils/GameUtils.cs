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
            aspectRatio = (float)Screen.width / (float)Screen.height;
#endif
            float vertExtent = Camera.main.orthographicSize;
            float horzExtent = vertExtent * aspectRatio;
            Vector3 size = new Vector3(horzExtent, vertExtent, 0);
            return new Bounds(Vector3.zero, size);
        }

        public static void DrawGizmosRect(
            Vector3 _TopLeft,
            Vector3 _TopRight,
            Vector3 _BottomLeft,
            Vector3 _BottomRight,
            Color _Color)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = _Color;
            Gizmos.DrawLine(_BottomLeft, _TopLeft);
            Gizmos.DrawLine(_TopLeft, _TopRight);
            Gizmos.DrawLine(_TopRight, _BottomRight);
            Gizmos.DrawLine(_BottomRight, _BottomLeft);
            Gizmos.color = prevColor;
        }

        public static void FillByCameraRect(SpriteRenderer _Item)
        {
            var cameraBounds = GetVisibleBounds();
            var rendererBounds = _Item.bounds;
            var transform = _Item.transform;
            transform.localScale = new Vector3(
                cameraBounds.size.x * 2f / rendererBounds.size.x,
                cameraBounds.size.y * 2f / rendererBounds.size.y);
            transform.position = new Vector3(
                rendererBounds.center.x,
                rendererBounds.center.y,
                transform.position.z);
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