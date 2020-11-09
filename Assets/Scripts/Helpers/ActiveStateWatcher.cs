using UnityEngine;

namespace Helpers
{
    public class ActiveStateWatcher : MonoBehaviour
    {
        public event ActiveStateHandler ActiveStateChanged;

        private void OnEnable()
        {
            ActiveStateChanged?.Invoke(new ActiveStateEventArgs(true));
        }

        private void OnDisable()
        {
            ActiveStateChanged?.Invoke(new ActiveStateEventArgs(false));
        }
    }

    public delegate void ActiveStateHandler(ActiveStateEventArgs _Args);

    public class ActiveStateEventArgs
    {
        public bool IsActive { get; }

        public ActiveStateEventArgs(bool _IsActive)
        {
            IsActive = _IsActive;
        }
    }
}