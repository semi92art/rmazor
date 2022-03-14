#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR
{
    public class BackAndFrontColorsSetConverter : JsonConverter<IList<BackAndFrontColorsSetItem>>
    {
        [Serializable]
        private class ColorHex
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
                Dbg.Log(_ColorHex.Hex);
                ColorUtility.TryParseHtmlString("#" + _ColorHex.Hex, out Color color);
                return color;
            }
        }
        
        [Serializable]
        private class BackAndFrontColorsHexSetItem
        {
            public ColorHex? Main        { get; set; }
            public ColorHex? Background1 { get; set; }
            public ColorHex? Background2 { get; set; }
        }

        public override void WriteJson(
            JsonWriter _Writer,
            IList<BackAndFrontColorsSetItem>? _Value,
            JsonSerializer _Serializer)
        {
            IList<BackAndFrontColorsHexSetItem> hexSet = _Value!.Select(_Item => new BackAndFrontColorsHexSetItem
            {
                Main = ColorHex.FromUnityToHexColor(_Item.main),
                Background1 = ColorHex.FromUnityToHexColor(_Item.bacground1),
                Background2 = ColorHex.FromUnityToHexColor(_Item.bacground2)
            }).ToList();
            string serialized = JsonConvert.SerializeObject(hexSet);
            _Writer.WriteValue(serialized);
        }

        public override IList<BackAndFrontColorsSetItem> ReadJson(
            JsonReader                        _Reader,          
            Type                              _ObjectType, 
            IList<BackAndFrontColorsSetItem>? _ExistingValue,
            bool                              _HasExistingValue,
            JsonSerializer                    _Serializer)
        {
            string hexSetRaw = (string)_Reader.Value!;
            var hexSet = JsonConvert.DeserializeObject<IList<BackAndFrontColorsHexSetItem>>(hexSetRaw);
            IList<BackAndFrontColorsSetItem> set = hexSet.Select(_Item => new BackAndFrontColorsSetItem
            {
                main = ColorHex.FromHexToUnityColor(_Item.Main!),
                bacground1 = ColorHex.FromHexToUnityColor(_Item.Background1!),
                bacground2 = ColorHex.FromHexToUnityColor(_Item.Background2!)
            }).ToList();
            return set;
        }

        public override bool CanRead => true;
    }
}