using DI.Extensions;
using UnityEngine.Events;
using Utils;

namespace Ticker
{
    public interface ITicker
    {
        event UnityAction Paused;
        event UnityAction UnPaused;
        float             Time           { get; }
        float             DeltaTime      { get; }
        float             FixedTime      { get; }
        float             FixedDeltaTime { get; }
        bool              Pause          { get; set; }
        void              Reset();
        void              Register(object _Object);
        void              Unregister(object _Object);
        void              ClearRegisteredObjects();
    }

    public interface IViewGameTicker  : ITicker { }
    public interface IModelGameTicker : ITicker { }
    public interface IUITicker        : ITicker { }
    public interface ICommonTicker    : ITicker { }
    
    public abstract class Ticker : ITicker
    {
        private TickerManager m_TickerManager;
        private bool          m_TickerManagerInstantiated;
        private string        Name => GetType().Name.WithSpaces();

        private TickerManager TickerManager
        {
            get
            {
                if (m_TickerManagerInstantiated) 
                    return m_TickerManager;
                m_TickerManagerInstantiated = true;
                return CommonUtils.MonoBehSingleton(ref m_TickerManager, Name);
            }
        }

        protected Ticker()
        {
            TickerManager.Paused += OnPaused;
            TickerManager.UnPaused += OnUnPaused;
        }
        
        public event UnityAction Paused;
        public event UnityAction UnPaused;
        public float             Time           => TickerManager.Time;
        public float             DeltaTime      => UnityEngine.Time.deltaTime;
        public float             FixedTime      => TickerManager.FixedTime;
        public float             FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;

        public bool Pause
        {
            get => TickerManager.Pause;
            set => TickerManager.Pause = value;
        }

        public  void Reset()                    => TickerManager.Reset();
        public  void Register(object _Object)   => TickerManager.RegisterObject(_Object);
        public  void Unregister(object _Object) => TickerManager.UnregisterObject(_Object);
        public  void ClearRegisteredObjects()   => TickerManager.Clear();
        private void OnUnPaused()               => UnPaused?.Invoke();
        private void OnPaused()                 => Paused?.Invoke();

        ~Ticker()
        {
            TickerManager.Paused  -= OnPaused;
            TickerManager.UnPaused -= OnUnPaused;
        }
    }
    
    public class ViewGameTicker : Ticker, IViewGameTicker { }
    public class ModelGameTicker : Ticker, IModelGameTicker { }
    public class UITicker : Ticker, IUITicker { }
    public class CommonTicker : Ticker, ICommonTicker { }
}