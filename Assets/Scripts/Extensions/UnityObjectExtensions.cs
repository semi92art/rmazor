using UnityEngine;

namespace Extensions
{
    public static class UnityObjectExtensions
    {
        public static void DestroySafe(this Object _GameObject, bool _CheckForNull = true)
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
    }
}