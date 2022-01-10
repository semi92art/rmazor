using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public class CoroutinesRunner : MonoBehaviour
    {
        [HideInInspector] public volatile bool              mustRun;
        public readonly                   List<UnityAction> actions = new List<UnityAction>();

        private void Update()
        {
            if (!mustRun)
                return;
            foreach (var action in actions)
                action?.Invoke();
            actions.Clear();
            mustRun = false;
        }
    }
}