using System.Linq;
using UnityEngine;

namespace Common.Extensions
{
    public static class UnityObjectExtensions
    {
        public static void DestroySafe(this GameObject _GameObject, bool _CheckForNull = true)
        {
            if (_CheckForNull && _GameObject == null)
                return;
            if (Application.isEditor)
                Object.DestroyImmediate(_GameObject);
            else
                Object.Destroy(_GameObject);
        }
        
        public static void DestroySafe(this Component _Component, bool _CheckForNull = true)
        {
            if (_CheckForNull && _Component == null)
                return;
            if (Application.isEditor)
                Object.DestroyImmediate(_Component);
            else
                Object.Destroy(_Component);
        }

        public static void DestroyChildrenSafe(this GameObject _GameObject, bool _CheckForNull = true)
        {
            var children = _GameObject.transform.GetChildren()
                .Where(_T => !_T.IsNull())
                .Select(_T => _T.gameObject)
                .ToList();
            foreach (var child in children)
                child.DestroySafe(_CheckForNull);
        }
    }
}