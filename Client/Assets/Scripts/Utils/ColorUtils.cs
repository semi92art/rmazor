using PygmyMonkey.ColorPalette;
using UnityEngine;

namespace Utils
{
    public static class ColorUtils
    {
        #region api
        
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

        public static Color GetColorFromCurrentPalette(string _ColorName)
        {
            return GetCurrentPalette().getColorFromName(_ColorName).color;
        }
        
        public static Color MixColors(Color _A, Color _B)
        {
            return (_A * _A.a + _B * _B.a * (1f - _A.a)) / (_A.a + _A.b * (1f - _A.a));
        }
        
        #endregion
        
        #region nonpublic methods

        private static ColorPalette GetCurrentPalette()
        {
            string paletteName = $"Game {GameClientUtils.GameId}";
            var cpd = ColorPaletteData.Singleton;
            int paletteIdx = cpd.getPaletteIndexFromName(paletteName);
            return cpd.colorPaletteList[paletteIdx];
        }
        
        #endregion
    }
}
