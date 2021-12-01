using Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace Utils
{
    public static class SaveUtils
    {
        private const string PassPhrase = "f47759941904a9bf6f89736c4541d850107c9be6ec619e7e65cf80a14ff7e8e4";
        
        public static T GetValue<T>(SaveKey<T> _Key)
        {
            string value = PlayerPrefs.GetString(_Key.Key, string.Empty);
            if (string.IsNullOrEmpty(value))
                return default;
#if !UNITY_EDITOR
            value = StringCipher.Decrypt(value, PassPhrase);
#endif
            var result = JsonConvert.DeserializeObject<T>(value, SerializerSettings);
            return result;
        }
        
        public static void PutValue<T>(SaveKey<T> _Key, T _Value)
        {
            string value = JsonConvert.SerializeObject(_Value, SerializerSettings);
#if !UNITY_EDITOR
            value = StringCipher.Encrypt(value, PassPhrase);
#endif
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