using System.Collections;
using CielaSpike;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public static partial class Coroutines
    {
        private static CoroutinesRunner _coroutineRunner;
        private static bool             _runnerFound;

        static Coroutines()
        {
            FindRunner();
        }

        public static void Run(IEnumerator _Coroutine, bool _FromAnotherThread = false)
        {
            if (_Coroutine == null)
                return;
            FindRunner().StartCoroutine(_FromAnotherThread ? AnotherThreadCoroutine(_Coroutine) : _Coroutine);
        }

        public static void Stop(IEnumerator _Coroutine)
        {
            if (_Coroutine != null)
                FindRunner().StopCoroutine(_Coroutine);
        }

        public static void RunSync(UnityAction _Action)
        {
            _coroutineRunner.actions.Add(_Action);
            _coroutineRunner.mustRun = true;
        }
        
        private static MonoBehaviour FindRunner()
        {
            if (_runnerFound)
                return _coroutineRunner;
            _coroutineRunner = Object.FindObjectOfType<CoroutinesRunner>();
            _runnerFound = true;
            return _coroutineRunner;
        }

        private static IEnumerator AnotherThreadCoroutine(IEnumerator _SyncCoroutine)
        {
            yield return Ninja.JumpBack;
            yield return _SyncCoroutine;
            yield return Ninja.JumpToUnity;
        }
    }
}
