using UnityEngine;

namespace GameHelpers
{
    public class ActiveStateWatcher : MonoBehaviour
    {
        public event BoolHandler OnActiveStateChanged;

        private void OnEnable()
        {
            OnActiveStateChanged?.Invoke(true);
        }

        private void OnDisable()
        {
            OnActiveStateChanged?.Invoke(false);
        }
    }

    public delegate void BoolHandler(bool _Value);
}