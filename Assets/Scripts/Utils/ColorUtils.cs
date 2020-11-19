using PygmyMonkey.ColorPalette;
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

        public static Color GetColorFromPalette(string _PaletteName, string _ColorName)
        {
            var cpd = ColorPaletteData.Singleton;
            int paletteIdx = cpd.getPaletteIndexFromName(_PaletteName);
            return cpd.colorPaletteList[paletteIdx].getColorFromName(_ColorName).color;
        }
    }
}
