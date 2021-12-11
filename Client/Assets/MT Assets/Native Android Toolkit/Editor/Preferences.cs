using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    public class Preferences : EditorWindow
    {
        //Variables of preferences
        public static NativeAndroidPreferences natPreferences;

        //Private variables
        private Vector2 scrollPos;
        public static bool savePreferencesNowOnOpen = false;
        public int numberOfUpdates = 0;

        //Preferences code

        static void LoadThePreferences()
        {
            //Create the default directory, if not exists
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Preferences"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Preferences");

            //Try to load the preferences file
            natPreferences = (NativeAndroidPreferences)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset", typeof(NativeAndroidPreferences));
            //Validate the preference file. if this preference file is of another project, delete then 
            if (natPreferences != null)
            {
                if (natPreferences.projectName != Application.productName)
                {
                    AssetDatabase.DeleteAsset("Assets/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset");
                    natPreferences = null;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            //If null, create and save a preferences file 
            if (natPreferences == null)
            {
                natPreferences = ScriptableObject.CreateInstance<NativeAndroidPreferences>();
                natPreferences.projectName = Application.productName;
                AssetDatabase.CreateAsset(natPreferences, "Assets/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        static void SaveThePreferences()
        {
            //Save the preferences in Prefs.asset
            natPreferences.projectName = Application.productName;

            EditorUtility.SetDirty(natPreferences);
            AssetDatabase.SaveAssets();
        }

        //Public utils code

        public static bool isLaunchLogsEnabled()
        {
            //Load the preferences and return if "LaunchLogs" option is enabled
            LoadThePreferences();
            return natPreferences.launchLogs;
        }

        //Setup window code

        public static void OpenWindow(bool savePreferencesOnOpen)
        {
            //Method to open the Window
            var window = GetWindow<Preferences>("NAT Prefs");
            window.minSize = new Vector2(400, 600);
            window.maxSize = new Vector2(400, 600);
            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.Show();

            //Get parameters
            savePreferencesNowOnOpen = savePreferencesOnOpen;
        }

        public Rect GetInspectorWindowSize()
        {
            //Returns the current size of inspector window
            return EditorGUILayout.GetControlRect(true, 0f);
        }

        void OnGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            EditorGUI.BeginChangeCheck();

            //Load the preferences, if is null
            if (natPreferences == null)
            {
                LoadThePreferences();
            }
            if (natPreferences.iconOfNotifications == null)
            {
                natPreferences.iconOfNotifications = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Notify/DefaultIcon.png", typeof(Texture2D));
            }
            if (AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Android/NAT Core.aar", typeof(object)) == null)
            {
                EditorGUILayout.HelpBox("Could not find \"NAT Core.aar\", please reinstall Native Android Toolkit to correct this problem.", MessageType.Error);
                return;
            }

            var labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            EditorGUILayout.HelpBox("Here you can change preferences for how Native Android Toolkit works in the editor, and on the Android system your app will run on. After changing your preferences, click the \"Save Preferences\" button to apply the changes.", MessageType.Info);

            GUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(Screen.width), GUILayout.Height(490));
            EditorGUILayout.BeginVertical("box");

            //Android Manifest
            EditorGUILayout.LabelField("Android Manifest", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            natPreferences.modifyAndroidManifest = (NativeAndroidPreferences.ModifyManifest)EditorGUILayout.EnumPopup(new GUIContent("Modify APK Manifest",
                                "Enable this option if you want Native Android Toolkit to make changes to your AndroidManifest.xml."),
                                natPreferences.modifyAndroidManifest);
            if (natPreferences.modifyAndroidManifest == NativeAndroidPreferences.ModifyManifest.YesCreateNewIfNotExists)
            {
                //If not found a AndroidManifest.xml file
                if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(object)) == null)
                {
                    EditorGUILayout.HelpBox("The file \"AndroidManifest.xml\" could not be found. A new file will be created, containing only the options you choose below. This will not change the way your game works.", MessageType.Info);
                }
                //If not found a AndroidManifest.xml file
                if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(object)) != null)
                {
                    EditorGUILayout.HelpBox("The file \"AndroidManifest.xml\" was found. It will only be modified in the options you choose below. The other parameters will remain intact.", MessageType.Info);
                }

                natPreferences.declareUnityPlayerActivity = EditorGUILayout.Toggle(new GUIContent("Declare UnityActivity",
                    "If you enable this option, Native Android Toolkit will declare UnityPlayer activity in the manifest so that the Android system can access this default activity from your app. If you do not enable this option, Unity activity will not be declared in the manifest and the Android system will not be able to access it. The default setting is to leave this option enabled. If you disable this option, the default Unity activity (which your app will run on) will not be accessible by the Android system, and if it is the Main activity, your app icon will no longer appear to the user."),
                    natPreferences.declareUnityPlayerActivity);
                if (natPreferences.declareUnityPlayerActivity == true)
                {
                    EditorGUI.indentLevel += 1;
                    natPreferences.unityPlayerActivityIsDefault = EditorGUILayout.Toggle(new GUIContent("Is Main Activity",
                    "If you enable this option, UnityPlayerActivity will be set as the Main activity and whenever the user touches your app icon, the UnityPlayer activity will open. This is the default setting for Unity Engine-generated APKs. If you need to change the activity that will be performed when the user clicks on your app icon, disable this option. If your app does not have a Main activity, your app will not display an icon on the Android system even if it is installed."),
                    natPreferences.unityPlayerActivityIsDefault);
                    EditorGUI.indentLevel -= 1;
                }

                natPreferences.requestPermissionsOnOpen = EditorGUILayout.Toggle(new GUIContent("Req. Perm. On Open",
                    "Enable this option to be prompted for all permissions your app needs as soon as it opens.\n\nBy default, this option is enabled because Unity by default requests all permissions as soon as your game is opened.\n\nDisable this option and Unity will no longer request permissions when your app opens. Permissions will only be requested when your app requests it from the Native Android Toolkit C# API."),
                    natPreferences.requestPermissionsOnOpen);

                if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(object)) != null)
                {
                    //Butons to manager the AndroidManifest.xml
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("The path to AndroidManifest.xml is\nAssets/Plugins/Android/AndroidManifest.xml", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Open Manifest", GUILayout.Width(160), GUILayout.Height(16)))
                    {
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(TextAsset)));
                    }
                    GUILayout.Space(GetInspectorWindowSize().x - 340);
                    if (GUILayout.Button("Delete Manifest", GUILayout.Width(160), GUILayout.Height(16)))
                    {
                        bool dialog = EditorUtility.DisplayDialog("Delete Android Manifest?", "Deleting AndroidManifest will cause Unity to use the default AndroidManifest it generates. Continue?", "Yes", "Cancel");
                        if (dialog == true)
                        {
                            AssetDatabase.DeleteAsset("Assets/Plugins/Android/AndroidManifest.xml");
                            natPreferences.modifyAndroidManifest = NativeAndroidPreferences.ModifyManifest.No;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            //NAT Core settings
            GUILayout.Space(10);
            EditorGUILayout.LabelField("NAT Core Settings", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            natPreferences.iconOfNotifications = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Icon of Notifications",
                        "The icon that will represent the notifications released by your app in the device status bar."),
                        natPreferences.iconOfNotifications, typeof(Texture2D), true, GUILayout.Height(16));
            if (natPreferences.iconOfNotifications != null)
            {
                bool isResValid = true;
                bool isExtValid = true;
                //Verify resolution
                if (natPreferences.iconOfNotifications.width != 64 || natPreferences.iconOfNotifications.height != 64)
                {
                    EditorGUILayout.HelpBox("Please provide a transparent icon of exactly 64x64 pixels resolution to ensure the best image display in the status bar.", MessageType.Error);
                    isResValid = false;
                }
                //Verify extension
                string extension = Path.GetExtension(AssetDatabase.GetAssetPath(natPreferences.iconOfNotifications)).Replace(".", "");
                if (extension != "png")
                {
                    isExtValid = false;
                }
                natPreferences.iconIsValid = (isResValid == true && isExtValid == true) ? true : false;
            }

            if (natPreferences.cameraPermission == false || natPreferences.filePermission == false || natPreferences.accessWifiStatePermission == false || natPreferences.vibratePermission == false || natPreferences.networkStatePermission == false
                || natPreferences.bluetoothPermission == false || natPreferences.internetPermission == false || natPreferences.modifyAudioPermission == false || natPreferences.keepDeviceEnabled == false)
            {
                EditorGUILayout.HelpBox("Disabling a permission will cause some Native Android Toolkit API methods to stop working. For example, disabling Camera permission will cause flashlight functions to stop working.", MessageType.Warning);
            }

            natPreferences.cameraPermission = EditorGUILayout.Toggle(new GUIContent("Camera Permission",
                        "Enable this option if you want to add camera access permission in your app. This permission allows flash on and off functions to work."),
                        natPreferences.cameraPermission);

            natPreferences.filePermission = EditorGUILayout.Toggle(new GUIContent("Files Permission",
                        "Enable this option if you want to add file management permission in your app. This permission allows Native Android Toolkit file management to work."),
                        natPreferences.filePermission);

            natPreferences.accessWifiStatePermission = EditorGUILayout.Toggle(new GUIContent("Wifi State Permission",
                        "Enable this option if you want Native Android Toolkit to access Wifi information. This permission allows Native Android Toolkit to perform functions with wifi, such as checking the network name, whether the device is connected to a network, and so on."),
                        natPreferences.accessWifiStatePermission);

            natPreferences.vibratePermission = EditorGUILayout.Toggle(new GUIContent("Vibrate Permission",
                        "Enable this permission if you want NAT to access device vibration. This permission will allow NAT to vibrate the device with a pattern or for a specified time."),
                        natPreferences.vibratePermission);

            natPreferences.networkStatePermission = EditorGUILayout.Toggle(new GUIContent("Net. State Permission",
                        "Enable this option if you want NAT to access network information. With this permission, NAT can observe if there is internet connectivity among other things."),
                        natPreferences.networkStatePermission);

            natPreferences.bluetoothPermission = EditorGUILayout.Toggle(new GUIContent("Bluetooth Permission",
                        "Enable this option if you want NAT to have access to Bluetooth. With this permission NAT can check for Bluetooth connectivity and so on."),
                        natPreferences.bluetoothPermission);

            natPreferences.internetPermission = EditorGUILayout.Toggle(new GUIContent("Internet Permission",
                        "Enable this option to allow NAT to access the Internet. With this option, NAT can access the Internet, so it can do things like get the current network time through NTP."),
                        natPreferences.internetPermission);

            natPreferences.modifyAudioPermission = EditorGUILayout.Toggle(new GUIContent("Audio Permission",
                        "Enable this option if you want NAT to be able to view/modify device audio settings. With this option, NAT can, for example, see if there are any headphones connected to the phone."),
                        natPreferences.modifyAudioPermission);

            natPreferences.keepDeviceEnabled = EditorGUILayout.Toggle(new GUIContent("Wake Up Permission",
                       "The Native Android Toolkit can use the Wake Lock permission of the Android system, to prevent the device from disabling some features of the native NAT code. For example, with this permission enabled, it is possible to deliver more precisely scheduled notifications for example, but this can sacrifice a little of the user's battery."),
                       natPreferences.keepDeviceEnabled);
            if (natPreferences.keepDeviceEnabled == true)
            {
                EditorGUI.indentLevel += 1;
                natPreferences.enableAccurateNotify = EditorGUILayout.Toggle(new GUIContent("Accurate Notify",
                       "Enable this option to enable the precision mechanism for scheduled notifications in the Native Android Toolkit. If this option is enabled, the native NAT code will use a series of checks to ensure that your inactivity notifications and scheduled notifications are delivered as close to the expected time as possible, even if the device has a battery saving system. Note that if the battery saving method is too aggressive, notifications may not be delivered. Consult the documentation for more details."),
                       natPreferences.enableAccurateNotify);
                EditorGUI.indentLevel -= 1;
            }

            natPreferences.warningOnChangeDate = EditorGUILayout.Toggle(new GUIContent("Warn On Change Time",
                       "If this option is enabled, Native Android Toolkit will send a notification to the user of your app, whenever NAT detects that the time has been changed manually. Basically this notification sent to your user can let you know that your app is now aware that the time has been changed manually and that your app can take some steps to prevent cheating."),
                       natPreferences.warningOnChangeDate);
            if (natPreferences.warningOnChangeDate == true)
            {
                EditorGUI.indentLevel += 1;
                natPreferences.titleOfWarningOnChangeDate = EditorGUILayout.TextField(new GUIContent("Title Of Notification",
                        "The title of the warning notification that will be sent to the user when he changes the device time, manually."),
                        natPreferences.titleOfWarningOnChangeDate);

                natPreferences.textOfWarningOnChangeDate = EditorGUILayout.TextField(new GUIContent("Text Of Notification",
                        "The text of the warning notification that will be sent to the user when he changes the device time, manually."),
                        natPreferences.textOfWarningOnChangeDate);
                EditorGUI.indentLevel -= 1;
            }

            if (natPreferences.minApiLevel != NativeAndroidPreferences.MinSdkVersion.AndroidJellyBeanApi16)
            {
                EditorGUILayout.HelpBox("Minimum API level selected is not recommended for this version of Native Android Toolkit.", MessageType.Warning);
            }
            natPreferences.minApiLevel = (NativeAndroidPreferences.MinSdkVersion)EditorGUILayout.EnumPopup(new GUIContent("Minimum API Level",
                                "Defines the minimum API required to run Native Android Toolkit. Always leave the required API of your Unity project at or below. Changing to an API level other than recommended may cause malfunctions in NAT functions."),
                                natPreferences.minApiLevel);

            if (natPreferences.targetSdkVersion != NativeAndroidPreferences.TargetSdkVersion.AndroidOreoApi26)
            {
                EditorGUILayout.HelpBox("Target API level selected is not recommended for this version of Native Android Toolkit.", MessageType.Warning);
            }
            natPreferences.targetSdkVersion = (NativeAndroidPreferences.TargetSdkVersion)EditorGUILayout.EnumPopup(new GUIContent("Target API Level",
                                "The target API level to which NAT was built. Always leave this option at the recommended value because NAT is built to run at the recommended API level. Even if the device running NAT has a higher API level than the NAT target API level, this should not cause problems. There is usually no reason to change this option.\n\nRemember that: You must have SDK equal to or greater than target API. For example, if you select API level 28, you will need your project to use SDK 28 or higher to be able to compile NAT, so keep the target API version as recommended."),
                                natPreferences.targetSdkVersion);

            //Asset preferences
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Editor Preferences", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            natPreferences.launchLogs = EditorGUILayout.Toggle(new GUIContent("Launch Logs",
                        "Enable this option if you want Native Android Toolkit to display mock logs whenever any Android Native Android Toolkit method is called while your app runs in the Editor.\n\nEven if this option is enabled, logs will only be displayed if your app runs in the Editor."),
                        natPreferences.launchLogs);

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);

            //Save the changes
            if (GUILayout.Button("Save Preferences", GUILayout.Height(32)))
            {
                SaveChanges();
                return;
            }

            //If save preferences on open is true, save preferences
            if (savePreferencesNowOnOpen == true)
            {
                numberOfUpdates += 1;
                if (numberOfUpdates >= 6)
                {
                    SaveChanges();
                    savePreferencesNowOnOpen = false;
                    numberOfUpdates = 0;
                    return;
                }
            }

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        void SaveChanges()
        {
            //Validate the icon
            if (natPreferences.iconIsValid == false)
            {
                EditorUtility.DisplayDialog("Invalid Icon", "The notification icon provided must be \".png\" format and have a resolution of 64x64.", "Ok");
                return;
            }

            //Show progress dialog
            EditorUtility.DisplayProgressBar("A moment", "Saving...", 1f);

            //Save the preferences
            SaveThePreferences();

            //Crash protection
            bool errorsFounded = false;
            try
            {
                //If is desired to modify android manifest
                if (natPreferences.modifyAndroidManifest == NativeAndroidPreferences.ModifyManifest.YesCreateNewIfNotExists)
                {
                    //If not exists the AndroidManifest.xml, create then
                    if (AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(object)) == null)
                    {
                        CreateBaseAndroidManifest();
                    }

                    //Apply the declaration of UnityPlayerActivity
                    DeclareUnityPlayerActivity(natPreferences.declareUnityPlayerActivity, natPreferences.unityPlayerActivityIsDefault);

                    //If request permissions on open, is disabled
                    if (natPreferences.requestPermissionsOnOpen == false)
                    {
                        ChangeMetaDataValue("unityplayer.SkipPermissionsDialog", "true");
                    }

                    //If request permissions on open, is enabled
                    if (natPreferences.requestPermissionsOnOpen == true)
                    {
                        ChangeMetaDataValue("unityplayer.SkipPermissionsDialog", "false");
                    }
                }

#if MTAssets_IonicZip_Available
                //Extract the AAR
                string zipToUnpack = "Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Android/NAT Core.aar";
                string unpackDirectory = "Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract";
                using (Ionic.Zip.ZipFile zipUnpack = Ionic.Zip.ZipFile.Read(zipToUnpack))
                {
                    //Here, we extract every entry, but we could extract conditionally
                    //Based on entry name, size, date, checkbox status, etc.  
                    foreach (Ionic.Zip.ZipEntry e in zipUnpack)
                    {
                        e.Extract(unpackDirectory, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                AssetDatabase.Refresh();

                //Replace the icon of AAR with the selected icon
                AssetDatabase.DeleteAsset("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/res/drawable/custom_small_icon.png");
                AssetDatabase.Refresh();
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(natPreferences.iconOfNotifications), "Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/res/drawable/custom_small_icon.png");
                AssetDatabase.Refresh();

                //Add or remove desired permissions
                AddOrRemovePermission("CAMERA", natPreferences.cameraPermission);
                AddOrRemovePermission("FLASHLIGHT", natPreferences.cameraPermission);
                AddOrRemovePermission("READ_EXTERNAL_STORAGE", natPreferences.filePermission);
                AddOrRemovePermission("WRITE_EXTERNAL_STORAGE", natPreferences.filePermission);
                AddOrRemovePermission("ACCESS_WIFI_STATE", natPreferences.accessWifiStatePermission);
                AddOrRemovePermission("VIBRATE", natPreferences.vibratePermission);
                AddOrRemovePermission("ACCESS_NETWORK_STATE", natPreferences.networkStatePermission);
                AddOrRemovePermission("BLUETOOTH", natPreferences.bluetoothPermission);
                AddOrRemovePermission("BLUETOOTH_ADMIN", natPreferences.bluetoothPermission);
                AddOrRemovePermission("INTERNET", natPreferences.internetPermission);
                AddOrRemovePermission("MODIFY_AUDIO_SETTINGS", natPreferences.modifyAudioPermission);
                AddOrRemovePermission("WAKE_LOCK", natPreferences.keepDeviceEnabled);

                //Change all desired meta-data
                ChangeAarMetaDataValue("sendNotificationOnChangeTime", (natPreferences.warningOnChangeDate == true) ? "true" : "false");
                ChangeAarMetaDataValue("titleOfTimeChangedNotification", natPreferences.titleOfWarningOnChangeDate);
                ChangeAarMetaDataValue("textOfTimeChangedNotification", natPreferences.textOfWarningOnChangeDate);
                if (natPreferences.keepDeviceEnabled == true)
                    ChangeAarMetaDataValue("enableAccurateNotify", (natPreferences.enableAccurateNotify == true) ? "true" : "false");
                if (natPreferences.keepDeviceEnabled == false)
                    ChangeAarMetaDataValue("enableAccurateNotify", "false");

                //Change the sdk info
                ChangeApiLevelinfo(natPreferences.minApiLevel, natPreferences.targetSdkVersion);
#endif

                errorsFounded = false;
            }
            catch (Exception e)
            {
                //Show the warning
                Debug.LogError("An error occurred while saving preferences, so saving was interrupted. Click here for more information.\n\n" + e);
                errorsFounded = true;
            }

            //If the protection not found errors
            if (errorsFounded == false)
            {
#if MTAssets_IonicZip_Available
                //Delete the original AAR
                AssetDatabase.DeleteAsset("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Android/NAT Core.aar");
                AssetDatabase.Refresh();

                //Get file separator
                string bar = "";
#if !UNITY_2019_1_OR_NEWER
                if (PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Legacy)
                {
                    bar = "/";
                }
                if (PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest)
                {
                    bar = "\\";
                }
#endif
#if UNITY_2019_1_OR_NEWER
            bar = "\\";
#endif

                //Zip the Extract folder into a new AAR again
                DirectoryInfo dirToCompact = new DirectoryInfo("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract");
                var ext = new List<string> { ".jpg", ".gif", ".png", ".xml", ".jar", ".txt" };
                var filePaths = Directory.GetFiles(dirToCompact.ToString(), "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s)));
                Ionic.Zip.ZipFile zipPack = new Ionic.Zip.ZipFile("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Android/NAT Core.aar");
                zipPack.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                foreach (string filePath in filePaths)
                {
                    FileAttributes fileAttributes = File.GetAttributes(filePath);
                    if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        zipPack.AddDirectory(filePath, Path.GetDirectoryName(filePath).Replace("Assets" + bar + "MT Assets" + bar + "Native Android Toolkit" + bar + "Libraries" + bar + "Plugins" + bar + "Extract", ""));
                    }
                    else
                    {
                        zipPack.AddFile(filePath, Path.GetDirectoryName(filePath).Replace("Assets" + bar + "MT Assets" + bar + "Native Android Toolkit" + bar + "Libraries" + bar + "Plugins" + bar + "Extract", ""));
                    }
                }
                zipPack.AddDirectoryByName("drawable");
                zipPack.AddDirectoryByName("layout");
                zipPack.AddDirectoryByName("values");
                zipPack.Save();
                AssetDatabase.Refresh();

                //Delete the classes.jar to avoid problemns on build
                AssetDatabase.DeleteAsset("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/classes.jar");
                AssetDatabase.Refresh();
#endif
#if !MTAssets_IonicZip_Available
                EditorUtility.DisplayDialog("Warning", "It was not possible to identify the Ionic.Zip included in the Native Android Toolkit, therefore, it was not possible to apply the changes to the NAT core AAR (this means that changes in permissions, for example, could not be applied). Consult the documentation for more details.", "Ok");
#endif

                //Apply the last modification of NAT Core.aar to the file
                string lastModifyDate = File.GetLastWriteTimeUtc("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Android/NAT Core.aar").ToString();
                File.WriteAllText("Assets/MT Assets/_AssetsData/Editor/NatCoreLastWrite.ini", lastModifyDate);

                //Refresh assets
                AssetDatabase.Refresh();

                //Hide progress dialog
                EditorUtility.ClearProgressBar();

                //Show warn
                EditorUtility.DisplayDialog("Done", "The changes have been applied!", "Ok");
            }
            if (errorsFounded == true)
            {
                //Refresh assets
                AssetDatabase.Refresh();

                //Hide progress dialog
                EditorUtility.ClearProgressBar();

                //Show warn
                EditorUtility.DisplayDialog("Error", "Errors occurred while saving preferences, please check your Unity console for more information.\n\nIf you are experiencing errors frequently, please try to update your Native Android Toolkit, re-install it in your project. If you are still having problems, contact us at mtassets@windsoft.xyz", "Ok");
            }
        }

        public void CreateBaseAndroidManifest()
        {
            //Create the directory
            if (!AssetDatabase.IsValidFolder("Assets/Plugins"))
            {
                AssetDatabase.CreateFolder("Assets", "Plugins");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Plugins/Android"))
            {
                AssetDatabase.CreateFolder("Assets/Plugins", "Android");
            }

            //Load the AndroidManifestBase text
            string androidManifestBaseStr = File.ReadAllText("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Manifest/AndroidManifestBase.xml");

            //Insert the current date
            DateTime dateNow = DateTime.Now;
            string newAndroidManifestBaseStr = androidManifestBaseStr.Replace("%DATE%", dateNow.Year + "/" + dateNow.Month + "/" + dateNow.Day + "-" + dateNow.Hour + ":" + dateNow.Minute + ":" + dateNow.Second);

            //Write the new AndroidManifest
            File.WriteAllText("Assets/Plugins/Android/AndroidManifest.xml", newAndroidManifestBaseStr);
        }

        public void DeclareUnityPlayerActivity(bool declare, bool isMain)
        {
            //Read the AndroidManifest.xml
            string manifestXml = File.ReadAllText("Assets/Plugins/Android/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //If is desired to not declare
            if (declare == false)
            {
                //Get all nodes of activity
                foreach (var node in xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList())
                {
                    node.Remove();
                }
            }
            //If is desired to declare
            if (declare == true)
            {
                //Count quantity of activities declared that exists, where attributes is equal to informed
                var nodes = xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList();

                //If exists more than one activity declaration with that attributes, delete all to create only one
                if (nodes.Count > 1)
                {
                    foreach (var node in nodes)
                    {
                        node.Remove();
                    }
                    nodes.Clear();
                }

                //If not exists the declaration of UnityPlayerActivity, create then
                if (nodes.Count == 0)
                {
                    var manifest = xmlDoc.Descendants("application").FirstOrDefault();
                    manifest.Add(new XElement("activity", new XAttribute(xmlAndroidNs + "name", "com.unity3d.player.UnityPlayerActivity"), new XAttribute(xmlAndroidNs + "label", "@string/app_name")));
                    foreach (var node in xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList())
                    {
                        XElement action = new XElement("action", new XAttribute(xmlAndroidNs + "name", "android.intent.action.MAIN"));
                        XElement category = new XElement("category", new XAttribute(xmlAndroidNs + "name", "android.intent.category.LAUNCHER"));
                        node.Add(new XElement("intent-filter", action, category));
                        node.Add(new XElement("meta-data", new XAttribute(xmlAndroidNs + "name", "unityplayer.UnityActivity"), new XAttribute(xmlAndroidNs + "value", "true")));
                        node.Add(new XElement("meta-data", new XAttribute(xmlAndroidNs + "name", "unityplayer.ForwardNativeEventsToDalvik"), new XAttribute(xmlAndroidNs + "value", "true")));
                    }
                }

                //Delete the intent filter
                foreach (var node in xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList())
                {
                    node.Descendants("intent-filter").Remove();
                }

                //If is desired to set UnityPlayer Activity as Main
                if (isMain == true)
                {
                    foreach (var node in xmlDoc.Descendants("activity").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "com.unity3d.player.UnityPlayerActivity").ToList())
                    {
                        XElement action = new XElement("action", new XAttribute(xmlAndroidNs + "name", "android.intent.action.MAIN"));
                        XElement category = new XElement("category", new XAttribute(xmlAndroidNs + "name", "android.intent.category.LAUNCHER"));
                        node.Add(new XElement("intent-filter", action, category));
                    }
                }
            }

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the new AndroidManifest.xml
            File.WriteAllText("Assets/Plugins/Android/AndroidManifest.xml", stringResult);
        }

        public void ChangeMetaDataValue(string name, string value)
        {
            //Read the AndroidManifest.xml
            string manifestXml = File.ReadAllText("Assets/Plugins/Android/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //Count quantity of meta-data that exists, where attributes is equal to informed
            var nodes = xmlDoc.Descendants("meta-data").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == name).ToList();

            //If exists more than one meta-data with that attributes, delete all to create only one
            if (nodes.Count > 1)
            {
                foreach (var node in nodes)
                {
                    node.Remove();
                }
                nodes.Clear();
            }

            //If not exists a meta-data with that attributes, create then
            if (nodes.Count == 0)
            {
                var manifest = xmlDoc.Descendants("application").FirstOrDefault();
                manifest.Add(new XElement("meta-data", new XAttribute(xmlAndroidNs + "name", name), new XAttribute(xmlAndroidNs + "value", value)));
            }

            //Get all nodes of meta-data
            foreach (var node in xmlDoc.Descendants("meta-data").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == name).ToList())
            {
                node.Attribute(xmlAndroidNs + "value").SetValue(value);
            }

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the new AndroidManifest.xml
            File.WriteAllText("Assets/Plugins/Android/AndroidManifest.xml", stringResult);
        }

        public void AddOrRemovePermission(string permissionName, bool enabled)
        {
            //Load the AndroidManifest of AAR
            string manifestXml = File.ReadAllText("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //Remove or add the desired Permission, according to option
            if (enabled == true)
            {
                //Delete all nodes of desired permission found, to reset
                foreach (var node in xmlDoc.Descendants("uses-permission").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "android.permission." + permissionName).ToList())
                {
                    node.Remove();
                }

                //Get the uses-sdk node to add the permission after this
                var internetPermissionNode = xmlDoc.Descendants("uses-sdk").ToList();

                //Add the nodes of desired permission after uses-sdk node
                internetPermissionNode[0].AddAfterSelf(new XElement("uses-permission", new XAttribute(xmlAndroidNs + "name", "android.permission." + permissionName)));
            }
            if (enabled == false)
            {
                //Delete all desired permission
                foreach (var node in xmlDoc.Descendants("uses-permission").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == "android.permission." + permissionName).ToList())
                {
                    node.Remove();
                }
            }

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the modified new AndroidManifest in AAR to compress
            File.WriteAllText("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/AndroidManifest.xml", stringResult);
        }

        public void ChangeAarMetaDataValue(string name, string value)
        {
            //Read the AndroidManifest.xml
            string manifestXml = File.ReadAllText("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //Count quantity of meta-data that exists, where attributes is equal to informed
            var nodes = xmlDoc.Descendants("meta-data").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == name).ToList();

            //If exists more than one meta-data with that attributes, delete all to create only one
            if (nodes.Count > 1)
            {
                foreach (var node in nodes)
                {
                    node.Remove();
                }
                nodes.Clear();
            }

            //If not exists a meta-data with that attributes, create then
            if (nodes.Count == 0)
            {
                var manifest = xmlDoc.Descendants("application").FirstOrDefault();
                manifest.Add(new XElement("meta-data", new XAttribute(xmlAndroidNs + "name", name), new XAttribute(xmlAndroidNs + "value", value)));
            }

            //Get all nodes of meta-data
            foreach (var node in xmlDoc.Descendants("meta-data").Where(node => (string)node.Attribute(xmlAndroidNs + "name").Value == name).ToList())
            {
                node.Attribute(xmlAndroidNs + "value").SetValue(value);
            }

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the new AndroidManifest.xml
            File.WriteAllText("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/AndroidManifest.xml", stringResult);
        }

        public void ChangeApiLevelinfo(NativeAndroidPreferences.MinSdkVersion minSdkVersion, NativeAndroidPreferences.TargetSdkVersion targetSdkVersion)
        {
            //Get the data
            string minSdkVersionStr = "";
            string targetSdkVersionStr = "";
            switch (minSdkVersion)
            {
                case NativeAndroidPreferences.MinSdkVersion.AndroidJellyBeanApi16:
                    minSdkVersionStr = "16";
                    break;
            }
            switch (targetSdkVersion)
            {
                case NativeAndroidPreferences.TargetSdkVersion.AndroidOreoApi26:
                    targetSdkVersionStr = "26";
                    break;
                case NativeAndroidPreferences.TargetSdkVersion.AndroidOreoApi27:
                    targetSdkVersionStr = "27";
                    break;
                case NativeAndroidPreferences.TargetSdkVersion.AndroidPieApi28:
                    targetSdkVersionStr = "28";
                    break;
            }

            //Load the AndroidManifest of AAR
            string manifestXml = File.ReadAllText("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/AndroidManifest.xml");

            //Load AndroidManifest.xml, and set Android namespace
            XDocument xmlDoc = XDocument.Parse(manifestXml);
            XNamespace xmlAndroidNs = "http://schemas.android.com/apk/res/android";
            xmlDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            //Change api level data
            foreach (var node in xmlDoc.Descendants("uses-sdk").ToList())
            {
                node.Attribute(xmlAndroidNs + "minSdkVersion").Value = minSdkVersionStr;
                node.Attribute(xmlAndroidNs + "targetSdkVersion").Value = targetSdkVersionStr;
            }

            //Change the AndroidManifest result from utf-16 to utf-8
            var wr = new Utf8StringWriter();
            xmlDoc.Save(wr);
            string stringResult = (wr.GetStringBuilder().ToString());

            //Save the modified new AndroidManifest in AAR to compress
            File.WriteAllText("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Extract/AndroidManifest.xml", stringResult);
        }
    }
}