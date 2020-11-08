using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extensions
{
    public static class ObjectExtensions
    {
        public static T AddComponentIfNotExist<T>(this GameObject _Object) where T : Component
        {
            return _Object.GetComponent<T>() == null ? _Object.AddComponent<T>() : null;
        }
        
        //https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html?_ga=2.165477879.911525322.1599381767-2044961467.1583736117
        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            System.Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos) {
                if (pinfo.CanWrite) {
                    try {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos) {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
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

        public static T GetCompItem<T>(this GameObject _Prefab, string _ItemName) where T : Component
        {
            return _Prefab.GetComponent<PrefabContent>().GetItemComponent<T>(_ItemName);
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

        public static List<T> GetComponentsInChildrenEx<T>(this Component _Item) where T : Behaviour
        {
            IEnumerable<T> result = _Item.GetComponentsInChildren<T>();
            T root = _Item.GetComponent<T>();
            if (root != null)
                result = result.Concat(new[] {root});
            return result.ToList();
        }

        public static Dictionary<T, bool> GetComponentsInChildrenEnabled<T>(this Component _Item) where T : Behaviour
        {
            var list = _Item.GetComponentsInChildrenEx<T>();
            var result = new Dictionary<T, bool>();
            foreach (var item in list.Where(item => !result.ContainsKey(item)))
                result.Add(item, item.enabled);
            return result;
        }

        public static GameObject Clone(this GameObject _Object)
        {
            var tr = _Object.transform;
            GameObject cloned = Object.Instantiate(_Object, tr.position, tr.rotation, tr.parent);
            cloned.name = $"{_Object.name} (Clone)";
            return cloned;
        }

        public static bool IsAlive(this UIBehaviour _UiBehaviour)
        {
            return _UiBehaviour != null && !_UiBehaviour.IsDestroyed();
        }
    }
}