using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Common.Helpers;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.Extensions
{
    public static class ObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject _Object) where T : Component
        {
            var res = _Object.GetComponent<T>();
            if (res.IsNull())
                res = _Object.AddComponent<T>();
            return res == null ? default : res;
        }

        public static GameObject RemoveComponentIfExist<T>(this GameObject _Object) where T : Component
        {
            var component = _Object.GetComponent<T>();
            component.DestroySafe();
            return _Object;
        }
        
        //https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html?_ga=2.165477879.911525322.1599381767-2044961467.1583736117
        public static T GetCopy<T>(this T _Item) where T : Object
        {
            string serialized = JsonConvert.SerializeObject(_Item);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static GameObject SetParent(this GameObject _Object, GameObject _Parent)
        {
            _Object.transform.SetParent(_Parent.transform);
            return _Object;
        }

        public static GameObject SetParent(this GameObject _Object, Transform _Parent)
        {
            _Object.transform.SetParent(_Parent);
            return _Object;
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
            result = result.Except(result.Where(_Itm => _Itm.IsNull()));
                
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
            var cloned = Object.Instantiate(_Object, tr.position, tr.rotation, tr.parent);
            cloned.name = _Object.name;
            return cloned;
        }

        public static bool IsNull<T>(this T _Item) where T : Component
        {
            if (_Item is UIBehaviour beh)
                return beh.IsDestroyed();
            return _Item == null;
        }

        public static bool IsNotNull<T>(this T _Item) where T : Component => !IsNull(_Item);
        
        public static T AddComponentOnNewChild<T>(
            this Transform  _Parent,
            string          _Name,
            out GameObject  _Child,
            Vector2?        _LocalPosition = null) where T : Component
        {
            var go = new GameObject(_Name);
            go.SetParent(_Parent);
            go.transform.SetLocalPosXY(_LocalPosition ?? Vector2.zero);
            _Child = go;
            return go.GetOrAddComponent<T>();
        }
        
        public static T AddComponentOnNewChild<T>(
            this GameObject _Parent,
            string          _Name,
            out GameObject  _Child,
            Vector2?        _LocalPosition = null) where T : Component
        {
            return _Parent.transform.AddComponentOnNewChild<T>(_Name, out _Child, _LocalPosition);
        }
        
        public static T SetGoActive<T>(this T _Item, bool _Active) where T : Component
        {
            _Item.gameObject.SetActive(_Active);
            return _Item;
        }
        
        public static bool CastTo<T>(this object _Item, out T _Result) where T : class
        {
            _Result = null;
            string s = null;
            try
            {
                s = JsonConvert.SerializeObject(_Item);
                _Result = JsonConvert.DeserializeObject<T>(s);
                if (_Result == null)
                {
                    Dbg.LogWarning(s);
                    return false;
                }
            }
            catch (SerializationException ex)
            {
                Dbg.LogError(ex.Message);
                Dbg.LogError(s);
                return false;
            }
            return true;
        }
    }
}