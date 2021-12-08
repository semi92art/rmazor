using System.IO;
using System.Xml.Linq;
using Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace Utils
{
    public static class SaveUtils
    {
        private const           string    PassPhrase = "f47759941904a9bf6f89736c4541d85";
        private static readonly string    SavesPath  = Path.Combine(Application.persistentDataPath, "Saves.xml");
        private static          XDocument _savesDoc;

        static SaveUtils()
        {
            CreateSavesFileIfNotExist();
        }
        
        private static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include
        };
        
        public static T GetValue<T>(SaveKey<T> _Key)
        {
            string value = GetXElementValue(_Key.Key);
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
            PutXElementValue(_Key.Key, value);
        }

        private static void CreateSavesFileIfNotExist()
        {
            if (File.Exists(SavesPath))
            {
                _savesDoc = XDocument.Load(SavesPath);
                return;
            }
            _savesDoc = new XDocument(
                new XComment("This file contains encrypted game data."),
                new XElement("data"));
            _savesDoc.Save(SavesPath);
        }

        private static string GetXElementValue(string _Key)
        {
            var rootEl = _savesDoc.Element("data");
            return rootEl?.Element(_Key)?.Value;
        }

        private static void PutXElementValue(string _Key, string _Value)
        {
            var rootEl = _savesDoc.Element("data");
            if (rootEl == null)
            {
                Dbg.LogError($"{nameof(PutXElementValue)}: Root element is null");
                return;
            }
            var el = rootEl.Element(_Key);
            if (el == null)
            {
                rootEl.Add(new XElement(_Key, _Value));
                _savesDoc.Save(SavesPath);
                return;
            }
            el.Value = _Value;
            
            _savesDoc.Save(SavesPath, SaveOptions.None);
        }
    }
}