using UnityEngine;

namespace Utils
{
    public static class ColorUtils
    {
        public static Color Empty => new Color(0, 0, 0, 0);

        public static Color CreateColor(int _R, int _G, int _B, int _A)
        {
            return new Color(_R / 255.0f, _G / 255.0f, _B / 255.0f, _A / 255.0f);
        }

        public static Color CreateColor(int _R, int _G, int _B)
        {
            return new Color(_R / 255.0f, _G / 255.0f, _B / 255.0f, 1);
        }
    }
}
