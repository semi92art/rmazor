using System;
using UnityEngine;

#nullable enable
namespace Common.Entities
{
    [Serializable]
    public class ColorHex
    {
        public string? Hex { get; set; }

        public static ColorHex FromUnityToHexColor(Color _Color)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(_Color);
            var col = new ColorHex {Hex = hex};
            return col;
        }

        public static Color FromHexToUnityColor(ColorHex _ColorHex)
        {
            ColorUtility.TryParseHtmlString("#" + _ColorHex.Hex, out Color color);
            return color;
        }
    }
}