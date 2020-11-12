using UnityEngine;

namespace Helpers
{
    public class ActiveStateWatcher : MonoBehaviour
    {
        public event BoolEventHandler ActiveStateChanged;
        public event BoolEventHandler ViabilityStateChanged;
        
        private void OnEnable()
        {
            ActiveStateChanged?.Invoke(new BoolEventArgs(true));
        }

        private void Awake()
        {
            ViabilityStateChanged?.Invoke(new BoolEventArgs(true));
        }

        private void OnDisable()
        {
            ActiveStateChanged?.Invoke(new BoolEventArgs(false));
        }

        private void OnDestroy()
        {
            ViabilityStateChanged?.Invoke(new BoolEventArgs(false));
        }
    }

    public delegate void BoolEventHandler(BoolEventArgs _Args);

    public class BoolEventArgs
    {
        public bool Value { get; }

        public BoolEventArgs(bool _Value)
        {
            Value = _Value;
        }
    }
}