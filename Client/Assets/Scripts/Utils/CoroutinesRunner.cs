using System.Collections;
using GameHelpers;
using UnityEngine;

namespace Utils
{
    public static partial class Coroutines
    {
        private const string RunnerName = "Coroutines Runner";
        private static MonoBehaviour _coroutineRunner;
        private static bool _runnerFound;

        static Coroutines() => FindRunner();

        private static MonoBehaviour FindRunner()
        {
            if (_runnerFound)
                return _coroutineRunner;
            
            var go = GameObject.Find(RunnerName);
            if (go == null)
            {
                go = new GameObject(RunnerName);
                go.AddComponent<DontDestroyOnLoad>();
            }

            _coroutineRunner = go.GetComponent<DontDestroyOnLoad>();
            _runnerFound = true;
            return _coroutineRunner;
        }
        
        public static void Run(IEnumerator _Coroutine)
        {
            if (_Coroutine == null)
                return;
            FindRunner().StartCoroutine(_Coroutine);
        }

        public static void Stop(IEnumerator _Coroutine)
        {
            if (_Coroutine != null)
                FindRunner().StopCoroutine(_Coroutine);
        }
    }
}
