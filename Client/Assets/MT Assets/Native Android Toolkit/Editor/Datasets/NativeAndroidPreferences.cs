using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This script is the Dataset of the scriptable object "Preferences". This script saves Native Android Toolkit preferences.
     */

    public class NativeAndroidPreferences : ScriptableObject
    {
        public enum ModifyManifest
        {
            No,
            YesCreateNewIfNotExists
        }
        public enum MinSdkVersion
        {
            AndroidJellyBeanApi16
        }
        public enum TargetSdkVersion
        {
            AndroidOreoApi26,
            AndroidOreoApi27,
            AndroidPieApi28
        }

        public string projectName;
        public bool natInstalledInFirstScene;

        public ModifyManifest modifyAndroidManifest = ModifyManifest.No;
        public bool declareUnityPlayerActivity = true;
        public bool unityPlayerActivityIsDefault = true;
        public bool requestPermissionsOnOpen = true;

        public Texture2D iconOfNotifications = null;
        public bool iconIsValid = false;
        public bool filePermission = true;
        public bool cameraPermission = true;
        public bool accessWifiStatePermission = true;
        public bool vibratePermission = true;
        public bool networkStatePermission = true;
        public bool bluetoothPermission = true;
        public bool internetPermission = true;
        public bool modifyAudioPermission = true;
        public bool keepDeviceEnabled = true;
        public bool enableAccurateNotify = true;
        public bool warningOnChangeDate = true;
        public string titleOfWarningOnChangeDate = "Time Changed";
        public string textOfWarningOnChangeDate = "Time has changed on your device. This may affect the way time goes by in this app, to keep everything running, please open the app now.";
        public MinSdkVersion minApiLevel = MinSdkVersion.AndroidJellyBeanApi16;
        public TargetSdkVersion targetSdkVersion = TargetSdkVersion.AndroidOreoApi26;

        public bool launchLogs = true;
    }
}