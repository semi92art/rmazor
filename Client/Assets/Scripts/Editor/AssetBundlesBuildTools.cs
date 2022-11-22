using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;
using Common.Entities;
using Common.Exceptions;
using Common.Managers;
using Common.Utils;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AssetBundlesBuildTools
    {
        #region constants

        private const string ProgressBarTitle = "Building Bundles";
        private const string BundlesLocalPath = "../../bundles";
        private const string UserName         = "semi92art";
        private const string Token            = "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";
        private const string RepositoryName   = "bundles";
        
        #endregion

        #region nonpublic members
        
        private static string BundlesPath => $"Assets/AssetBundles/{GetOsBundleSubPath()}";
        private static string PushCommand => $"push https://{Token}@github.com/{UserName}/{RepositoryName}.git";

        #endregion

        #region api

        [MenuItem("Tools/Bundles/Build", false, 1)]
        public static void BuildBundles()
        {
            BuildBundles(false);
        }

        [MenuItem("Tools/Bundles/Copy To Git Folder and Commit", false, 2)]
        public static void CopyToGitFolderAndCommit()
        {
            CopyToGitFolder();
            CommitToGit();
        }
        
        [MenuItem("Tools/Bundles/Clear Cache", false, 13)]
        public static void ClearBundlesCache()
        {
            bool clearCacheSuccess = Caching.ClearCache();
            Dbg.Log(clearCacheSuccess ? "Successfully cleaned the cache" : "Cache is being used");
        }

        #endregion
        
        private static void CopyToGitFolder()
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
                .Select(_FileName => _FileName.Replace("Android", "android"))
                .Select(_FileName => _FileName.Replace("iOS", "ios"))
                .Select(_FileName =>
                {
                    return string.Join(
                        "/",
                        dInfo.Parent?.Parent?.FullName,
                        "/bundles/mgc/",
                        Application.version, _FileName) + ".unity3d";
                })
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
                        Dbg.LogError($"Cannot read version from existing file {versionFileName}");
                }
                File.Copy(fileNames[i], newFileNames[i], true);
            }
        }
        
        private static void CommitToGit()
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Staging in git...", 20f);
            string unstagedFiles = GitUtils.RunGitCommand("ls-files --others --exclude-standard", BundlesLocalPath);
            if (!string.IsNullOrEmpty(unstagedFiles))
                Dbg.Log($"New Files: {unstagedFiles}");
            string modifiedFiles = GitUtils.RunGitCommand("diff --name-only", BundlesLocalPath);
            if (!string.IsNullOrEmpty(modifiedFiles))
                Dbg.Log($"Modified Files: {modifiedFiles}");
            var fileNames = BundleFileNames(unstagedFiles, modifiedFiles);
            var sb = new StringBuilder();
            foreach (string fileName in fileNames)
            {
                sb.Append(fileName);
                sb.Append(" ");
            }
            string fileNamesText = sb.ToString();
            if (string.IsNullOrEmpty(fileNamesText) || string.IsNullOrWhiteSpace(fileNamesText))
                Dbg.Log("No new bundles to push");
            GitUtils.RunGitCommand($"stage {fileNamesText}", BundlesLocalPath);
            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Commit in git...", 50f);
            GitUtils.RunGitCommand("commit -m 'UnityBuild'", BundlesLocalPath);
            // EditorUtility.DisplayProgressBar(ProgressBarTitle, "Pushing to remote repository...", 70f);
            // GitUtils.RunGitCommand(PushCommand, BundlesLocalPath);
            EditorUtility.ClearProgressBar();
        }

        #region nonpublic methods
        
        private static void BuildBundles(bool _Forced)
        {
            CreateBundleNamesSetList();
            var options = BuildAssetBundleOptions.StrictMode;
            if (_Forced) options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            BuildAssetBundles(options);
        }
    
        private static void CreateBundleNamesSetList()
        {
            var bundleNamesSet = ResLoader.GetPrefabSet(AssetBundleManager.CommonBundleName);
            var assetInfos = new List<BundleAssetPathInfo>();
            var allPrefabSets = Resources
                .LoadAll<PrefabSetScriptableObject>(ResLoader.PrefabSetsLocalPath)
                .Where(_Set => !_Set.name.Contains(AssetBundleManager.CommonBundleName))
                .ToList();
            foreach (var prefabSet in allPrefabSets)
            {
                assetInfos
                    .AddRange(prefabSet.prefabs.Where(_P => _P.bundle)
                        .Select(_Prefab => new BundleAssetPathInfo(
                            prefabSet.name,
                            _Prefab.name, 
                            AssetDatabase.GetAssetPath(_Prefab.item).ToLower())));
            }
            string serialized = JsonConvert.SerializeObject(assetInfos);
            const string path = "Assets/Prefabs/bundle_names.asset";
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null)
            {
                asset = new TextAsset(serialized);
                AssetDatabase.CreateAsset(asset, path);
            }
            else if (asset.text != serialized)
            {
                File.WriteAllText(path, serialized);
                asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }
            var assetImporter = AssetImporter.GetAtPath(path);
            assetImporter.SetAssetBundleNameAndVariant(AssetBundleManager.CommonBundleName, null);
            var pref = bundleNamesSet.prefabs.FirstOrDefault(
                _Item => _Item.name == AssetBundleManager.BundleNamesAssetName);
            if (pref == null)
            {
                pref = new PrefabSetScriptableObject.Prefab
                {
                    name = "bundle_names",
                    item = asset
                };
                bundleNamesSet.prefabs.Add(pref);
            }
            else if (pref.item == null)
                pref.item = asset;
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(bundleNamesSet);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            Dbg.Log("Bundle names updated successfully!");
        }

        private static void BuildAssetBundles(BuildAssetBundleOptions _Options)
        {
            if (!Directory.Exists(BundlesPath))
                Directory.CreateDirectory(BundlesPath);
            BuildPipeline.BuildAssetBundles(
                BundlesPath,
                _Options,
                EditorUserBuildSettings.activeBuildTarget);
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
            foreach (string fileNameList in _FileNameLists)
                sb.Append(fileNameList);
            return sb.ToString().Split('\n')
                .Where(_Name => !string.IsNullOrEmpty(_Name))
                .Where(_Name => _Name.Contains(".unity3d"));
        }

        #endregion
    }
}

