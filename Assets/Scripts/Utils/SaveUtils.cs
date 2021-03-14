using System;
using Entities;
using UnityEngine;
using Newtonsoft.Json;

namespace Utils
{
    public static class SaveUtils
    {
        public static T GetValue<T>(SaveKey _Key)
        {
            if (typeof(T) != _Key.Type)
                Dbg.LogError($"Type mismatch: generic {typeof(T).Name} and SaveKey type {_Key.Type.Name}");
            string value = PlayerPrefs.GetString(_Key.Key, string.Empty);
            if (string.IsNullOrEmpty(value))
                return default;
            var result = JsonConvert.DeserializeObject<T>(value, SerializerSettings);
            return result;
        }
        
        public static void PutValue<T>(SaveKey _Key, T _Value)
        {
            if (typeof(T) != _Key.Type)
                Dbg.LogError($"Type mismatch: generic {typeof(T).Name} and SaveKey type {_Key.Type.Name}");

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