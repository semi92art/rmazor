using DI.Extensions;
using UnityEngine;

namespace Utils
{
    public static class GameUtils
    {
        public static Bounds GetVisibleBounds()
        {
            float vertExtent = Camera.main.orthographicSize * 2f;
            float horzExtent = vertExtent * GraphicUtils.AspectRatio;
            Vector3 size = new Vector3(horzExtent, vertExtent, 0);
            return new Bounds(Camera.main.transform.position.SetZ(0), size);
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
                cameraBounds.size.x / rendererBounds.size.x,
                cameraBounds.size.y / rendererBounds.size.y);
            transform.position = new Vector3(
                rendererBounds.center.x,
                rendererBounds.center.y,
                transform.position.z);
        }
    }
}