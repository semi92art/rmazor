using System;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;

namespace Utils
{
    public static class SaveUtils
    {
        public static object GetValue(SaveKey _Key)
        {
            string value = PlayerPrefs.GetString(_Key.Key, String.Empty);
            if (string.IsNullOrEmpty(value))
                return null;
            return JsonConvert.DeserializeObject(value, _Key.Type);
        }

        public static T GetValue<T>(SaveKey _Key)
        {
            if (typeof(T) != _Key.Type)
                Debug.LogError($"Type mismatch: generic {typeof(T).Name} and SaveKey type {_Key.Type.Name}");

            object result = GetValue(_Key);
            if (!typeof(T).GetTypeInfo().IsClass && result == null)
                return default;
            return (T) GetValue(_Key);
        }
        
        public static void PutValue(SaveKey _Key, object _Value)
        {
            string value = JsonConvert.SerializeObject(_Value);
            PlayerPrefs.SetString(_Key.Key, value);
        }

        public static void PutValue<T>(SaveKey _Key, T _Value)
        {
            if (typeof(T) != _Key.Type)
                Debug.LogError($"Type mismatch: generic {typeof(T).Name} and SaveKey type {_Key.Type.Name}");
            PutValue(_Key, (object)_Value);
        }
    }
}