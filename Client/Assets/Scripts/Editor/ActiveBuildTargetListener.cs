using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
// ReSharper disable DelegateSubtraction

public class ActiveBuildTargetListener : IActiveBuildTargetChanged
{
    private const string NiceVibrationsVer39 = "3.9";
    private const string NiceVibrationsVer41 = "4.1";
    
    public int callbackOrder => 0;
    
    public void OnActiveBuildTargetChanged(BuildTarget _PreviousTarget, BuildTarget _NewTarget)
    {
        switch (_NewTarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneOSX:
                SwitchToIos();
                break;
            case BuildTarget.Android: ConfigureAndroidPlatform(); break;
            case BuildTarget.iOS:     ConfigureIosPlatform();     break;
        }
    }

    private static void SwitchToIos()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
    }

    private static void ConfigureAndroidPlatform()
    {
        var target = NamedBuildTarget.Android;
        SetNiceVibrationsPluginVersion(NiceVibrationsVer39, target);
        // AddAppodeal(target);
        // RemoteUnityAds(target);
        // AddUnityAds(target);
    }

    private static void ConfigureIosPlatform()
    {
        var target = NamedBuildTarget.iOS;
        SetNiceVibrationsPluginVersion(NiceVibrationsVer41, target);
        // RemoveAppodeal(target);
        // AddUnityAds(target);
    }

    private static void SetNiceVibrationsPluginVersion(string _Version, NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        var pluginSymbols = new[]
        {
            "NICE_VIBRATIONS_3_9",
            "MOREMOUNTAINS_NICEVIBRATIONS",
            "MOREMOUNTAINS_NICEVIBRATIONS_RUMBLE"
        };
        string path = GetOlderNiceVibrationsPluginPath(false);
        switch (_Version)
        {
            case NiceVibrationsVer39:
                foreach (string symbolToAdd in pluginSymbols)
                {
                    if (!scriptDefSymbols.Contains(symbolToAdd))
                        scriptDefSymbols.Add(symbolToAdd);
                }
                PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
                string bakPath = GetOlderNiceVibrationsPluginPath(true);
                if (!Directory.Exists(bakPath))
                {
                    Dbg.LogError("Older plugin not found in path: " + bakPath);
                    return;
                }
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                CopyFilesRecursively(bakPath, path);
                break;
            case NiceVibrationsVer41:
                foreach (string symbolToRemove in pluginSymbols)
                {
                    if (scriptDefSymbols.Contains(symbolToRemove))
                        scriptDefSymbols.Remove(symbolToRemove);
                }
                PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                break;
        }
    }
    
    private static void AddAppodeal(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (!scriptDefSymbols.Contains("APPODEAL"))
            scriptDefSymbols.Add("APPODEAL");
        string path = GetAppodealPluginPath(false);
        string bakPath = GetAppodealPluginPath(true);
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        if (!Directory.Exists(bakPath))
        {
            Dbg.LogError("Older plugin not found in path: " + bakPath);
            return;
        }
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        CopyFilesRecursively(bakPath, path);
    }

    private static void RemoveAppodeal(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (scriptDefSymbols.Contains("APPODEAL"))
            scriptDefSymbols.Remove("APPODEAL");
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        string path = GetAppodealPluginPath(false);
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }
    
    private static string GetOlderNiceVibrationsPluginPath(bool _Bak)
    {
        string dir = Directory.GetCurrentDirectory();
        if (!_Bak) return Path.Combine(dir,
            "Assets",
            "NiceVibrations",
            "OlderVersions",
            "v3.9");
        return Path.Combine(dir,
            "_Other",
            "additional-packages",
            "NiceVibrations-v3.9");
    }
    
    private static string GetAppodealPluginPath(bool _Bak)
    {
        string dir = Directory.GetCurrentDirectory();
        if (!_Bak) return Path.Combine(dir,
            "Assets",
            "Appodeal");
        return Path.Combine(dir,
            "_Other",
            "additional-packages",
            "Appodeal");
    }
    
    private static List<string> GetScriptingDefineSymbols(NamedBuildTarget _Target)
    {
        string allSymbolsRaw = PlayerSettings.GetScriptingDefineSymbols(_Target);
        return allSymbolsRaw.Split(";").ToList();
    }
    
    private static void CopyFilesRecursively(string _SourcePath, string _TargetPath)
    {
        foreach (string dirPath in Directory.GetDirectories(_SourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(
                dirPath.Replace(_SourcePath, 
                    _TargetPath));
        }
        foreach (string newPath in Directory.GetFiles(_SourcePath, "*.*",SearchOption.AllDirectories))
        {
            File.Copy(
                newPath, 
                newPath.Replace(_SourcePath, _TargetPath), 
                true);
        }
    }
    private static AddRequest _addUnityAdsRequest;
    private static RemoveRequest _removeUnityAdsRequest;
    
    private static void AddUnityAds(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (!scriptDefSymbols.Contains("UNITY_ADS_API"))
            scriptDefSymbols.Add("UNITY_ADS_API"); 
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        _addUnityAdsRequest = Client.Add("com.unity.ads");
        EditorApplication.update += AddUnityAdsProgress;    
    }

    private static void RemoteUnityAds(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (scriptDefSymbols.Contains("UNITY_ADS_API"))
            scriptDefSymbols.Remove("UNITY_ADS_API");
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        _removeUnityAdsRequest = Client.Remove("com.unity.ads");
        EditorApplication.update += RemoveUnityAdsProgress;
    }
    
   private static void AddUnityAdsProgress()
    {
        if (_addUnityAdsRequest.IsCompleted)
        {
            if (_addUnityAdsRequest.Status == StatusCode.Success)
                Dbg.Log("Installed package: " + _addUnityAdsRequest.Result.packageId + " version: " + _addUnityAdsRequest.Result.version);
            else if (_addUnityAdsRequest.Status >= StatusCode.Failure)
                Dbg.Log(_addUnityAdsRequest.Error.message);
            EditorApplication.update -= AddUnityAdsProgress;
        }
    }
   
   private static void RemoveUnityAdsProgress()
   {
       if (_removeUnityAdsRequest.IsCompleted)
       {
           if (_removeUnityAdsRequest.Status == StatusCode.Success)
               Dbg.Log("Uninstalled package: " + _removeUnityAdsRequest.PackageIdOrName);
           else if (_addUnityAdsRequest.Status >= StatusCode.Failure)
               Dbg.Log(_removeUnityAdsRequest.Error.message);
           EditorApplication.update -= RemoveUnityAdsProgress;
       }
   }
}
