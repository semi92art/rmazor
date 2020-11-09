using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UI;
using UI.Entities;
using UnityEngine;

namespace Utils
{
    public class ResLoader
    {
        #region private fields

        private static XElement m_Ads;
        private static XElement m_LocalizedValues;

        #endregion
    
        #region static constructor

        static ResLoader()
        {
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
        }
    
        #endregion
    
        #region API
    
        public static string GoogleAdsApplicationId => GetAdsNodeValue("app_id");
        public static string GoogleAdsBannerId => GetAdsNodeValue("banner");
        public static string GoogleAdsFullscreenId => GetAdsNodeValue("fullscreen");
        public static string GoogleAdsRewardId => GetAdsNodeValue("reward");
        public static string GoogleAdsNativeAdId => GetAdsNodeValue("native");
        public static List<string> GoogleTestDeviceIds => m_Ads.Elements("test_device").Select(el => el.Value).ToList();

        public static string GetLocalizedValue(string key)
        {
            return m_LocalizedValues.Elements("key")
                .Where(el => el.Attribute("id").Value == key)
                .Select(el => el.Value)
                .FirstOrDefault();
        }

        public static UIStyleObject GetStyle(string _StyleName)
        {
            return Resources.Load<UIStyleObject>($"styles/{_StyleName}");
        }

        public static Sprite GetCounry(string _Key)
        {
            return Resources.Load<Sprite>($"countries/{_Key}");
        }

        #endregion
        
        #region private methods
    
        private static XElement FromResources(string path)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            using (MemoryStream ms = new MemoryStream(textAsset.bytes))
                return XDocument.Load(ms).Element("data");
        }

        private static string GetAdsNodeValue(string type)
        {
            return m_Ads.Elements("ad").Where(el =>
            {
                XAttribute typeAttr = el.Attribute("type");
                return el.Attribute("os")?.Value == GetOsName() && typeAttr != null && typeAttr.Value == type;
            }).Select(el => el.Value).FirstOrDefault();
        }

        private static string GetOsName()
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