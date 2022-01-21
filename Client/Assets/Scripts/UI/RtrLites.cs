using Common.Entities.UI;
using UnityEngine;

namespace UI
{
    public static class RtrLites
    {
        public static RectTransformLite FullFill =>
            new RectTransformLite
            {
                Anchor = UiAnchor.Create(0, 0, 1, 1), 
                AnchoredPosition = Vector2.zero,
                Pivot = Vector2.one * 0.5f,
                SizeDelta = Vector2.zero
            };

        public static RectTransformLite DialogWindow =>
            new RectTransformLite
            {
                Anchor = UiAnchor.Create(0, 0, 1, 1), 
                AnchoredPosition = Vector2.up * 10f,
                Pivot = Vector2.one * 0.5f,
                SizeDelta = new Vector2(-50f, -300f)
            };
    }
}