using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Helpers
{
    public class CoroutinesRunnerMonoBeh : MonoBehaviour
    {
        [HideInInspector] public          bool              isDestroyed;
        [HideInInspector] public volatile bool              mustRun;
        public readonly                   List<UnityAction> Actions = new List<UnityAction>();

        private void Update()
        {
            if (!mustRun)
                return;
            try
            {
                foreach (var action in Actions)
                    action?.Invoke();
            }
            catch (Exception e)
            {
                Dbg.LogError(e);
            }

            Actions.Clear();
            mustRun = false;
        }

        private void OnDestroy()
        {
            isDestroyed = true;
        }
    }
}