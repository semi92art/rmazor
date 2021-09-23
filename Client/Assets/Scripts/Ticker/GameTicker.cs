using Utils;

namespace Ticker
{
    public interface ITicker
    {
        float Time { get; }
        bool Pause { get; set; }
        void Reset();
        void Register(object _Object);
        void Unregister(object _Object);
        void ClearRegisteredObjects();
    }

    public interface IGameTicker : ITicker { }
    public interface IUITicker : ITicker { }
    
    // public abstract class Ticker : ITicker
    // {
    //     protected TickerManager m_TickerManager;
    //     protected abstract TickerManager TickerManager { get; }
    //
    //     public float Time => TickerManager.Time;
    //
    //     public bool Pause
    //     {
    //         get => TickerManager.Pause;
    //         set => TickerManager.Pause = value;
    //     }
    //
    //     public void Reset() => TickerManager.Reset();
    //
    //     public void Register(object _Object) => TickerManager.RegisterObject(_Object);
    //
    //     public void Unregister(object _Object) => TickerManager.UnregisterObject(_Object);
    //     
    //     public void ClearRegisteredObjects() => TickerManager.Clear();
    // }

    // FIXME костылей дохрена
    public class GameTicker : IGameTicker
    {
        public static IGameTicker Instance { get; private set; }
        
        public GameTicker()
        {
            Instance = this;
        }
        
        private TickerManager m_TickerManager;
        
        private TickerManager TickerManager => 
            CommonUtils.MonoBehSingleton(ref m_TickerManager, "Game Ticker Manager");
        // protected override TickerManager TickerManager => 
        //     CommonUtils.MonoBehSingleton(ref m_TickerManager, "Game Ticker Manager");
        
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
    }

    // FIXME костылей дохрена
    public class UITicker : IUITicker
    {
        public static IUITicker Instance { get; private set; }
        
        public UITicker()
        {
            Instance = this;
        }
        
        private TickerManager m_TickerManager;
        private TickerManager TickerManager => 
            CommonUtils.MonoBehSingleton(ref m_TickerManager, "UI Ticker Manager");
        // protected override TickerManager TickerManager => 
        //     CommonUtils.MonoBehSingleton(ref m_TickerManager, "UI Ticker Manager");
        

        
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
    }
}