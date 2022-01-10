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
                return CommonUtils.MonoBehSingleton(ref m_TickerProceeder, Name);
            }
        }

        protected Ticker()
        {
            TickerProceeder.Paused += OnPaused;
            TickerProceeder.UnPaused += OnUnPaused;
        }
        
        public event UnityAction Paused;
        public event UnityAction UnPaused;
        public float             Time           => TickerProceeder.Time;
        public float             DeltaTime      => UnityEngine.Time.deltaTime;
        public float             FixedTime      => TickerProceeder.FixedTime;
        public float             FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;

        public bool Pause
        {
            get => TickerProceeder.Pause;
            set => TickerProceeder.Pause = value;
        }

        public  void Reset()                    => TickerProceeder.Reset();
        public  void Register(object _Object)   => TickerProceeder.RegisterObject(_Object);
        public  void Unregister(object _Object) => TickerProceeder.UnregisterObject(_Object);
        public  void ClearRegisteredObjects()   => TickerProceeder.Clear();
        private void OnUnPaused()               => UnPaused?.Invoke();
        private void OnPaused()                 => Paused?.Invoke();

        ~Ticker()
        {
            TickerProceeder.Paused  -= OnPaused;
            TickerProceeder.UnPaused -= OnUnPaused;
        }
    }
    
    public class ViewGameTicker : Ticker, IViewGameTicker { }
    public class ModelGameTicker : Ticker, IModelGameTicker { }
    public class UITicker : Ticker, IUITicker { }
    public class CommonTicker : Ticker, ICommonTicker { }
}