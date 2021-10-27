using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DI.Extensions;
using ScriptableObjects;
using UI.Entities;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public static class ResLoader
    {
        #region nonpublic members

        private static readonly XElement Ads;

        #endregion
    
        #region static constructor

        static ResLoader()
        {
            Ads = FromResources(@"configs\ads");
        }
    
        #endregion
    
        #region API
    
        public static string GoogleAdsApplicationId => GetAdsNodeValue("app_id");
        public static string GoogleAdsBannerId => GetAdsNodeValue("banner");
        public static string GoogleAdsFullscreenId => GetAdsNodeValue("fullscreen");
        public static string GoogleAdsRewardId => GetAdsNodeValue("reward");
        public static string GoogleAdsNativeAdId => GetAdsNodeValue("native");
        public static List<string> GoogleTestDeviceIds => Ads.Elements("test_device").Select(_El => _El.Value).ToList();

        public static PrefabSetScriptableObject GetPrefabSet(string _PrefabSetName)
        {
            var set = Resources.Load<PrefabSetScriptableObject>($"prefab_sets/{_PrefabSetName}");
            if (set == null)
                Dbg.LogError($"Prefab set with name {_PrefabSetName} does not exist");
            return set;
        }

        public static bool PrefabSetExist(string _PrefabSetName)
        {
            var set = Resources.Load<PrefabSetScriptableObject>($"prefab_sets/{_PrefabSetName}");
            return set != null;
        }

#if UNITY_EDITOR
        public static PrefabSetScriptableObject CreatePrefabSetIfNotExist(string _PrefabSetName)
        {
            if (PrefabSetExist(_PrefabSetName))
                return GetPrefabSet(_PrefabSetName);
            var set = ScriptableObject.CreateInstance<PrefabSetScriptableObject>();
            AssetDatabase.CreateAsset(set, $"Assets/Resources/prefab_sets/{_PrefabSetName}.asset");
            AssetDatabase.SaveAssets();
            return set;
        }
#endif

        public static TextAsset GetText(string _Path)
        {
            return Resources.Load<TextAsset>(_Path);
        }

        #endregion
        
        #region nonpublic methods
    
        private static XElement FromResources(string _Path)
        {
            var textAsset = Resources.Load<TextAsset>(_Path);
            using (var ms = new MemoryStream(textAsset.bytes))
                return XDocument.Load(ms).Element("data");
        }

        private static string GetAdsNodeValue(string _Type)
        {
            Func<string, string, bool> compareOsNames = (_S1, _S2) => _S1.EqualsIgnoreCase(_S2);
            return Ads.Elements("ad")
                .First(_El => _El.Attribute("os").Value.EqualsIgnoreCase(CommonUtils.GetOsName())
                && _El.Attribute("type")?.Value == _Type).Value;
        }
        
        #endregion
    }
}