using System.Linq;
using UICreationSystem.Factories;
using UnityEngine;

namespace UICreationSystem
{
    public class PrefabInitializer
    {
        public static GameObject InitializeUiPrefab(
            RectTransform _RectTransform,
            string _Style,
            string _Prefab)
        {
            GameObject instance = InitializePrefabCore(_Style, _Prefab);
            UiFactory.CopyRTransform(_RectTransform, instance.RTransform());
            Object.Destroy(_RectTransform.gameObject);
            
            return instance;
        }

        public static GameObject InitializePrefab(
            Transform _Transform,
            string _Style,
            string _Prefab)
        {
            GameObject instance = InitializePrefabCore(_Style, _Prefab);
            UiFactory.CopyTransform(_Transform, instance.transform);
            Object.Destroy(_Transform.gameObject);

            return instance;
        }

        private static GameObject InitializePrefabCore(
            string _Style,
            string _Prefab)
        {
            UIStyleObject style = ResLoader.GetStyle(_Style);
            GameObject prefab = style.prefabs.FirstOrDefault(p => p.name == _Prefab).item;
            if (prefab == null)
            {
                Debug.LogError($"Prefab of style {_Style} with name {_Prefab} was not set");
                return null;
            }
            return Object.Instantiate(prefab);
        }
    }
}