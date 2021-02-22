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
        public static GameObject GetPrefab(string _StyleName, string _PrefabName)
        {
            return GetPrefabBase(_StyleName, _PrefabName);
        }

#if UNITY_EDITOR

        public static void SetPrefab(string _StyleName, string _PrefabName, Object _Prefab)
        {
            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            if (style == null)
                style = ResLoader.CreateStyleIfNotExist(_StyleName);
            var perfabsList = style.prefabs;
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

        public static List<Prefab> GetAllPrefabs(string _StyleName)
        {
            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            return style.prefabs.ToList();
        }
        
        public static GameObject InitUiPrefab(
            RectTransform _RectTransform,
            string _StyleName,
            string _PrefabName)
        {
            GameObject instance = GetPrefabBase(_StyleName, _PrefabName);
            UiFactory.CopyRTransform(_RectTransform, instance.RTransform());
            Object.Destroy(_RectTransform.gameObject);
            instance.RTransform().localScale = Vector3.one;
            return instance;
        }

        public static GameObject InitPrefab(
            Transform _Parent,
            string _StyleName,
            string _PrefabName)
        {
            GameObject instance = GetPrefabBase(_StyleName, _PrefabName);
            if (_Parent != null)
                instance.transform.SetParent(_Parent);
            
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        public static T GetObject<T>(
            string _StyleName,
            string _ObjectName) where T : Object
        {
            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            T content = style.prefabs.FirstOrDefault(_P => _P.name == _ObjectName)?.item as T;

            if (content == null)
                content = AssetBundleManager.Instance.GetAsset<T>(_ObjectName, _StyleName);
            
            if (content == null)
                Debug.LogError($"Content of style {_StyleName} with name {_ObjectName} was not set");
            
            return content;
        }
        
        private static GameObject GetPrefabBase(
            string _StyleName,
            string _PrefabName,
            bool _Instantiate = true)
        {
            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            GameObject prefab = style.prefabs.FirstOrDefault(p => p.name == _PrefabName).item as GameObject;
            
            if (prefab == null)
            {
                Debug.LogError($"Prefab of style {_StyleName} with name {_PrefabName} was not set");
                return null;
            }

            GameObject instance = _Instantiate ? Object.Instantiate(prefab) : prefab;
            if (_Instantiate)
                instance.name = instance.name.Replace("(Clone)", string.Empty);
            return instance;
        }
    }
}