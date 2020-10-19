using UnityEngine;

namespace UICreationSystem
{
    public class RectTransformLite
    {
        public UiAnchor? Anchor { get; set; }
        public Vector2? AnchoredPosition { get; set; }
        public Vector2? Pivot { get; set; }
        public Vector2? SizeDelta { get; set; }
    }
}