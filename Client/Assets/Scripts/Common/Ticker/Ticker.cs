using Common.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Ticker
{
    public interface ITicker
    {
        event UnityAction Paused;
        event UnityAction UnPaused;
        float             Time           { get; }
        float             TimeUnscaled   { get; }
        float             DeltaTime      { get; }
        float             FixedTime      { get; }
        float             FixedDeltaTime { get; }
        bool              Pause          { get; set; }
        void              Reset();
        void              Register(object   _Object);
        void              Unregister(object _Object);
        void              ClearRegisteredObjects();
    }

    public interface IViewGameTicker  : ITicker { }
    public interface IModelGameTicker : ITicker { }
    public interface IUITicker        : ITicker { }
    public interface ICommonTicker    : ITicker { }
    
    public abstract class Ticker : ITicker
    {
        private static T MonoBehSingleton<T>(ref T _Instance, string _Name) where T : MonoBehaviour
        {
            if (!_Instance.IsNull())
                return _Instance;
            var go = GameObject.Find(_Name);
            if (go == null || go.transform.parent != null)
                go = new GameObject(_Name);
            _Instance = go.GetOrAddComponent<T>();
            Object.DontDestroyOnLoad(go);
            return _Instance;
        }
        
        private TickerProceeder m_TickerProceeder;
        private bool          m_TickerManagerInstantiated;
        private string        Name => GetType().Name.WithSpaces();

        private TickerProceeder TickerProceeder
        {
            get
            {
                if (m_TickerManagerInstantiated) 
                    return m_TickerProceeder;
                m_TickerManagerInstantiated = true;
                return MonoBehSingleton(ref m_TickerProceeder, Name);
            }
        }

        protected Ticker()
        {
            TickerProceeder.Paused += OnPaused; //-V3080
            TickerProceeder.UnPaused += OnUnPaused;
        }
        
        public event UnityAction Paused;
        public event UnityAction UnPaused;
        public float             Time           => TickerProceeder.Time;
        public float             TimeUnscaled   =>  UnityEngine.Time.unscaledTime;
        public float             DeltaTime      => UnityEngine.Time.deltaTime;
        public float             FixedTime      => TickerProceeder.FixedTime;
        public float             FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;

        public bool Pause
        {
            get => TickerProceeder.Pause;
            set => TickerProceeder.Pause = value;
        }

        public  void Reset()                    => TickerProceeder.Reset(); //-V3080
        public  void Register(object _Object)   => TickerProceeder.RegisterObject(_Object); //-V3080
        public  void Unregister(object _Object) => TickerProceeder.UnregisterObject(_Object); //-V3080
        public  void ClearRegisteredObjects()   => TickerProceeder.Clear(); //-V3080
        private void OnUnPaused()               => UnPaused?.Invoke();
        private void OnPaused()                 => Paused?.Invoke();

        ~Ticker()
        {
            if (TickerProceeder == null)
                return;
            TickerProceeder.Paused  -= OnPaused;
            TickerProceeder.UnPaused -= OnUnPaused;
        }
    }
    
    public class ViewGameTicker : Ticker, IViewGameTicker { }
    public class ModelGameTicker : Ticker, IModelGameTicker { }
    public class UITicker : Ticker, IUITicker { }
    public class CommonTicker : Ticker, ICommonTicker { }
}