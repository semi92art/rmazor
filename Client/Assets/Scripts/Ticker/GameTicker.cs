using Utils;

namespace Ticker
{
    public interface ITicker
    {
        bool Pause { get; set; }
        void Register(object _Object);
        void Unregister(object _Object);
        void ClearRegisteredObjects();
    }

    public interface IGameTicker : ITicker { }
    public interface IUITicker : ITicker { }
    
    public abstract class Ticker : ITicker
    {
        protected TickerManager _instance;
        protected abstract TickerManager Instance { get; }

        public bool Pause
        {
            get => Instance.Pause;
            set => Instance.Pause = value;
        }
        
        public void Register(object _Object) => Instance.RegisterObject(_Object);

        public void Unregister(object _Object) => Instance.UnregisterObject(_Object);
        
        public void ClearRegisteredObjects() => Instance.Clear();
    }

    public class GameTicker : Ticker, IGameTicker
    {
        protected override TickerManager Instance => 
            CommonUtils.MonoBehSingleton(ref _instance, "Game Ticker Manager");
    }

    public class UITicker : Ticker, IUITicker
    {
        protected override TickerManager Instance => 
            CommonUtils.MonoBehSingleton(ref _instance, "UI Ticker Manager");
    }
}