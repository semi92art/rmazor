using UnityEngine.Events;

namespace Common.Helpers
{
    public abstract class InitBase : IInit
    {
        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public virtual void Init()
        {
            RaiseInitialization();
        }

        protected void RaiseInitialization()
        {
            Initialize?.Invoke();
            Initialized = true;
        }
    }
}