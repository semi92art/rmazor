using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Network;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using Object = UnityEngine.Object;

namespace Managers
{
    public class AssetBundleManager : MonoBehaviour, ISingleton
    {
        #region constants

        private const string SoundsBundle = "sounds";
        private const string LevelsBundle = "levels";
        
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
        
        #region singleton
    
        private static AssetBundleManager _instance;
        public static AssetBundleManager Instance => CommonUtils.MonoBehSingleton(ref _instance, "Asset Bundle Manager");

        #endregion
        
        #region api
        
        public void Init() { } // only for calling IEnumerator Start()
        public bool Initialized { get; private set; }
        public List<string> Errors { get; } = new List<string>();

        public T GetAsset<T>(string _AssetName, string _BundleName) where T : Object
        {
            return m_Bundles[_BundleName].FirstOrDefault(
                _Info => _Info.Name.GetFileName(false) == _AssetName)?.Asset as T;
        }
        
        #endregion
        
        #region nonpublic members
        
        private readonly string[] m_BundleNames = {SoundsBundle};//, LevelsBundle};
        private const string BundlesUri = "https://raw.githubusercontent.com/semi92art/bundles/main/mgc";
        private readonly Dictionary<string, List<AssetInfo>> m_Bundles = new Dictionary<string, List<AssetInfo>>();
        
        #endregion
        
        #region engine methods

        private IEnumerator Start()
        {
            foreach (var bundleName in m_BundleNames)
                yield return LoadBundle(bundleName);
            Initialized = true;
        }
        
        #endregion

        #region nonpublic methods
        
        private IEnumerator LoadBundle(string _BundleName)
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
                    Debug.LogError(bundleVersionRequest.error);
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
                        Debug.LogError(error);
                        yield break;
                    }
                    
                    m_Bundles.Add(_BundleName, new List<AssetInfo>());
                    var cachedAssets = m_Bundles[_BundleName];
                    cachedAssets.AddRange(from assetName in loadedBundle.GetAllAssetNames() 
                        let asset = loadedBundle.LoadAsset(assetName) 
                        select new AssetInfo(assetName, asset));
                }
            }
        }

        private string GetRemotePath(string _Name)
        {
            return string.Join("/", BundlesUri, CommonUtils.GetOsName(), _Name);
        }

        #endregion
    }
}