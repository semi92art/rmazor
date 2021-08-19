using UnityEngine;

namespace TimeProviders
{
    public interface ITimeProvider
    {
        float Time { get; }
        bool Pause { get; set; }
        void Reset();
    }

    public abstract class TimeProviderBase : MonoBehaviour, ITimeProvider
    {
        protected float Delta;
    
        public float Time { get; protected set; }
        public bool Pause { get; set; }

        public void Reset()
        {
            Time = 0;
            Delta = 0;
        }
    
        protected virtual void Update()
        {
            if (Pause)
            {
                Delta += UnityEngine.Time.deltaTime;
                return;
            }
            Time = UnityEngine.Time.time - Delta;
        }
    }
}