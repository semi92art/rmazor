using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This class is responsible for detecting if Ionic.Zip already is present in the project, so the Native Android Toolkit component can handle each situation.
     */

    [InitializeOnLoad]
    class IonicZip
    {
        static IonicZip()
        {
            //Run the script after Unity compiles
            EditorApplication.delayCall += DetectIfIonicZipDllExists;
        }

        public static void DetectIfIonicZipDllExists()
        {
            //Delete the old DLL if exists
            if (AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Editor/Ionic.Zip-1.9.8.dll", typeof(object)) != null)
                AssetDatabase.DeleteAsset("Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Editor/Ionic.Zip-1.9.8.dll");

            //Get active build target
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            if (NamespaceExists("Ionic.Zip") == false)
            {
                //If Ionic.Zip is not available, copy and instal the Ionic.Zip built in NAT 

                //Create the directory in project
                if (!AssetDatabase.IsValidFolder("Assets/MT Assets"))
                    AssetDatabase.CreateFolder("Assets", "MT Assets");
                if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                    AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
                if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                    AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
                if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Editor"))
                    AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Editor");

                AssetDatabase.CopyAsset(
                    "Assets/MT Assets/Native Android Toolkit/Libraries/Plugins/Editor/Ionic.Zip-1.9.8.BuiltIn.nat",
                    "Assets/MT Assets/_AssetsData/Editor/Ionic.Zip-1.9.8.dll");
                
                EditorUtility.DisplayDialog("IonicZip Imported", "The Native Android Toolkit imported the DLL \"Ionic.Zip\" for your project. This plugin allows the Native Android Toolkit to edit the core AAR file of the Native Android Toolkit, through the preferences window, accessible in \"Tools > MT Assets > Native Android Toolkit > Preferences\".\n\nThis is a protection mechanism to avoid conflicts between 1 or more DLL's in your project.\n\nYou can find the imported DLL at \"Assets/MT Assets/_AssetsData/Editor\".\n\nThis was just a warning, from the Native Android Toolkit installation wizard!", "Ok");
                
                AssetDatabase.Refresh();
            }

            //Try to load the Ionic.Zip of NAT
            object ionicZip = AssetDatabase.LoadAssetAtPath("Assets/MT Assets/_AssetsData/Editor/Ionic.Zip-1.9.8.dll", typeof(object));

            //If the Ionic.Zip of NAT exists, add the definition
            if (ionicZip == null)
                RemoveDefineIfNecessary("MTAssets_IonicZip_Available", BuildPipeline.GetBuildTargetGroup(buildTarget));
            if (ionicZip != null)
                AddDefineIfNecessary("MTAssets_IonicZip_Available", BuildPipeline.GetBuildTargetGroup(buildTarget));
        }

        public static bool NamespaceExists(string desiredNamespace)
        {
            //Return true if namespace exists
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace == desiredNamespace)
                        return true;
                }
            }
            return false;
        }

        public static void AddDefineIfNecessary(string _define, BuildTargetGroup _buildTargetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);

            if (defines == null) { defines = _define; }
            else if (defines.Length == 0) { defines = _define; }
            else { if (defines.IndexOf(_define, 0) < 0) { defines += ";" + _define; } }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, defines);
        }

        public static void RemoveDefineIfNecessary(string _define, BuildTargetGroup _buildTargetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);

            if (defines.StartsWith(_define + ";"))
            {
                // First of multiple defines.
                defines = defines.Remove(0, _define.Length + 1);
            }
            else if (defines.StartsWith(_define))
            {
                // The only define.
                defines = defines.Remove(0, _define.Length);
            }
            else if (defines.EndsWith(";" + _define))
            {
                // Last of multiple defines.
                defines = defines.Remove(defines.Length - _define.Length - 1, _define.Length + 1);
            }
            else
            {
                // Somewhere in the middle or not defined.
                var index = defines.IndexOf(_define, 0, System.StringComparison.Ordinal);
                if (index >= 0) { defines = defines.Remove(index, _define.Length + 1); }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, defines);
        }
    }
}