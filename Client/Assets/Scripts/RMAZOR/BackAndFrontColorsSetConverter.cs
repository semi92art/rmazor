#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Newtonsoft.Json;

namespace RMAZOR
{
    public class BackAndFrontColorsSetConverter : JsonConverter<IList<BackAndFrontColorsSetItem>>
    {
        #region types
        
        [Serializable]
        private class BackAndFrontColorsHexSetItem
        {
            [JsonProperty("M")]  public ColorHex? Main        { get; set; }
            [JsonProperty("B1")] public ColorHex? Background1 { get; set; }
            [JsonProperty("B2")] public ColorHex? Background2 { get; set; }
        }

        #endregion

        #region api

        public override bool CanRead => true;

        public override void WriteJson(
            JsonWriter                        _Writer,
            IList<BackAndFrontColorsSetItem>? _Value,
            JsonSerializer                    _Serializer)
        {
            IList<BackAndFrontColorsHexSetItem> hexSet = _Value!.Select(_Item => new BackAndFrontColorsHexSetItem
            {
                Main        = ColorHex.FromUnityToHexColor(_Item.main),
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
            string hexSetRaw = _Reader.ReadAsString()!;
            var hexSet = JsonConvert.DeserializeObject<IList<BackAndFrontColorsHexSetItem>>(hexSetRaw);
            IList<BackAndFrontColorsSetItem> set = hexSet.Select(_Item => new BackAndFrontColorsSetItem
            {
                main       = ColorHex.FromHexToUnityColor(_Item.Main!),
                bacground1 = ColorHex.FromHexToUnityColor(_Item.Background1!),
                bacground2 = ColorHex.FromHexToUnityColor(_Item.Background2!)
            }).ToList();
            return set;
        }

        #endregion
    }
}