using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Common.Managers
{
    public interface IAssetBundleManager : IInit
    {
        bool         BundlesLoaded { get; }
        T            GetAsset<T>(string _AssetName, string _BundleName) where T : Object;
    }

    public class BundleAssetPathInfo
    {
        [JsonProperty] public string BundleName { get; set; }
        [JsonProperty] public string AssetName  { get; set; }
        [JsonProperty] public string AssetPath  { get; set; }

        public BundleAssetPathInfo(string _BundleName, string _AssetName, string _AssetPath)
        {
            BundleName = _BundleName;
            AssetName  = _AssetName;
            AssetPath  = _AssetPath;
        }
    }
    
    public class AssetBundleManager : InitBase, IAssetBundleManager
    {
        #region types

        private class BundleAssetObjectInfo
        {
            public string Path  { get; }
            public object Asset { get; }

            public BundleAssetObjectInfo(string _Path, object _Asset)
            {
                Path = _Path;
                Asset = _Asset;
            }
        }

        #endregion
        
        #region constants

        public const  string BundleNamesAssetName = "bundle_names";
        public const  string CommonBundleName     = "common";
        private const string BundlesUri           = "https://raw.githubusercontent.com/semi92art/bundles/main/mgc";
        
        #endregion
        
        #region nonpublic members

        private readonly string[] m_BundleNames = {CommonBundleName, "sounds", "game_1_levels"};

        private readonly Dictionary<string, List<BundleAssetObjectInfo>> m_BundleAssetObjectInfos =
            new Dictionary<string, List<BundleAssetObjectInfo>>();

        private List<BundleAssetPathInfo> m_BundleAssetPathInfos = new List<BundleAssetPathInfo>();
        
        #endregion

        #region api

        public bool BundlesLoaded { get; private set; }

        public override void Init()
        {
            Cor.Run(LoadBundles());
        }

        public T GetAsset<T>(string _AssetName, string _BundleName) where T : Object
        {
            if (!BundlesLoaded)
            {
                Dbg.LogError("Bundles were not initialized");
                return default;
            }
            var objectInfos = m_BundleAssetObjectInfos.GetSafe(
                _BundleName, out bool containsKey);
            if (!containsKey)
            {
                Dbg.LogError($"Bundle with key \"{_BundleName}\" was not found in asset bundles dictionary");
                return default;
            }
            var pathInfo = m_BundleAssetPathInfos.FirstOrDefault(
                _I => _I.BundleName == _BundleName && _I.AssetName == _AssetName);
            if (pathInfo == null)
            {
                Dbg.LogError($"Asset name \"{_AssetName}\" was not found in asset names dictionary");
                return default;
            }
            string path = pathInfo.AssetPath;
            var objectInfo = objectInfos.FirstOrDefault(
                _I => _I.Path.EqualsIgnoreCase(path));
            if (objectInfo != null) 
                return objectInfo.Asset as T;
            Dbg.LogError($"Bundle path \"{path}\" was not found in bundles");
            return default;
        }
        
        #endregion

        #region nonpublic methods
        
        private IEnumerator LoadBundles()
        {
            foreach (string bundleName in m_BundleNames)
                yield return LoadBundle(bundleName);
            if (!m_BundleAssetObjectInfos.ContainsKey(CommonBundleName))
            {
                Dbg.LogWarning("Bundle with other bundle names was not loaded correctly.");
                base.Init();
                yield break;
            }
            var bundleNamesRaw =
                m_BundleAssetObjectInfos[CommonBundleName]
                    .FirstOrDefault(_Info => _Info.Path.Contains(BundleNamesAssetName))
                    ?.Asset as TextAsset;
            if (bundleNamesRaw == null)
            {
                Dbg.LogWarning("Bundle with other bundle names was not loaded correctly.");
                base.Init();
                yield break;
            }
            m_BundleAssetPathInfos = JsonConvert.DeserializeObject<List<BundleAssetPathInfo>>(bundleNamesRaw.text);
            BundlesLoaded = true;
            base.Init();
            Dbg.Log("Bundles initialized successfully!");
        }
        
        private IEnumerator LoadBundle(string _BundleName)
        {
            while (!Caching.ready)
                yield return null;
            string bundleVersionPath = $"{_BundleName}.unity3d.version";
            using var bundleVersionRequest = UnityWebRequest.Get(
                GetRemotePath(bundleVersionPath));
            yield return bundleVersionRequest.SendWebRequest();
            uint version;
            if (bundleVersionRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Dbg.LogWarning(bundleVersionRequest.error);
                version = GetCachedHash(bundleVersionPath);
            }
            else
            {
                uint.TryParse(bundleVersionRequest.downloadHandler.text, out version);
                PutCachedHash(bundleVersionPath, version);
            }
            string bundleName = $"{_BundleName}.unity3d";
            var cachedBundle = new CachedAssetBundle(bundleName, Hash128.Compute(version));
            var uri = new Uri(GetRemotePath($"{_BundleName}.unity3d"));
            using var bundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri, cachedBundle);
            yield return bundleRequest.SendWebRequest();
            if (bundleRequest.result != UnityWebRequest.Result.Success)
            {
                Dbg.LogWarning(bundleRequest.error);
                yield break;
            }
            var loadedBundle = DownloadHandlerAssetBundle.GetContent(bundleRequest);
            if (loadedBundle == null)
            {
                string error = $"Failed to load bundle {_BundleName} from remote server";
                Dbg.LogError(error);
                yield break;
            }
            m_BundleAssetObjectInfos.Add(_BundleName, new List<BundleAssetObjectInfo>());
            var cachedAssets = m_BundleAssetObjectInfos[_BundleName];
            cachedAssets.AddRange(from assetName in loadedBundle.GetAllAssetNames() 
                let asset = loadedBundle.LoadAsset(assetName) 
                select new BundleAssetObjectInfo(assetName, asset));
        }

        private static string GetRemotePath(string _Name)
        {
            return string.Join("/", BundlesUri, CommonUtils.GetOsName(), _Name);
        }

        private static uint GetCachedHash(string _BundleName)
        {
            var saveKey = SaveKeysCommon.BundleVersion(_BundleName);
            return SaveUtils.GetValue(saveKey);
        }

        private static void PutCachedHash(string _BundleName, uint _Hash)
        {
            var saveKey = SaveKeysCommon.BundleVersion(_BundleName);
            SaveUtils.PutValue(saveKey, _Hash);
        }

        #endregion
    }

    public class AssetBundleManagerFake : InitBase, IAssetBundleManager
    {
        public bool BundlesLoaded => true;

        public T GetAsset<T>(string _AssetName, string _BundleName) where T : Object
        {
            return null;
        }
    }
}