using UnityEngine;
using Newtonsoft.Json;

namespace Utils
{
    public static class SaverUtils
    {
        public static string GetValue(string _Key)
        {
            return PlayerPrefs.GetString(_Key);
        }

        public static T GetValue<T>(string _Key)
        {
            return JsonConvert.DeserializeObject<T>(GetValue(_Key));
        }

        public static void PutValue(string _Key, string _Value)
        {
            PlayerPrefs.SetString(_Key, _Value);
        }

        public static void PutValue(string _Key, object _Value)
        {
            PutValue(_Key, JsonConvert.SerializeObject(_Value));
        }
    }
}