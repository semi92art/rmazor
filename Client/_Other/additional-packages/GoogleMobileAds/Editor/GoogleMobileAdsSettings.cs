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

        internal static GoogleMobileAdsSettings LoadInstance()
        {
            //Read from resources.
            var instance = Resources.Load<GoogleMobileAdsSettings>(MobileAdsSettingsFile);

            //Create instance if null.
            if (instance == null)
            {
                Directory.CreateDirectory(MobileAdsSettingsResDir);
                instance = ScriptableObject.CreateInstance<GoogleMobileAdsSettings>();
                string assetPath = Path.Combine(
                    MobileAdsSettingsResDir,
                    MobileAdsSettingsFile + MobileAdsSettingsFileExtension);
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
            }

            return instance;
        }
        

        [SerializeField]
        private bool delayAppMeasurementInit;

        public string GoogleMobileAdsAndroidAppId => "ca-app-pub-5357184552698168~9131218519";
        public string GoogleMobileAdsIOSAppId     => "ca-app-pub-5357184552698168~8964338187";

        public bool DelayAppMeasurementInit
        {
            get { return delayAppMeasurementInit; }

            set { delayAppMeasurementInit = value; }
        }
    }
}
