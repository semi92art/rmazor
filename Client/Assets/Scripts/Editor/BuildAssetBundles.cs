﻿using System.Collections.Generic;
using Exceptions;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using Utils.Editor;
using Debug = UnityEngine.Debug;

public static class BuildAssetBundles
{
    private const string BundlesLocalPath = "../bundles";
    private const string GitRepo = "https://semi92art:Anthony_1980@github.com/semi92art/bundles.git";
    private const string ProgressBarTitle = "Building Bundles";
    private static string BundlesPath => $"Assets/AssetBundles/{GetOsBundleSubPath()}";
    
    [MenuItem("Tools/Build Bundles")]
    public static void BuildAllAssetBundles()
    {
        EditorUtility.DisplayProgressBar(ProgressBarTitle, "Starting build...", 0f);
        if (!Directory.Exists(BundlesPath))
            Directory.CreateDirectory(BundlesPath);
        BuildPipeline.BuildAssetBundles(BundlesPath,
            BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayProgressBar(ProgressBarTitle, "Copy bundles to git folder...", 10f);
        CopyBundlesToGitFolder();
        
        EditorUtility.DisplayProgressBar(ProgressBarTitle, "Staging in git...", 20f);
        string unstagedFiles = GitUtils.RunGitCommand("ls-files --others --exclude-standard", BundlesLocalPath);
        if (!string.IsNullOrEmpty(unstagedFiles))
            Utils.Dbg.Log($"New Files: {unstagedFiles}");
        string modifiedFiles = GitUtils.RunGitCommand("diff --name-only", BundlesLocalPath);
        if (!string.IsNullOrEmpty(modifiedFiles))
            Utils.Dbg.Log($"Modified Files: {modifiedFiles}");
        var fileNames = BundleFileNames(unstagedFiles, modifiedFiles);
        var sb = new StringBuilder();
        foreach (var fileName in fileNames)
        {
            sb.Append(fileName);
            sb.Append(" ");
        }
        
        string fileNamesText = sb.ToString();
        if (string.IsNullOrEmpty(fileNamesText) || string.IsNullOrWhiteSpace(fileNamesText))
            Utils.Dbg.Log("No new bundles to push");

        GitUtils.RunGitCommand($"stage {fileNamesText}", BundlesLocalPath);
        EditorUtility.DisplayProgressBar(ProgressBarTitle, "Commit in git...", 50f);
        GitUtils.RunGitCommand("commit -m 'UnityBuild'", BundlesLocalPath);
        EditorUtility.DisplayProgressBar(ProgressBarTitle, "Pushing to remote repository...", 70f);
        GitUtils.RunGitCommand($"remote set-url origin {GitRepo}", BundlesLocalPath);
        GitUtils.RunGitCommand("push origin HEAD", BundlesLocalPath);
        EditorUtility.ClearProgressBar();
    }

    private static string GetOsBundleSubPath()
    {
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
            case BuildTarget.iOS:
                return EditorUserBuildSettings.activeBuildTarget.ToString();
            default:
                throw new SwitchCaseNotImplementedException(EditorUserBuildSettings.activeBuildTarget);
        }
    }

    private static IEnumerable<string> BundleFileNames(params string[] _FileNameLists)
    {
        var sb = new StringBuilder();
        foreach (var fileNameList in _FileNameLists)
            sb.Append(fileNameList);
        
        return sb.ToString().Split('\n')
            .Where(_Name => !string.IsNullOrEmpty(_Name))
            .Where(_Name => _Name.Contains(".unity3d"));
    }

    private static void CopyBundlesToGitFolder()
    {
        DirectoryInfo dInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
        var fileNames = Directory.GetFiles(BundlesPath, "*.*", SearchOption.AllDirectories);
        fileNames = fileNames
            .Where(_FileName => !_FileName.Contains(".meta"))
            .Where(_FileName => !_FileName.Contains(".manifest"))
            .Where(_FileName =>
            {
                var fInfo = new FileInfo(_FileName);
                return fInfo.Name != "Android" && fInfo.Name != "iOS";
            })
            .ToArray();
        var newFileNames = fileNames
            .Select(_FileName => _FileName.Replace("Assets/", string.Empty))
            .Select(_FileName => _FileName.Replace("AssetBundles/", string.Empty))
            .Select(_FileName => dInfo.Parent?.FullName + "/bundles/mgc/" + _FileName)
            .Select(_FileName => _FileName + ".unity3d")
            .ToArray();

        for (int i = 0; i < newFileNames.Length; i++)
        {
            var fInfo = new FileInfo(newFileNames[i]);
            if (!Directory.Exists(fInfo.DirectoryName) && !string.IsNullOrEmpty(fInfo.DirectoryName))
                Directory.CreateDirectory(fInfo.DirectoryName);
            string versionFileName = fInfo.FullName + ".version";
            if (!File.Exists(versionFileName))
                File.WriteAllText(versionFileName, "0");
            else
            {
                if (int.TryParse(File.ReadAllText(versionFileName), out int ver))
                    File.WriteAllText(versionFileName, $"{++ver}");
                else
                    Utils.Dbg.LogError($"Cannot read version from existing file {versionFileName}");
            }
            File.Copy(fileNames[i], newFileNames[i], true);
        }
    }
}

