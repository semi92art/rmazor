using System;
using System.Linq;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Managers
{
    public class PrefabEntity<T> : Entity<T> where T : Object { }

    public enum EPrefabSource
    {
        Asset,
        Bundle
    }
    
    public interface IPrefabSetManager
    {
        GameObject GetPrefab(string _PrefabSetName, string _PrefabName);
#if UNITY_EDITOR
        void SetPrefab(string _PrefabSetName, string _PrefabName, Object _Prefab);
#endif
        GameObject      InitPrefab(Transform       _Parent,        string _PrefabSetName, string _PrefabName);
        GameObject      InitUiPrefab(RectTransform _RectTransform, string _PrefabSetName, string _PrefabName);
        T               GetObject<T>(string        _PrefabSetName, string _ObjectName, EPrefabSource _Source = EPrefabSource.Asset) where T : Object;
        T               InitObject<T>(string       _PrefabSetName, string _ObjectName, EPrefabSource _Source = EPrefabSource.Asset) where T : Object;
        PrefabEntity<T> GetObjectEntity<T>(string  _PrefabSetName, string _ObjectName, EPrefabSource _Source = EPrefabSource.Asset) where T : Object;
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
            string        _PrefabSetName,
            string        _ObjectName,
            EPrefabSource _Source = EPrefabSource.Asset)
            where T : Object
        {
            var set = ResLoader.GetPrefabSet(_PrefabSetName);
            if (set == null)
            {
                LogErrorPrefabNotSet(_PrefabSetName);
                return null;
            }
            var prefab = set.prefabs.FirstOrDefault(_P => _P.name == _ObjectName);
            if (prefab == null)
            {
                LogErrorPrefabNotSet(_PrefabSetName, _ObjectName);
                return null;
            }
            T content = _Source switch
            {
                EPrefabSource.Asset  => prefab.item as T,
                EPrefabSource.Bundle => prefab.bundle
                    ? AssetBundleManager.GetAsset<T>(_ObjectName, _PrefabSetName)
                    : prefab.item as T,
                _ => throw new SwitchCaseNotImplementedException(_Source)
            };
            bool Success() => content != null;
            if (_Source == EPrefabSource.Bundle && prefab.bundle && !Success())
                content = prefab.item as T;
            if (!Success())
                LogErrorPrefabNotSet(_PrefabSetName, _ObjectName, prefab.bundle);
            return content;
        }

        public T InitObject<T>(
            string        _PrefabSetName,
            string        _ObjectName,
            EPrefabSource _Source = EPrefabSource.Asset)
            where T : Object
        {
            var @object = GetObject<T>(_PrefabSetName, _ObjectName, _Source);
            return Object.Instantiate(@object);
        }

        public PrefabEntity<T> GetObjectEntity<T>(
            string        _PrefabSetName,
            string        _ObjectName,
            EPrefabSource _Source = EPrefabSource.Asset)
            where T : Object
        {
            var entity = new PrefabEntity<T>();
            var set = ResLoader.GetPrefabSet(_PrefabSetName);
            if (set == null)
            {
                LogErrorPrefabNotSet(_PrefabSetName);
                entity.Result = EEntityResult.Fail;
                return entity;
            }
            var prefab = set.prefabs.FirstOrDefault(_P => _P.name == _ObjectName);
            if (prefab == null)
            {
                LogErrorPrefabNotSet(_PrefabSetName, _ObjectName);
                entity.Result = EEntityResult.Fail;
                return entity;
            }
            switch (_Source)
            {
                case EPrefabSource.Asset:
                    if (prefab.item == null)
                        LogErrorPrefabNotSet(_PrefabSetName, _ObjectName, false);
                    entity.Value = prefab.item as T;
                    entity.Result = prefab.item != null ? EEntityResult.Success : EEntityResult.Fail;
                    return entity;
                case EPrefabSource.Bundle:
                    Cor.Run(Cor.WaitWhile(
                        () => !AssetBundleManager.BundlesLoaded,
                        () =>
                        {
                            entity.Value = AssetBundleManager.GetAsset<T>(_ObjectName, _PrefabSetName);
                            if (entity.Value != null) 
                                return;
                            entity.Value = prefab.item as T;
                            entity.Result = prefab.item != null ? EEntityResult.Success : EEntityResult.Fail;
                            if (entity.Value == null)
                                LogErrorPrefabNotSet(_PrefabSetName, _ObjectName, false);
                        }));
                    return entity;
                default:
                    throw new SwitchCaseNotImplementedException(_Source);
            }
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

        private static void LogErrorPrefabNotSet(string _PrefabSetName, string _ObjectName = null, bool? _Bundle = null)
        {
            string errorString = string.IsNullOrEmpty(_ObjectName)
                ? $"Prefab set with name {_PrefabSetName} was not found."
                : $"Content of set \"{_PrefabSetName}\" " +
                  $"with name \"{_ObjectName}\" was not set"
                  + (_Bundle.HasValue ? $", bundle: {_Bundle}" : string.Empty) + ".";
            Dbg.LogError(errorString);
        }
        

        #endregion
        
    }
}