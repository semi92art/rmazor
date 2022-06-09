using System.Text;
using Newtonsoft.Json;
#nullable enable
namespace RMAZOR.Managers
{
    public class RemoteConfigPropertyInfoJsonConverter : JsonConverter<RemoteConfigPropertyInfo>
    {
        public override void WriteJson(
            JsonWriter                _Writer,
            RemoteConfigPropertyInfo? _Value,
            JsonSerializer            _Serializer)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_Value!.Key);
            sb.AppendLine(JsonConvert.SerializeObject(_Value.Type));
            sb.AppendLine(_Value.IsJson.ToString());
            string cachedValueString = _Value.GetCachedValueEntity == null ? "nullEntity" :
                _Value.GetCachedValueEntity.Value == null ? "null" : _Value.GetCachedValueEntity.Value.ToString();
            sb.AppendLine(cachedValueString);
            string serialized = sb.ToString();
            _Writer.WriteValue(serialized);
        }

        public override RemoteConfigPropertyInfo ReadJson(
            JsonReader                _Reader,
            System.Type               _ObjectType,
            RemoteConfigPropertyInfo? _ExistingValue,
            bool                      _HasExistingValue,
            JsonSerializer            _Serializer)
        {
            return JsonConvert.DeserializeObject<RemoteConfigPropertyInfo>((string) _Reader.Value!)!;
        }
    }
}