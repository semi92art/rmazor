using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

namespace Extensions
{
    public static class ObjectExtensions
    {
        public static T AddComponentIfNotExist<T>(this GameObject _Object) where T : Component
        {
            return _Object.GetComponent<T>() == null ? _Object.AddComponent<T>() : null;
        }

        public static void RemoveComponentIfExist<T>(this GameObject _Object) where T : Component
        {
            var component = _Object.GetComponent<T>();
            if (component != null)
                Object.Destroy(component);
        }
        
        //https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html?_ga=2.165477879.911525322.1599381767-2044961467.1583736117
        public static T GetCopy<T>(this T _Item) where T : Object
        {
            string serialized = JsonConvert.SerializeObject(_Item);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static void SetParent(this GameObject _Object, GameObject _Parent)
        {
            _Object.transform.SetParent(_Parent.transform);
        }

        public static void SetParent(this GameObject _Object, Transform _Parent)
        {
            _Object.transform.SetParent(_Parent);
        }
        
        public static GameObject GetContentItem(this GameObject _Prefab, string _ItemName)
        {
            return _Prefab.GetComponent<PrefabContent>().GetItem(_ItemName);
        }
        
        public static GameObject GetContentItem(this Component _PrefabComponent, string _ItemName)
        {
            return _PrefabComponent.GetComponent<PrefabContent>().GetItem(_ItemName);
        }

        public static T GetCompItem<T>(this GameObject _Prefab, string _ItemName) where T : Component
        {
            return _Prefab.GetComponent<PrefabContent>().GetItemComponent<T>(_ItemName);
        }
        
        public static T GetCompItem<T>(this Component _PrefabComponent, string _ItemName) where T : Component
        {
            return _PrefabComponent.GetComponent<PrefabContent>().GetItemComponent<T>(_ItemName);
        }

        public static List<Transform> GetChilds(this Transform _Transform, bool _Recursive = false)
        {
            var result = new List<Transform>();
            for (int i = 0; i < _Transform.childCount; i++)
            {
                var tr = _Transform.GetChild(i);
                result.Add(tr);
                if (_Recursive)
                    GetChilds(tr, ref result);
            }
            
            return result;
        }

        private static void GetChilds(Transform _Transform, ref List<Transform> _List)
        {
            _List.Add(_Transform);
            for (int i = 0; i < _Transform.childCount; i++)
                GetChilds(_Transform.GetChild(i), ref _List);
        }

        public static List<T> GetComponentsInChildrenEx<T>(
            this Component _Item) where T : Behaviour
        {
            IEnumerable<T> result = _Item.GetComponentsInChildren<T>();
            result = result.Except(result.Where(_Itm => !_Itm.IsAlive()));
                
            T root = _Item.GetComponent<T>();
            if (root != null)
                result = result.Concat(new[] {root});
            return result.Distinct().ToList();
        }

        public static Dictionary<T, bool> GetComponentsInChildrenEnabled<T>(
            this Component _Item) where T : Behaviour
        {
            var list = _Item.GetComponentsInChildrenEx<T>();
            return list.ToDictionary(_Itm => _Itm, _Itm => _Itm.enabled);
        }

        public static GameObject Clone(this GameObject _Object)
        {
            var tr = _Object.transform;
            GameObject cloned = Object.Instantiate(_Object, tr.position, tr.rotation, tr.parent);
            cloned.name = _Object.name;
            return cloned;
        }

        public static bool IsAlive(this Behaviour _Beh)
        {
            return !(_Beh is UIBehaviour) && _Beh != null ||
                   _Beh is UIBehaviour uiBeh && uiBeh != null && !uiBeh.IsDestroyed();
        }
    }
}