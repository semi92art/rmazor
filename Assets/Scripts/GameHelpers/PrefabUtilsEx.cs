using System.Collections.Generic;
using System.Linq;
using Extensions;
using Managers;
using UI.Entities;
using UI.Factories;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameHelpers
{
    public static class PrefabUtilsEx
    {
        public static GameObject GetPrefab(string _PrefabSetName, string _PrefabName)
        {
            return GetPrefabBase(_PrefabSetName, _PrefabName);
        }

#if UNITY_EDITOR

        public static void SetPrefab(string _PrefabSetName, string _PrefabName, Object _Prefab)
        {
            var set = ResLoader.GetPrefabSet(_PrefabSetName);
            if (set == null)
                set = ResLoader.CreatePrefabSetIfNotExist(_PrefabSetName);
            var perfabsList = set.prefabs;
            var prefab = perfabsList.FirstOrDefault(_P => _P.name == _PrefabName);
            if (prefab == null)
            {
                perfabsList.Add(new Prefab
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

        public static List<Prefab> GetAllPrefabs(string _PrefabSetName)
        {
            PrefabSetObject set = ResLoader.GetPrefabSet(_PrefabSetName);
            return set.prefabs.ToList();
        }
        
        public static GameObject InitUiPrefab(
            RectTransform _RectTransform,
            string _PrefabSetName,
            string _PrefabName)
        {
            GameObject instance = GetPrefabBase(_PrefabSetName, _PrefabName);
            UiFactory.CopyRTransform(_RectTransform, instance.RTransform());
            Object.Destroy(_RectTransform.gameObject);
            instance.RTransform().localScale = Vector3.one;
            return instance;
        }

        public static GameObject InitPrefab(
            Transform _Parent,
            string _PrefabSetName,
            string _PrefabName)
        {
            GameObject instance = GetPrefabBase(_PrefabSetName, _PrefabName);
            if (_Parent != null)
                instance.transform.SetParent(_Parent);
            
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        public static T GetObject<T>(
            string _PrefabSetName,
            string _ObjectName,
            bool _FromBundles = true) where T : Object
        {
            PrefabSetObject set = ResLoader.GetPrefabSet(_PrefabSetName);
            T content = set.prefabs.FirstOrDefault(_P => _P.name == _ObjectName)?.item as T;

            if (content == null && _FromBundles)
                content = AssetBundleManager.Instance.GetAsset<T>(_ObjectName, _PrefabSetName);
            
            if (content == null)
                Dbg.LogError($"Content of set {_PrefabSetName} with name {_ObjectName} was not set");
            
            return content;
        }
        
        private static GameObject GetPrefabBase(
            string _PrefabSetName,
            string _PrefabName,
            bool _Instantiate = true)
        {
            PrefabSetObject set = ResLoader.GetPrefabSet(_PrefabSetName);
            GameObject prefab = set.prefabs.FirstOrDefault(p => p.name == _PrefabName).item as GameObject;
            
            if (prefab == null)
            {
                Dbg.LogError($"Prefab of set {_PrefabSetName} with name {_PrefabName} was not set");
                return null;
            }

            GameObject instance = _Instantiate ? Object.Instantiate(prefab) : prefab;
            if (_Instantiate)
                instance.name = instance.name.Replace("(Clone)", string.Empty);
            return instance;
        }
    }
}