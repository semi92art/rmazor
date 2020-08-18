using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Clickers
{
    public class ResourcesLoader
    {
        #region private fields

        private XElement m_Ads;
        private XElement m_LocalizedValues;

        #endregion
    
        #region constructor and singleton
    
        private static ResourcesLoader _instance;
        public static ResourcesLoader Instance => _instance ?? new ResourcesLoader();
    
        private ResourcesLoader()
        {
            if (_instance != null)
                return;

            string lang;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                case SystemLanguage.Ukrainian:
                case SystemLanguage.Belarusian:
                    lang = "rus";
                    break;
                case SystemLanguage.Chinese:
                case SystemLanguage.French:
                case SystemLanguage.German:
                case SystemLanguage.Italian:
                case SystemLanguage.Portuguese:
                case SystemLanguage.Spanish:
                    lang = "eng";
                    break;
                default:
                    lang = "eng";
                    break;
            }
        
        
            m_Ads = FromResources("configs\\ads");
            m_LocalizedValues = FromResources($"configs\\{lang}");
            _instance = this;
        }
    
        #endregion
    
        #region API
    
        public string GoogleAdsApplicationId => GetAdsNodeValue("app_id");
        public string GoogleAdsBannerId => GetAdsNodeValue("banner");
        public string GoogleAdsFullscreenId => GetAdsNodeValue("fullscreen");
        public string GoogleAdsRewardId => GetAdsNodeValue("reward");
        public string GoogleAdsNativeAdId => GetAdsNodeValue("native");
        public List<string> GoogleTestDeviceIds => m_Ads.Elements("test_device").Select(el => el.Value).ToList();

        public string GetLocalizedValue(string key)
        {
            return m_LocalizedValues.Elements("key")
                .Where(el => el.Attribute("id").Value == key)
                .Select(el => el.Value)
                .FirstOrDefault();
        }

        #endregion
    
    
        #region private methods
    
        private XElement FromResources(string path)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            using (MemoryStream ms = new MemoryStream(textAsset.bytes))
                return XDocument.Load(ms).Element("data");
        }

        private string GetAdsNodeValue(string type)
        {
            return m_Ads.Elements("ad").Where(el =>
            {
                XAttribute typeAttr = el.Attribute("type");
                return el.Attribute("os")?.Value == GetOsName() && typeAttr != null && typeAttr.Value == type;
            }).Select(el => el.Value).FirstOrDefault();
        }

        private string GetOsName()
        {
#if UNITY_ANDROID || UNITY_EDITOR
        return "android";
#elif UNITY_IOS
        return "ios";
#else
            return string.Empty;
#endif
        }
    
        #endregion
    }
}