using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Utils;

namespace Managers
{
    public static class AssetBundleManager
    {
        #region constants

        public const  string BundleNamesListName = "bundle_names";
        public const  string CommonBundleName    = "common";
        private const string SoundsBundleName    = "sounds";
        private const string LevelsBundleName    = "levels";
        private const string BundlesUri          = "https://raw.githubusercontent.com/semi92art/bundles/main/mgc";
        
        #endregion
        
        #region types

        private class AssetInfo
        {
            public string Name { get; }
            public object Asset { get; }

            public AssetInfo(string _Name, object _Asset)
            {
                Name = _Name;
                Asset = _Asset;
            }
        }
        
        #endregion
        
        #region nonpublic members

        private static readonly string[] BundleNames = {CommonBundleName, SoundsBundleName, LevelsBundleName};
        private static readonly Dictionary<string, List<AssetInfo>> Bundles = new Dictionary<string, List<AssetInfo>>();
        private static Dictionary<string, string> _bundleNamesDict = new Dictionary<string, string>();
        
        #endregion
        
        #region constructor

        static AssetBundleManager()
        {
            Coroutines.Run(LoadBundles());
        }
        
        #endregion

        #region api

        public static bool BundlesLoaded { get; private set; }
        public static List<string> Errors { get; } = new List<string>();

        public static T GetAsset<T>(string _AssetName, string _PrefabSetName) where T : Object
        {
            if (!BundlesLoaded)
                Dbg.LogError("Bundles were not initialized");
            if (!_bundleNamesDict.ContainsKey(_AssetName))
            {
                Dbg.LogError($"Bundle key \"{_AssetName}\" was not found in bundles names dictionary");
                return null;
            }
            string bundleName = _bundleNamesDict[_AssetName];
            var dict = Bundles[_PrefabSetName]
                .ToDictionary(
                    _Bundle => _Bundle.Name,
                    _Bundle => _Bundle.Asset);
            if (!dict.ContainsKey(bundleName))
            {
                Dbg.LogError($"Bundle name \"{bundleName}\" was not found in bundles");
                return null;
            }
            var result = dict[bundleName];
            return result as T;
        }
        
        #endregion

        #region nonpublic methods
        
        private static IEnumerator LoadBundles()
        {
            foreach (var bundleName in BundleNames)
                yield return LoadBundle(bundleName);
            var bundleNamesRaw =
                Bundles[CommonBundleName]
                    .FirstOrDefault(_Info => _Info.Name.Contains(BundleNamesListName))
                    ?.Asset as TextAsset;
            if (bundleNamesRaw == null)
            {
                Dbg.LogError("Bundle with other bundle names was not loaded correctly.");
                yield break;
            }
            _bundleNamesDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(bundleNamesRaw.text);
            BundlesLoaded = true;
            Dbg.Log("Bundles initialized!");
        }
        
        private static IEnumerator LoadBundle(string _BundleName)
        {
            while (!Caching.ready)
                yield return null;
            
            using (var bundleVersionRequest = UnityWebRequest.Get(
                GetRemotePath($"{_BundleName}.unity3d.version")))
            {
                yield return bundleVersionRequest.SendWebRequest();
                string version = bundleVersionRequest.downloadHandler.text;
                if (bundleVersionRequest.isHttpError || bundleVersionRequest.isNetworkError)
                {
                    Dbg.LogError(bundleVersionRequest.error);
                    yield break;
                }
                using (var bundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(
                    GetRemotePath($"{_BundleName}.unity3d"), Hash128.Compute(version)))
                {
                    yield return bundleRequest.SendWebRequest();
                    var loadedBundle = DownloadHandlerAssetBundle.GetContent(bundleRequest);

                    if (loadedBundle == null)
                    {
                        string error = $"Failed to load bundle {_BundleName} from remote server";
                        Errors.Add(error);
                        Dbg.LogError(error);
                        yield break;
                    }
                    
                    Bundles.Add(_BundleName, new List<AssetInfo>());
                    var cachedAssets = Bundles[_BundleName];
                    cachedAssets.AddRange(from assetName in loadedBundle.GetAllAssetNames() 
                        let asset = loadedBundle.LoadAsset(assetName) 
                        select new AssetInfo(assetName, asset));
                }
            }
        }

        private static string GetRemotePath(string _Name)
        {
            return string.Join("/", BundlesUri, CommonUtils.GetOsName(), _Name);
        }

        #endregion
    }
}