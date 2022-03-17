#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Common.Entities
{
    public class MainColorItemsSetConverter : JsonConverter<IList<MainColorsSetItem>>
    {
        #region types

        [Serializable]
        private class ColorsHexSetItem
        {
            [JsonProperty("N")] public string?   Name  { get; set; }
            [JsonProperty("C")] public ColorHex? Color { get; set; }
        }

        #endregion
        
        #region api

        public override bool CanRead => true;

        public override void WriteJson(
            JsonWriter                _Writer,
            IList<MainColorsSetItem>? _Value,
            JsonSerializer            _Serializer)
        {
            IList<ColorsHexSetItem> hexSet = _Value!.Select(_Item => new ColorsHexSetItem
            {
                Color = ColorHex.FromUnityToHexColor(_Item.color),
                Name  = _Item.name
            }).ToList();
            string serialized = JsonConvert.SerializeObject(hexSet);
            _Writer.WriteValue(serialized);
        }

        public override IList<MainColorsSetItem> ReadJson(
            JsonReader                _Reader,
            Type                      _ObjectType,
            IList<MainColorsSetItem>? _ExistingValue,
            bool                      _HasExistingValue,
            JsonSerializer            _Serializer)
        {
            string hexSetRaw = (string)_Reader.Value!;
            var hexSet = JsonConvert.DeserializeObject<IList<ColorsHexSetItem>>(hexSetRaw);
            IList<MainColorsSetItem> set = hexSet.Select(_Item => new MainColorsSetItem
            {
                color = ColorHex.FromHexToUnityColor(_Item.Color!),
                name  = _Item.Name!
            }).ToList();
            return set;
        }

        #endregion
    }
}