using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This class is responsible for detecting if NAT Core.aar was modified, and request the apply of preferences for this
     */

    //Only works if Ionic.Zip is available in this project, to apply preferences on zip NAT Core.aar
#if MTAssets_IonicZip_Available

    [InitializeOnLoad]
    public class CoreLibPrefs
    {
        static CoreLibPrefs()
        {
            //Run the script after Unity compiles
            EditorApplication.delayCall += DetectIfNatCoreAarWasModified;
        }

        public static void DetectIfNatCoreAarWasModified()
        {
            //Create the directory
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets"))
                AssetDatabase.CreateFolder("Assets", "MT Assets");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Editor"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Editor");

            //Get modify date of Nat Core.aar
            string lastModifyDate = File.GetLastWriteTimeUtc("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Android/NAT Core.aar").ToString();

            //If the file not exists, create then with current modified date to ignore the first time on inport
            if (AssetDatabase.LoadAssetAtPath("Assets/MT Assets/_AssetsData/Editor/NatCoreLastWrite.ini", typeof(object)) == null)
                File.WriteAllText("Assets/MT Assets/_AssetsData/Editor/NatCoreLastWrite.ini", lastModifyDate);

            //Get last modify date done by preferences window
            string lastModifyDateByPreferences = File.ReadAllText("Assets/MT Assets/_AssetsData/Editor/NatCoreLastWrite.ini");

            //Verify if modyfied date is different from the last modyfied date by preferences window
            if (lastModifyDate != lastModifyDateByPreferences)
            {
                if (EditorUtility.DisplayDialog("Native Android Toolkit", "The Native Android Toolkit has detected that the native code library has been updated or modified externally, so it is necessary for your preferences to be applied again for everything to work. Do you want to apply now?", "Apply", "Ignore") == true)
                {
                    Preferences.OpenWindow(true);
                }
            }
        }
    }
#endif
}