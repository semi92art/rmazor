using System.Collections;
using UnityEngine;

namespace CielaSpike
{
    public static class ThreadNinjaMonoBehaviourExtensions
    {
        /// <summary>
        /// Start a co-routine on a background thread.
        /// </summary>
        /// <param name="_Routine"></param>
        /// <param name="_Task">Gets a task object with more control on the background thread.</param>
        /// <param name="_Behaviour"></param>
        /// <returns></returns>
        public static Coroutine StartCoroutineAsync(
            this MonoBehaviour _Behaviour, 
            IEnumerator _Routine, 
            out Task _Task)
        {
            _Task = new Task(_Routine);
            return _Behaviour.StartCoroutine(_Task);
        }

        /// <summary>
        /// Start a co-routine on a background thread.
        /// </summary>
        public static Coroutine StartCoroutineAsync(
            this MonoBehaviour _Behaviour, 
            IEnumerator _Routine)
        {
            return StartCoroutineAsync(_Behaviour, _Routine, out _);
        }
    }
}