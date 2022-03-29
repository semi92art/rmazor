using System.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using UnityEditor;
using UnityEngine;

namespace Common.Managers
{
    public class PrefabEntity<T> : Entity<T> where T : Object { }
    
    public interface IPrefabSetManager
    {
        GameObject GetPrefab(string _PrefabSetName, string _PrefabName);
#if UNITY_EDITOR
        void SetPrefab(string _PrefabSetName, string _PrefabName, Object _Prefab);
#endif
        GameObject      InitPrefab(Transform       _Parent,        string _PrefabSetName, string _PrefabName);
        GameObject      InitUiPrefab(RectTransform _RectTransform, string _PrefabSetName, string _PrefabName);
        T               GetObject<T>(string        _PrefabSetName, string _ObjectName) where T : Object;
        T               InitObject<T>(string       _PrefabSetName, string _ObjectName) where T : Object;
        PrefabEntity<T> GetObjectEntity<T>(string  _PrefabSetName, string _ObjectName) where T : Object;
    }
    
    public class PrefabSetManager : IPrefabSetManager
    {
        private IAssetBundleManager AssetBundleManager { get; }

        #region inject

        public PrefabSetManager(IAssetBundleManager _AssetBundleManager)
        {
            AssetBundleManager = _AssetBundleManager;
        }   

        #endregion
        
        #region api

        public GameObject GetPrefab(string _PrefabSetName, string _PrefabName)
        {
            return GetPrefabBase(_PrefabSetName, _PrefabName);
        }

#if UNITY_EDITOR

        public void SetPrefab(string _PrefabSetName, string _PrefabName, Object _Prefab)
        {
            var set = ResLoader.GetPrefabSet(_PrefabSetName);
            if (set == null)
                set = ResLoader.CreatePrefabSetIfNotExist(_PrefabSetName);
            var perfabsList = set.prefabs;
            var prefab = perfabsList.FirstOrDefault(_P => _P.name == _PrefabName);
            if (prefab == null)
            {
                perfabsList.Add(new PrefabSetScriptableObject.Prefab
                {
                    item = _Prefab,
                    name =  _PrefabName
                });
            }
            else
            {
                int idx = perfabsList.IndexOf(prefab);
                perfabsList[idx].item = _Prefab;
            } 
            AssetDatabase.SaveAssets();
        }
#endif
        
        public GameObject InitUiPrefab(
            RectTransform _RectTransform,
            string _PrefabSetName,
            string _PrefabName)
        {
            var instance = GetPrefabBase(_PrefabSetName, _PrefabName);
            CopyRTransform(_RectTransform, instance.RTransform());
            Object.Destroy(_RectTransform.gameObject);
            instance.RTransform().localScale = Vector3.one;
            return instance;
        }

        public GameObject InitPrefab(
            Transform _Parent,
            string _PrefabSetName,
            string _PrefabName)
        {
            var instance = GetPrefabBase(_PrefabSetName, _PrefabName);
            if (instance == null)
                return instance;
            if (_Parent != null)
                instance.transform.SetParent(_Parent);
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        public T GetObject<T>(
            string _PrefabSetName,
            string _ObjectName) where T : Object
        {
            var set = ResLoader.GetPrefabSet(_PrefabSetName);
            if (set == null)
                return default;
            var prefab = set.prefabs.FirstOrDefault(_P => _P.name == _ObjectName);
            if (prefab == null)
            {
                Dbg.LogError($"Content of set \"{_PrefabSetName}\" " +
                             $"with name \"{_ObjectName}\" was not set");
                return default;
            }
            bool realSuccess = false;
            T content = prefab.bundle ? 
                AssetBundleManager.GetAsset<T>(_ObjectName, _PrefabSetName, out realSuccess) : prefab.item as T;
            if (!prefab.bundle)
                realSuccess = content != null;
            if (!realSuccess)
            {
                Dbg.LogError($"Content of set \"{_PrefabSetName}\" " +
                             $"with name \"{_ObjectName}\" was not set, bundles: {prefab.bundle}");
            }
            return content;
        }

        public T InitObject<T>(
            string _PrefabSetName,
            string _ObjectName) where T : Object
        {
            var @object = GetObject<T>(_PrefabSetName, _ObjectName);
            return Object.Instantiate(@object);
        }

        public PrefabEntity<T> GetObjectEntity<T>(string _PrefabSetName, string _ObjectName) where T : Object
        {
            var entity = new PrefabEntity<T>();
            var set = ResLoader.GetPrefabSet(_PrefabSetName);
            if (set == null)
            {
                entity.Result = EEntityResult.Fail;
                return entity;
            }
            var prefab = set.prefabs.FirstOrDefault(_P => _P.name == _ObjectName);
            if (prefab == null)
            {
                Dbg.LogError($"Content of set \"{_PrefabSetName}\" " +
                             $"with name \"{_ObjectName}\" was not set");
                entity.Result = EEntityResult.Fail;
                return entity;
            }

            if (!prefab.bundle)
            {
                entity.Value = prefab.item as T;
                entity.Result = EEntityResult.Success;
                return entity;
            }
            
            Cor.Run(Cor.WaitWhile(
                () => !AssetBundleManager.BundlesLoaded,
                () =>
                {
                    entity.Value = AssetBundleManager.GetAsset<T>(_ObjectName, _PrefabSetName, out bool success);
                    entity.Result = success ? EEntityResult.Success : EEntityResult.Fail;
                }));
            return entity;
        }

        #endregion

        #region nonpublic methods

        private static GameObject GetPrefabBase(
            string _PrefabSetName,
            string _PrefabName,
            bool _Instantiate = true)
        {
            var setScriptable = ResLoader.GetPrefabSet(_PrefabSetName);
            var prefab = setScriptable.prefabs.FirstOrDefault(_P => _P.name == _PrefabName)?.item as GameObject;
            
            if (prefab == null)
            {
                Dbg.LogError($"Prefab of set {_PrefabSetName} with name {_PrefabName} was not set");
                return null;
            }

            var instance = _Instantiate ? Object.Instantiate(prefab) : prefab;
            if (_Instantiate)
                instance.name = instance.name.Replace("(Clone)", string.Empty);
            return instance;
        }
        
        private static void CopyRTransform(RectTransform _From, RectTransform _To)
        {
            _To.SetParent(_From.parent);
            _To.anchorMin = _From.anchorMin;
            _To.anchorMax = _From.anchorMax;
            _To.anchoredPosition = _From.anchoredPosition;
            _To.pivot = _From.pivot;
            _To.sizeDelta = _From.sizeDelta;
        }

        #endregion
        
    }
}