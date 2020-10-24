using System.Linq;
using UICreationSystem.Factories;
using UnityEngine;

namespace UICreationSystem
{
    public static class PrefabInitializer
    {
        public static GameObject InitUiPrefab(
            RectTransform _RectTransform,
            string _Style,
            string _Prefab)
        {
            GameObject instance = InitPrefabBase(_Style, _Prefab);
            instance.name = instance.name.Replace("(Clone)", string.Empty);
            UiFactory.CopyRTransform(_RectTransform, instance.RTransform());
            Object.Destroy(_RectTransform.gameObject);
            instance.RTransform().localScale = Vector3.one;
            return instance;
        }

        public static GameObject InitPrefab(
            Transform _Transform,
            string _Style,
            string _Prefab)
        {
            GameObject instance = InitPrefabBase(_Style, _Prefab);
            UiFactory.CopyTransform(_Transform, instance.transform);
            Object.Destroy(_Transform.gameObject);
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        private static GameObject InitPrefabBase(
            string _Style,
            string _Prefab)
        {
            UIStyleObject style = ResLoader.GetStyle(_Style);
            GameObject prefab = style.prefabs.FirstOrDefault(p => p.name == _Prefab).item as GameObject;
            if (prefab == null)
            {
                Debug.LogError($"Prefab of style {_Style} with name {_Prefab} was not set");
                return null;
            }
            return Object.Instantiate(prefab);
        }
        
        public static T GetObject<T>(
            string _Style,
            string _Name) where T : Object
        {
            UIStyleObject style = ResLoader.GetStyle(_Style);
            T content = style.prefabs.FirstOrDefault(p => p.name == _Name).item as T;

            if (content == null)
                Debug.LogError($"Content of style {_Style} with name {_Name} was not set");
            
            return content;
        }
    }
}