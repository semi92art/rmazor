using System.Linq;
using ModestTree;
using UnityEngine;

namespace Extensions
{
    public static class UnityObjectExtensions
    {
        public static void DestroySafe(this GameObject _GameObject, bool _CheckForNull = true)
        {
            if (_CheckForNull && _GameObject == null)
                return;
#if UNITY_EDITOR
            if (Application.isPlaying)
                Object.Destroy(_GameObject);
            else
                Object.DestroyImmediate(_GameObject);
#else
            Object.Destroy(_GameObject);
#endif
        }
        
        public static void DestroySafe(this Component _GameObject, bool _CheckForNull = true)
        {
            if (_CheckForNull && _GameObject == null)
                return;
#if UNITY_EDITOR
            if (Application.isPlaying)
                Object.Destroy(_GameObject);
            else
                Object.DestroyImmediate(_GameObject);
#else
            Object.Destroy(_GameObject);
#endif
        }

        public static void DestroyChildrenSafe(this GameObject _GameObject, bool _CheckForNull = true)
        {
            var children = _GameObject.transform.GetChilds()
                .Where(_T => !_T.IsNull())
                .Select(_T => _T.gameObject)
                .ToList();
            foreach (var child in children)
                child.DestroySafe(_CheckForNull);
        }
    }
}