using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Helpers
{
    public class CoroutinesRunnerMonoBeh : MonoBehaviour
    {
        [HideInInspector] public volatile bool              mustRun;
        public readonly                   List<UnityAction> Actions = new List<UnityAction>();

        private void Update()
        {
            if (!mustRun)
                return;
            foreach (var action in Actions)
                action?.Invoke();
            Actions.Clear();
            mustRun = false;
        }
    }
}