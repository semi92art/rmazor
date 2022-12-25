#nullable enable
using System;
using Common.Entities;
using mazing.common.Runtime.Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override void WriteJson(
            JsonWriter     _Writer,
            Color          _Value,
            JsonSerializer _Serializer)
        {
            var hexCol = ColorHex.FromUnityToHexColor(_Value);
            string serialized = JsonConvert.SerializeObject(hexCol);
            _Writer.WriteValue(serialized);
        }

        public override Color ReadJson(
            JsonReader     _Reader,
            Type           _ObjectType,
            Color          _ExistingValue,
            bool           _HasExistingValue,
            JsonSerializer _Serializer)
        {
            string serialized = (string) _Reader.Value!;
            var hexCol = JsonConvert.DeserializeObject<ColorHex>(serialized);
            return ColorHex.FromHexToUnityColor(hexCol!);
        }
    }
}