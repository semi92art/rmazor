using System.Collections.Generic;
using System.Linq;
using Extensions;
using Managers;
using UI.Entities;
using UI.Factories;
using UnityEditor;
using UnityEngine;
using Utils;

namespace GameHelpers
{
    public static class PrefabInitializer
    {
        public static GameObject GetPrefab(string _Style, string _PrefabName)
        {
            return GetPrefabBase(_Style, _PrefabName);
        }

        public static void SetPrefab(string _Style, string _PrefabName, GameObject _GameObject)
        {
            UIStyleObject style = ResLoader.GetStyle(_Style);
            if (style == null)
            {
                style = ScriptableObject.CreateInstance<UIStyleObject>();
                AssetDatabase.CreateAsset(style, $"Assets/Resources/{_Style}.asset");
            }

            var perfabsList = style.prefabs;
            var prefab = perfabsList.FirstOrDefault(_P => _P.name == _PrefabName);
            if (prefab == null)
            {
                perfabsList.Add(new Prefab
                {
                    item = _GameObject,
                    name =  _PrefabName
                });
            }
            else
            {
                int idx = perfabsList.IndexOf(prefab);
                perfabsList[idx].item = _GameObject;
            } 
            AssetDatabase.SaveAssets();
        }

        public static List<Prefab> GetAllPrefabs(string _Style)
        {
            UIStyleObject style = ResLoader.GetStyle(_Style);
            return style.prefabs.ToList();
        }
        
        public static GameObject InitUiPrefab(
            RectTransform _RectTransform,
            string _Style,
            string _PrefabName)
        {
            GameObject instance = GetPrefabBase(_Style, _PrefabName);
            UiFactory.CopyRTransform(_RectTransform, instance.RTransform());
            Object.Destroy(_RectTransform.gameObject);
            instance.RTransform().localScale = Vector3.one;
            return instance;
        }

        public static GameObject InitPrefab(
            Transform _Parent,
            string _Style,
            string _PrefabName)
        {
            GameObject instance = GetPrefabBase(_Style, _PrefabName);
            if (_Parent != null)
                instance.transform.SetParent(_Parent);
            
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        private static GameObject GetPrefabBase(
            string _Style,
            string _PrefabName,
            bool _Instantiate = true)
        {
            UIStyleObject style = ResLoader.GetStyle(_Style);
            GameObject prefab = style.prefabs.FirstOrDefault(p => p.name == _PrefabName).item as GameObject;
            
            if (prefab == null)
            {
                Debug.LogError($"Prefab of style {_Style} with name {_PrefabName} was not set");
                return null;
            }

            GameObject instance = _Instantiate ? Object.Instantiate(prefab) : prefab;
            if (_Instantiate)
                instance.name = instance.name.Replace("(Clone)", string.Empty);
            return instance;
        }
        
        public static T GetObject<T>(
            string _Style,
            string _Name) where T : Object
        {
            UIStyleObject style = ResLoader.GetStyle(_Style);
            T content = style.prefabs.FirstOrDefault(_P => _P.name == _Name)?.item as T;

            if (content == null)
                content = AssetBundleManager.Instance.GetAsset<T>(_Name, _Style);
            
            if (content == null)
                Debug.LogError($"Content of style {_Style} with name {_Name} was not set");
            
            return content;
        }
    }
}