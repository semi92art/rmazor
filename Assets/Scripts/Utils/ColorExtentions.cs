﻿using UnityEngine;

namespace Utils
{
    public static class ColorExtentions
    {
        public static Color SetAlpha(this Color _Color, float _Alpha)
        {
            return new Color(_Color.r, _Color.g, _Color.b, _Alpha);
        }
    }
}