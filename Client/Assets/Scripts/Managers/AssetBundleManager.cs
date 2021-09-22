﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

namespace Managers
{
    public class AssetBundleManager : MonoBehaviour, IInit
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
        
        #region nonpublic members

        private bool m_Started;
        
        #endregion
        
        #region api

        public void Init()
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !m_Started,
                () => Coroutines.Run(LoadBundles())
            ));
        }
        public event NoArgsHandler Initialized; // only for calling IEnumerator Start()
        public bool BundlesLoaded { get; private set; }
        public List<string> Errors { get; } = new List<string>();

        public T GetAsset<T>(string _AssetName, string _BundleName) where T : Object
        {
            if (!BundlesLoaded)
                Dbg.LogError("Bundles were not initialized");
            
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

        private void Start() => m_Started = true;

        #endregion

        #region nonpublic methods
        
        private IEnumerator LoadBundles()
        {
            foreach (var bundleName in m_BundleNames)
                yield return LoadBundle(bundleName);
            BundlesLoaded = true;
            Initialized?.Invoke();
        }
        
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
                    
                    m_Bundles.Add(_BundleName, new List<AssetInfo>());
                    var cachedAssets = m_Bundles[_BundleName];
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