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
    public class ResLoader
    {
        #region private fields

        private static XElement m_Ads;

        #endregion
    
        #region static constructor

        static ResLoader()
        {
            m_Ads = FromResources(@"configs\ads");
        }
    
        #endregion
    
        #region API
    
        public static string GoogleAdsApplicationId => GetAdsNodeValue("app_id");
        public static string GoogleAdsBannerId => GetAdsNodeValue("banner");
        public static string GoogleAdsFullscreenId => GetAdsNodeValue("fullscreen");
        public static string GoogleAdsRewardId => GetAdsNodeValue("reward");
        public static string GoogleAdsNativeAdId => GetAdsNodeValue("native");
        public static List<string> GoogleTestDeviceIds => m_Ads.Elements("test_device").Select(_El => _El.Value).ToList();

        public static UIStyleObject GetStyle(string _StyleName)
        {
            return Resources.Load<UIStyleObject>($"styles/{_StyleName}");
        }

        public static TextAsset GetText(string _Path)
        {
            return Resources.Load<TextAsset>(_Path);
        }

        #endregion
        
        #region private methods
    
        private static XElement FromResources(string _Path)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(_Path);
            using (var ms = new MemoryStream(textAsset.bytes))
                return XDocument.Load(ms).Element("data");
        }

        private static string GetAdsNodeValue(string _Type)
        {
            Func<string, string, bool> compareOsNames = (_S1, _S2) => _S1.EqualsIgnoreCase(_S2);
            return m_Ads.Elements("ad")
                .First(_El => _El.Attribute("os").Value.EqualsIgnoreCase(CommonUtils.GetOsName())
                && _El.Attribute("type")?.Value == _Type).Value;
        }
        
        #endregion
    }
}