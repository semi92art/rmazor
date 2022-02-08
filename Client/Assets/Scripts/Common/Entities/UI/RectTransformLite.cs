using UnityEngine;

namespace Common.Entities.UI
{
    public class RectTransformLite
    {
        public UiAnchor? Anchor { get; set; }
        public Vector2? AnchoredPosition { get; set; }
        public Vector2? Pivot { get; set; }
        public Vector2? SizeDelta { get; set; }
        
        public static RectTransformLite FullFill =>
            new RectTransformLite
            {
                Anchor = UiAnchor.Create(0, 0, 1, 1), 
                AnchoredPosition = Vector2.zero,
                Pivot = Vector2.one * 0.5f,
                SizeDelta = Vector2.zero
            };
    }
}