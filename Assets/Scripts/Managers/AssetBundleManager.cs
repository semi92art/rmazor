using System;
using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using Object = UnityEngine.Object;

namespace Managers
{
    public class AssetBundleManager : MonoBehaviour
    {
        #region singleton
    
        private static AssetBundleManager _instance;

        public static AssetBundleManager Instance
        {
            get
            {
                if (_instance is AssetBundleManager ptm && !ptm.IsNull())
                    return _instance;
                var go = new GameObject("Asset Bundle Manager");
                _instance = go.AddComponent<AssetBundleManager>();
                if (!GameClient.Instance.IsTestMode)
                    DontDestroyOnLoad(go);
                return _instance;
            }
        }

        #endregion
        
        #region api
        
        public void Init() { } // only for calling IEnumerator Start()
        public bool Initialized { get; private set; }

        public T GetAsset<T>(string _AssetName, string _BundleName) where T : Object
        {
            if (typeof(T) == typeof(AudioClip))
                return m_Bundles[_BundleName].LoadAsset<T>(_AssetName);
            throw new NotImplementedException();
        }
        
        #endregion
        
        #region nonpublic members

        private readonly string[] m_BundleNames = {"sounds"};
        private const string BundlesUri = "https://raw.githubusercontent.com/semi92art/bundles/main/mgc";
        private readonly Dictionary<string, AssetBundle> m_Bundles = new Dictionary<string, AssetBundle>();
        
        #endregion
        
        #region engine methods

        private IEnumerator Start()
        {
            foreach (var bundleName in m_BundleNames)
                yield return LoadBundle(bundleName);
            Initialized = true;
        }
        
        #endregion

        #region private methods
        
        private IEnumerator LoadBundle(string _BundleName)
        {
            while (!Caching.ready)
                yield return null;
            
            using (var bundleVersionRequest = UnityWebRequest.Get(GetRemotePath($"{_BundleName}.unity3d.version")))
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
                    m_Bundles.Add(_BundleName, DownloadHandlerAssetBundle.GetContent(bundleRequest));
                    if (m_Bundles[_BundleName] == null)
                        Debug.LogError($"Failed to load bundle {_BundleName} from remote server");
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