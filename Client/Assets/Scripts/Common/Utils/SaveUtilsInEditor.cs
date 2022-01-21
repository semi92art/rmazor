using Common.Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Utils
{
    public static class SaveUtilsInEditor
    {
        public static T GetValue<T>(SaveKey<T> _Key)
        {
            string value = PlayerPrefs.GetString(_Key.Key, string.Empty);
            if (string.IsNullOrEmpty(value))
                return default;

            var result = JsonConvert.DeserializeObject<T>(value, SerializerSettings);
            return result;
        }
        
        public static void PutValue<T>(SaveKey<T> _Key, T _Value)
        {
            string value = JsonConvert.SerializeObject(_Value, SerializerSettings);

            PlayerPrefs.SetString(_Key.Key, value);
            PlayerPrefs.Save();
        }

        private static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include
        };
    }
}