using System.Collections;
using System.Collections.Generic;
using Common.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Utils
{
    public static partial class Cor
    {
        #region nonpublic members
        
        private static          CoroutinesRunnerMonoBeh _coroutineRunnerMonoBeh;
        private static readonly List<IEnumerator>       RunningCoroutines = new List<IEnumerator>();

        #endregion

        #region api
        
        public static int GetRunningCoroutinesCount()
        {
            return RunningCoroutines.Count;
        }
        
        public static void Run(IEnumerator _Coroutine)
        {
            if (_Coroutine == null)
                return;
            if (_coroutineRunnerMonoBeh.isDestroyed)
                return;
            var facade = FacadeCoroutine(_Coroutine);
            _coroutineRunnerMonoBeh.StartCoroutine(facade);
        }

        public static void Stop(IEnumerator _Coroutine)
        {
            if (_Coroutine == null)
                return;
            if (_coroutineRunnerMonoBeh.isDestroyed)
                return;
            RunningCoroutines.Remove(_Coroutine);
            _coroutineRunnerMonoBeh.StopCoroutine(_Coroutine);
        }

        public static void RunSync(UnityAction _Action)
        {
            _coroutineRunnerMonoBeh.Actions.Add(_Action);
            _coroutineRunnerMonoBeh.mustRun = true;
        }

        public static bool IsRunning(IEnumerator _Coroutine)
        {
            return RunningCoroutines.Contains(_Coroutine);
        }

        #endregion

        #region nonpublic methods
        
        private static IEnumerator FacadeCoroutine(IEnumerator _Coroutine)
        {
            RunningCoroutines.Add(_Coroutine);
            yield return _Coroutine;
            RunningCoroutines.Remove(_Coroutine);
        }

        #endregion

        #region engine methods
        
        [RuntimeInitializeOnLoadMethod]
        private static void ResetState()
        {
            RunningCoroutines.Clear();
            _coroutineRunnerMonoBeh = Object.FindObjectOfType<CoroutinesRunnerMonoBeh>();
        }
        
        #endregion
    }
}
