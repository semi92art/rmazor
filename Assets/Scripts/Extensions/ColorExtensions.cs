using UnityEngine;

namespace Extensions
{
    public static class ColorExtensions
    {
        public static Color SetAlpha(this Color _Color, float _Alpha)
        {
            return new Color(_Color.r, _Color.g, _Color.b, _Alpha);
        }
    }
}