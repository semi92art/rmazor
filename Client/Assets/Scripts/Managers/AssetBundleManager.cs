using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Utils;
using Object = UnityEngine.Object;

namespace Managers
{
    public interface IAssetBundleManager : IInit
    {
        bool         BundlesLoaded { get; }
        T            GetAsset<T>(string _AssetName, string _PrefabSetName, out bool _Success) where T : Object;
    }
    
    public class AssetBundleManager : IAssetBundleManager
    {
        #region types

        private class AssetInfo
        {
            public string Name  { get; }
            public object Asset { get; }

            public AssetInfo(string _Name, object _Asset)
            {
                Name = _Name;
                Asset = _Asset;
            }
        }
        
        #endregion
        
        #region constants

        public const  string BundleNamesListName = "bundle_names";
        public const  string CommonBundleName    = "common";
        private const string SoundsBundleName    = "sounds";
        private const string LevelsBundleName    = "levels";
        private const string BundlesUri          = "https://raw.githubusercontent.com/semi92art/bundles/main/mgc";
        
        #endregion
        
        #region nonpublic members

        private readonly string[] m_BundleNames = {CommonBundleName, SoundsBundleName, LevelsBundleName};
        private readonly Dictionary<string, List<AssetInfo>> m_Bundles = new Dictionary<string, List<AssetInfo>>();
        private Dictionary<string, string> m_BundleNamesDict = new Dictionary<string, string>();
        
        #endregion

        #region api

        public bool              BundlesLoaded { get; private set; }
        public bool              Initialized   { get; private set; }
        public event UnityAction Initialize;
        
        public void Init()
        {
            Cor.Run(LoadBundles());
            Initialize?.Invoke();
            Initialized = true;
        }

        public T GetAsset<T>(string _AssetName, string _PrefabSetName, out bool _Success) where T : Object
        {
            if (!BundlesLoaded)
            {
                Dbg.LogError("Bundles were not initialized");
                _Success = false;
                return default;
            }
            if (!m_BundleNamesDict.ContainsKey(_AssetName))
            {
                Dbg.LogError($"Bundle key \"{_AssetName}\" was not found in bundles names dictionary");
                _Success = false;
                return default;
            }
            string bundleName = m_BundleNamesDict[_AssetName];
            var dict = m_Bundles[_PrefabSetName]
                .ToDictionary(
                    _Bundle => _Bundle.Name,
                    _Bundle => _Bundle.Asset);
            if (!dict.ContainsKey(bundleName))
            {
                Dbg.LogError($"Bundle name \"{bundleName}\" was not found in bundles");
                _Success = false;
                return default;
            }
            _Success = true;
            return dict[bundleName] as T;
        }
        
        #endregion

        #region nonpublic methods
        
        private IEnumerator LoadBundles()
        {
            foreach (var bundleName in m_BundleNames)
                yield return LoadBundle(bundleName);
            var bundleNamesRaw =
                m_Bundles[CommonBundleName]
                    .FirstOrDefault(_Info => _Info.Name.Contains(BundleNamesListName))
                    ?.Asset as TextAsset;
            if (bundleNamesRaw == null)
            {
                Dbg.LogError("Bundle with other bundle names was not loaded correctly.");
                yield break;
            }
            m_BundleNamesDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(bundleNamesRaw.text);
            BundlesLoaded = true;
            Dbg.Log("Bundles initialized!");
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
                version = GetCahcedHash(bundleVersionPath);
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
            var loadedBundle = DownloadHandlerAssetBundle.GetContent(bundleRequest);
            if (loadedBundle == null)
            {
                string error = $"Failed to load bundle {_BundleName} from remote server";
                Dbg.LogError(error);
                yield break;
            }
            m_Bundles.Add(_BundleName, new List<AssetInfo>());
            var cachedAssets = m_Bundles[_BundleName];
            cachedAssets.AddRange(from assetName in loadedBundle.GetAllAssetNames() 
                let asset = loadedBundle.LoadAsset(assetName) 
                select new AssetInfo(assetName, asset));
        }

        private static string GetRemotePath(string _Name)
        {
            return string.Join("/", BundlesUri, CommonUtils.GetOsName(), _Name);
        }

        private static uint GetCahcedHash(string _BundleName)
        {
            var saveKey = SaveKeys.BundleVersion(_BundleName);
            return SaveUtils.GetValue(saveKey);
        }

        private static void PutCachedHash(string _BundleName, uint _Hash)
        {
            var saveKey = SaveKeys.BundleVersion(_BundleName);
            SaveUtils.PutValue(saveKey, _Hash);
        }

        #endregion
    }

    public class AssetBundleManagerFake : IAssetBundleManager
    {
        public bool              BundlesLoaded => false;
        public List<string>      Errors        => null;
        public bool              Initialized   { get; private set; }
        public event UnityAction Initialize;
        
        public void Init()
        {
            Initialize?.Invoke();
            Initialized = true;
        }

        public T GetAsset<T>(string _AssetName, string _PrefabSetName, out bool _Success) where T : Object
        {
            throw new NotSupportedException();
        }
    }
}