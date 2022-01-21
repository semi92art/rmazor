using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Utils
{
    public class CoroutinesRunner : MonoBehaviour
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