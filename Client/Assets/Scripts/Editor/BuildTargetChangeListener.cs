using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
// ReSharper disable DelegateSubtraction
// ReSharper disable CheckNamespace

public class BuildTargetChangeListener : IActiveBuildTargetChanged
{
    #region constants

    private const string NiceVibrationsPluginVer39           = "v3.9";
    private const string NiceVibrationsPluginVer41           = "v4.1";
    private const string Appodeal        = "Appodeal";
    private const string NiceVibrations  = "NiceVibrations";
    private const string GoogleMobileAds = "GoogleMobileAds";

    #endregion

    #region nonpublic members

    private static AddRequest    _addUnityAdsRequest,    _addAppodealRequest;
    private static RemoveRequest _removeUnityAdsRequest, _removeAppodealRequest;

    #endregion

    #region api
    
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

    #endregion

    #region nonpublic methods
    
    private static void SwitchToIos()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
    }

    private static void ConfigureAndroidPlatform()
    {
        // var target = NamedBuildTarget.Android;
        // SetNiceVibrationsPluginVersion(NiceVibrationsPluginVer39, target);
    }

    private static void ConfigureIosPlatform()
    {
        // var target = NamedBuildTarget.iOS;
        // SetNiceVibrationsPluginVersion(NiceVibrationsPluginVer41, target);
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
    
    private static string GetPluginPath(string _Plugin, bool _Bak)
    {
        string dir = Directory.GetCurrentDirectory();
        if (!_Bak)
        {
            return _Plugin == NiceVibrations ? 
                Path.Combine(dir, "Assets", _Plugin, "OlderVersions", $"{NiceVibrationsPluginVer39}") 
                : Path.Combine(dir, "Assets", _Plugin);
        }
        return _Plugin == NiceVibrations ? 
            Path.Combine(dir, "_Other", "additional-packages", $"{NiceVibrations}-{NiceVibrationsPluginVer39}") 
            : Path.Combine(dir, "_Other", "additional-packages",
           _Plugin);
    }

    private static void AddPluginFiles(string _Plugin)
    {
        string path = GetPluginPath(_Plugin, false);
        string bakPath = GetPluginPath(_Plugin, true);
        if (!Directory.Exists(bakPath))
        {
            Dbg.LogError("Older plugin not found in path: " + bakPath);
            return;
        }
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        CopyFilesRecursively(bakPath, path);
    }
    
    private static void RemovePluginFiles(string _Plugin)
    {
        string path = GetPluginPath(_Plugin, false);
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    private static bool AddRequestCompletion(AddRequest _Request)
    {
        if (!_Request.IsCompleted) 
            return false;
        if (_Request.Status == StatusCode.Success)
            Dbg.Log("Installed package: " + _Request.Result.packageId + " version: " + _Request.Result.version);
        else if (_Request.Status >= StatusCode.Failure)
            Dbg.LogError(_Request.Error.message);
        return true;
    }
    
    private static bool RemoveRequestCompletion(RemoveRequest _Request)
    {
        if (!_Request.IsCompleted) 
            return false;
        if (_Request.Status == StatusCode.Success)
            Dbg.Log("Uninstalled package: " + _Request.PackageIdOrName);
        else if (_Request.Status >= StatusCode.Failure)
            Dbg.LogError(_Request.Error.message);
        return true;
    }

    #endregion
    
    #region NICE VIBRATIONS

    public static void SetNiceVibrationsPluginV39(NamedBuildTarget _Target)
    {
        SetNiceVibrationsPluginVersion(NiceVibrationsPluginVer39, _Target);
    }
    
    public static void SetNiceVibrationsPluginV41(NamedBuildTarget _Target)
    {
        SetNiceVibrationsPluginVersion(NiceVibrationsPluginVer41, _Target);
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
        string path = GetPluginPath(NiceVibrations, false);
        switch (_Version)
        {
            case NiceVibrationsPluginVer39:
                foreach (string symbolToAdd in pluginSymbols)
                {
                    if (!scriptDefSymbols.Contains(symbolToAdd))
                        scriptDefSymbols.Add(symbolToAdd);
                }
                PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
                string bakPath = GetPluginPath(NiceVibrations, true);
                if (!Directory.Exists(bakPath))
                {
                    Dbg.LogError("Older plugin not found in path: " + bakPath);
                    return;
                }
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                CopyFilesRecursively(bakPath, path);
                break;
            case NiceVibrationsPluginVer41:
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
    
    #endregion
    
    #region APPODEAL
    
    public static void AddAppodeal(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (!scriptDefSymbols.Contains("APPODEAL_3"))
            scriptDefSymbols.Add("APPODEAL_3");
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        // _addAppodealRequest = Client.Add("https://github.com/appodeal/appodeal-unity-plugin-upm.git");
        _addAppodealRequest = Client.Add("https://github.com/appodeal/appodeal-unity-plugin-upm.git#feature/release-3.0.1");
        EditorApplication.update += AddAppodealProgress;
    }

    public static void RemoveAppodeal(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (scriptDefSymbols.Contains("APPODEAL_3"))
            scriptDefSymbols.Remove("APPODEAL_3");
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        RemoveAppodealSettingsFolder();
        _removeAppodealRequest = Client.Remove("com.appodeal.appodeal-unity-plugin-upm");
        EditorApplication.update += RemoveAppodealProgress;
    }

    public static void CopyAppodealSettingsFromBackup()
    {
        AddPluginFiles(Appodeal);
    }

    public static void RemoveAppodealSettingsFolder()
    {
        RemovePluginFiles(Appodeal);
    }
    
    

    private static void AddAppodealProgress()
    {
        AddRequestCompletion(_addAppodealRequest);
        EditorApplication.update -= AddAppodealProgress;
    }
   
    private static void RemoveAppodealProgress()
    {
        RemoveRequestCompletion(_removeAppodealRequest);
        EditorApplication.update -= RemoveAppodealProgress;
    }
    
    #endregion

    #region ADMOB

    public static void AddGoogleAds(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (!scriptDefSymbols.Contains("ADMOB_API"))
            scriptDefSymbols.Add("ADMOB_API"); 
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        AddPluginFiles(GoogleMobileAds);
    }

    public static void RemoveGoogleAds(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (scriptDefSymbols.Contains("ADMOB_API"))
            scriptDefSymbols.Remove("ADMOB_API");
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        RemovePluginFiles(GoogleMobileAds);
    }

    #endregion

    #region UNITY ADS

    public static void AddUnityAds(NamedBuildTarget _Target)
    {
        var scriptDefSymbols = GetScriptingDefineSymbols(_Target);
        if (!scriptDefSymbols.Contains("UNITY_ADS_API"))
            scriptDefSymbols.Add("UNITY_ADS_API"); 
        PlayerSettings.SetScriptingDefineSymbols(_Target, scriptDefSymbols.ToArray());
        _addUnityAdsRequest = Client.Add("com.unity.ads");
        EditorApplication.update += AddUnityAdsProgress;    
    }

    public static void RemoveUnityAds(NamedBuildTarget _Target)
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
        AddRequestCompletion(_addUnityAdsRequest);
        EditorApplication.update -= AddUnityAdsProgress;
    }
   
    private static void RemoveUnityAdsProgress()
    {
        RemoveRequestCompletion(_removeUnityAdsRequest);
        EditorApplication.update -= RemoveUnityAdsProgress;
    }

    #endregion
}
