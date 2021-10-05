using Utils;

namespace Ticker
{
    public interface ITicker
    {
        event NoArgsHandler Paused;
        event NoArgsHandler UnPaused;
        float Time { get; }
        bool Pause { get; set; }
        void Reset();
        void Register(object _Object);
        void Unregister(object _Object);
        void ClearRegisteredObjects();
    }

    public interface IGameTicker : ITicker { }
    public interface IUITicker : ITicker { }
    
    public abstract class Ticker : ITicker
    {
        protected TickerManager m_TickerManager;
        protected abstract TickerManager TickerManager { get; }

        protected Ticker()
        {
            TickerManager.Paused += OnPaused;
            TickerManager.UnPaused += OnUnPaused;
        }
        
        public event NoArgsHandler Paused;
        public event NoArgsHandler UnPaused;
        public float Time => TickerManager.Time;
    
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
    
    public class GameTicker : Ticker, IGameTicker
    {
        protected override TickerManager TickerManager => 
            CommonUtils.MonoBehSingleton(ref m_TickerManager, "Game Ticker Manager");
    }

    // FIXME костылей дохрена
    public class UITicker : Ticker, IUITicker
    {
        public static IUITicker Instance { get; private set; }
        public UITicker() => Instance = this;
        protected override TickerManager TickerManager => 
            CommonUtils.MonoBehSingleton(ref m_TickerManager, "UI Ticker Manager");
    }
}