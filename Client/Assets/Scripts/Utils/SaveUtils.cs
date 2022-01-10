﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Entities;
using Newtonsoft.Json;
using UnityEngine;
using Utils.Encryption;

namespace Utils
{
    public static class SaveUtils
    {
        public static readonly string    SavesPath  = Path.Combine(Application.persistentDataPath, "Saves.xml");
        public static          XDocument SavesDoc;
        // private const          string    PassPhrase = "f47759941904a9bf6f89736c4541d85";
        private const          string    PassPhrase = "---";
        private static readonly Cryptography Crypto = new Cryptography(PassPhrase);

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
            if (!_Key.IsDirty)
                return _Key.CachedValue;
            string value = GetXElementValue(_Key.Key);
            if (string.IsNullOrEmpty(value))
                return default;
            value = Crypto.Decrypt(value);
// #if !UNITY_EDITOR
//             value = StringCipher.Decrypt(value, PassPhrase);
// #endif
            T result = JsonConvert.DeserializeObject<T>(value, SerializerSettings);
            _Key.CachedValue = result;
            return result;
        }
        
        public static void PutValue<T>(SaveKey<T> _Key, T _Value, bool _OnlyCache = false)
        {
            if (_Key.IsDirty)
            {
                _Key.IsDirty = false;
                _Key.CachedValue = _Value;
            }
            if (_OnlyCache)
                return;
            string value = JsonConvert.SerializeObject(_Value, SerializerSettings);
            value = Crypto.Encrypt(value);
// #if !UNITY_EDITOR
//             value = StringCipher.Encrypt(value, PassPhrase);
// #endif
            PutXElementValue(_Key.Key, value);
        }

        public static void ReloadSaves()
        {
            SavesDoc = XDocument.Load(SavesPath);
        }

        private static void CreateSavesFileIfNotExist()
        {
            if (File.Exists(SavesPath))
            {
                SavesDoc = XDocument.Load(SavesPath);
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