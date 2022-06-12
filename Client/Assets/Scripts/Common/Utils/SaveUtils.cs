using System.IO;
using System.Xml;
using System.Xml.Linq;
using Common.Entities;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Common.Utils
{
    public static class SaveUtils
    {
        public static readonly string    SavesPath  = Path.Combine(Application.persistentDataPath, "Saves.xml");
        public static          XDocument SavesDoc;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
            CreateSavesFileIfNotExist();
            ReloadSaves();
        }

        private static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include
        };
        
        public static T GetValue<T>(SaveKey<T> _Key)
        {
            if (_Key.WasSet)
                return _Key.CachedValue;
            string value = GetXElementValue(_Key.Key);
            T result;
            if (string.IsNullOrEmpty(value))
            {
                result = default;
            }
            else
            {
                if (!Application.isEditor)
                    value = Encryption.Cryptography.Instange.Decrypt(value);
                result = JsonConvert.DeserializeObject<T>(value, SerializerSettings);
            }
            _Key.WasSet = true;
            _Key.CachedValue = result;
            return result;
        }
        
        public static void PutValue<T>(SaveKey<T> _Key, T _Value, bool _OnlyCache = false)
        {
            _Key.WasSet = true;
            _Key.CachedValue = _Value;
            if (_OnlyCache)
                return;
            string value = JsonConvert.SerializeObject(_Value, SerializerSettings);
            if (!Application.isEditor)
                value = Encryption.Cryptography.Instange.Encrypt(value);
            PutXElementValue(_Key.Key, value);
        }

        public static void ReloadSaves()
        {
            SavesDoc = XDocument.Load(SavesPath);
        }

        public static void CreateSavesFileIfNotExist()
        {
            if (File.Exists(SavesPath))
            {
                bool needToRecreateFile = false;
                try
                {
                    SavesDoc = XDocument.Load(SavesPath);
                }
                catch (XmlException)
                {
                    needToRecreateFile = true;
                    File.Delete(SavesPath);
                }
                if (!needToRecreateFile)
                    return;
            }
            SavesDoc = new XDocument(
                new XComment("This file contains encrypted game data."),
                new XElement("data"));
            SavesDoc.Save(SavesPath);
        }

        private static string GetXElementValue(string _Key)
        {
            var rootEl = SavesDoc.Element("data");
            return rootEl?.Element(_Key)?.Value;
        }

        private static void PutXElementValue(string _Key, string _Value)
        {
            var rootEl = SavesDoc.Element("data");
            if (rootEl == null)
            {
                Dbg.LogError($"{nameof(PutXElementValue)}: Root element is null");
                return;
            }
            var el = rootEl.Element(_Key);
            if (el == null)
            {
                rootEl.Add(new XElement(_Key, _Value));
                SavesDoc.Save(SavesPath);
                return;
            }
            el.Value = _Value;
            
            SavesDoc.Save(SavesPath, SaveOptions.None);
        }
    }
}