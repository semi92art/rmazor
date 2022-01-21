using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Utils
{
    public static partial class Cor
    {
        private static CoroutinesRunner _coroutineRunner;

        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            _coroutineRunner = Object.FindObjectOfType<CoroutinesRunner>();
        }

        public static void Run(IEnumerator _Coroutine)
        {
            if (_Coroutine == null)
                return;
            _coroutineRunner.StartCoroutine(_Coroutine);
        }

        public static void Stop(IEnumerator _Coroutine)
        {
            if (_Coroutine != null)
                _coroutineRunner.StopCoroutine(_Coroutine);
        }

        public static void RunSync(UnityAction _Action)
        {
            _coroutineRunner.Actions.Add(_Action);
            _coroutineRunner.mustRun = true;
        }
    }
}
