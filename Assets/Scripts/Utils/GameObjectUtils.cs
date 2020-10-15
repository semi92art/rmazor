using System.Reflection;
using UnityEngine;

namespace Utils
{
    public static class GameObjectUtils
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
    }
}