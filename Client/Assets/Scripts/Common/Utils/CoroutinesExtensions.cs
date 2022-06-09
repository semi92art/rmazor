using System.Collections;
using UnityEngine.Events;

namespace Common.Utils
{
    public static class CoroutinesExtensions
    {
        public static IEnumerator ContinueWith(this IEnumerator _Coroutine, UnityAction _Action)
        {
            yield return _Coroutine;
            _Action?.Invoke();
        }
    }
}