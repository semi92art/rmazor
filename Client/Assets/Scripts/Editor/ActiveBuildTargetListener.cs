using System.IO;
using System.Linq;
using Common;
using UnityEditor;
using UnityEditor.Build;

public class ActiveBuildTargetListener : IActiveBuildTargetChanged
{
    public int callbackOrder => 0;
    
    public void OnActiveBuildTargetChanged(BuildTarget _PreviousTarget, BuildTarget _NewTarget)
    {
        switch (_NewTarget)
        {
            case BuildTarget.Android: ConfigureAndroidPlatform(); break;
            case BuildTarget.iOS:     ConfigureIosPlatform();     break;
        }
    }

    private static void ConfigureAndroidPlatform()
    {
        string allSymbolsRaw = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
        var allSymbols = allSymbolsRaw.Split(";").ToList();
        var symbolsToAdd = new[]
        {
            "NICE_VIBRATIONS_3_9",
            "MOREMOUNTAINS_NICEVIBRATIONS",
            "MOREMOUNTAINS_NICEVIBRATIONS_RUMBLE"
        };
        foreach (string symbolToAdd in symbolsToAdd)
        {
            if (!allSymbols.Contains(symbolToAdd))
                allSymbols.Add(symbolToAdd);
        }
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, allSymbols.ToArray());
        string bakPath = GetOlderPluginPath(true);
        string path = GetOlderPluginPath(false);
        if (!Directory.Exists(bakPath))
        {
            Dbg.LogError("Older plugin not found in path: " + bakPath);
            return;
        }
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        CopyFilesRecursively(bakPath, path);
    }

    private static void ConfigureIosPlatform()
    {
        string allSymbolsRaw = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.iOS);
        var allSymbols = allSymbolsRaw.Split(";").ToList();
        var symbolsToRemove = new[]
        {
            "NICE_VIBRATIONS_3_9",
            "MOREMOUNTAINS_NICEVIBRATIONS",
            "MOREMOUNTAINS_NICEVIBRATIONS_RUMBLE"
        };
        foreach (string symbolToRemove in symbolsToRemove)
        {
            if (allSymbols.Contains(symbolToRemove))
                allSymbols.Remove(symbolToRemove);
        }
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.iOS, allSymbols.ToArray());
        string path = GetOlderPluginPath(false);
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    private static string GetOlderPluginPath(bool _Bak)
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
}
