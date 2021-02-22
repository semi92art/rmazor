using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Extensions;
using UI.Entities;
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

        public static UIStyleObject GetStyle(string _StyleName)
        {
            var style = Resources.Load<UIStyleObject>($"styles/{_StyleName}");
            if (style == null)
                Debug.LogError($"Style with name {_StyleName} does not exist");
            return style;
        }

        public static bool StyleExist(string _StyleName)
        {
            var style = Resources.Load<UIStyleObject>($"styles/{_StyleName}");
            return style != null;
        }

#if UNITY_EDITOR
        public static UIStyleObject CreateStyleIfNotExist(string _StyleName)
        {
            if (StyleExist(_StyleName))
                return GetStyle(_StyleName);
            var style = ScriptableObject.CreateInstance<UIStyleObject>();
            UnityEditor.AssetDatabase.CreateAsset(style, $"Assets/Resources/styles/{_StyleName}.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            return style;
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
            TextAsset textAsset = Resources.Load<TextAsset>(_Path);
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