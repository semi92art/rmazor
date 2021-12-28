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

    public interface IViewGameTicker : ITicker { }
    public interface IModelGameTicker : ITicker { }
    public interface IUITicker : ITicker { }
    public interface ICommonTicker : ITicker { }
    
    public abstract class Ticker : ITicker
    {
        protected TickerManager m_TickerManager;
        protected abstract TickerManager TickerManager { get; }

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
    
        public void Reset() => TickerManager.Reset();
        public void Register(object _Object) => TickerManager.RegisterObject(_Object);
        public void Unregister(object _Object) => TickerManager.UnregisterObject(_Object);
        public void ClearRegisteredObjects() => TickerManager.Clear();
        private void OnUnPaused() => UnPaused?.Invoke();
        private void OnPaused() => Paused?.Invoke();

        ~Ticker()
        {
            TickerManager.Paused  -= OnPaused;
            TickerManager.UnPaused -= OnUnPaused;
        }
    }
    
    public class ViewGameTicker : Ticker, IViewGameTicker
    {
        protected override TickerManager TickerManager => 
            CommonUtils.MonoBehSingleton(ref m_TickerManager, "View Game Ticker Manager");
    }
    
    public class ModelGameTicker : Ticker, IModelGameTicker
    {
        protected override TickerManager TickerManager => 
            CommonUtils.MonoBehSingleton(ref m_TickerManager, "Model Game Ticker Manager");
    }
    
    public class UITicker : Ticker, IUITicker
    {
        protected override TickerManager TickerManager => 
            CommonUtils.MonoBehSingleton(ref m_TickerManager, "UI Ticker Manager");
    }
    
    public class CommonTicker : Ticker, ICommonTicker
    {
        public static ICommonTicker Instance { get; private set; }
        public CommonTicker() => Instance = this;
        protected override TickerManager TickerManager => 
            CommonUtils.MonoBehSingleton(ref m_TickerManager, "Common Ticker Manager");
    }
}