using System.IO;
using UnityEditor;
using UnityEngine;

namespace GoogleMobileAds.Editor
{

    internal class GoogleMobileAdsSettings : ScriptableObject
    {
        private const string MobileAdsSettingsResDir = "Assets/GoogleMobileAds/Resources";

        private const string MobileAdsSettingsFile = "GoogleMobileAdsSettings";

        private const string MobileAdsSettingsFileExtension = ".asset";

        private static GoogleMobileAdsSettings instance;

        [SerializeField]
        private string adMobAndroidAppId = string.Empty;

        [SerializeField]
        private string adMobIOSAppId = string.Empty;

        [SerializeField]
        private bool delayAppMeasurementInit = false;

        //FIXME тупой плагин обнуляет айдишники после каждого билда
        public string GoogleMobileAdsAndroidAppId
        {
            get { return "ca-app-pub-5357184552698168~9131218519"; }

            set { Instance.adMobAndroidAppId = value; }
        }

        //FIXME тупой плагин обнуляет айдишники после каждого билда
        public string GoogleMobileAdsIOSAppId
        {
            get { return "ca-app-pub-5357184552698168~8964338187"; }

            set { Instance.adMobIOSAppId = value; }
        }

        public bool DelayAppMeasurementInit
        {
            get { return Instance.delayAppMeasurementInit; }

            set { Instance.delayAppMeasurementInit = value; }
        }

        public static GoogleMobileAdsSettings Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = Resources.Load<GoogleMobileAdsSettings>(MobileAdsSettingsFile);

                if(instance != null)
                {
                    return instance;
                }

                Directory.CreateDirectory(MobileAdsSettingsResDir);

                instance = ScriptableObject.CreateInstance<GoogleMobileAdsSettings>();

                string assetPath = Path.Combine(MobileAdsSettingsResDir, MobileAdsSettingsFile);
                string assetPathWithExtension = Path.ChangeExtension(
                                                        assetPath, MobileAdsSettingsFileExtension);
                AssetDatabase.CreateAsset(instance, assetPathWithExtension);

                AssetDatabase.SaveAssets();

                return instance;
            }
        }
    }
}
