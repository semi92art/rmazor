using System.Collections.Generic;
using System.Reflection;
using Managers;
using UnityGameLoopDI;

namespace Entities
{
    public abstract class GameObserver
    {
        protected abstract void OnNotify(object _Sender, string _NotifyMessage, params object[] _Args);
    }

    public abstract class GameObservable : UnityGameLoopObjectDI
    {
        private readonly List<GameObserver> m_Observers = new List<GameObserver>();

        protected GameObservable()
        {
            if (!m_Observers.Contains(AdsManager.Instance))
                m_Observers.Add(AdsManager.Instance);
            if (!m_Observers.Contains(AnalyticsManager.Instance))
                m_Observers.Add(AnalyticsManager.Instance);
            if (!m_Observers.Contains(PurchasesManager.Instance))
                m_Observers.Add(PurchasesManager.Instance);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!m_Observers.Contains(DebugConsole.DebugConsoleView.Instance.Controller))
                m_Observers.Add(DebugConsole.DebugConsoleView.Instance.Controller);
#endif
        }
        
        protected void Notify(object _Sender, string _NotifyMessage, params object[] _Args)
        {
            MethodInfo mInfoOnNotify = typeof(GameObserver).GetMethod("OnNotify",
                BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var observer in m_Observers)
                mInfoOnNotify?.Invoke(observer, new[] {_Sender, _NotifyMessage, _Args});
        }

        public void AddObserver(GameObserver _Observer)
        {
            m_Observers.Add(_Observer);
        }

        public void AddObservers(IEnumerable<GameObserver> _Observers)
        {
            m_Observers.AddRange(_Observers);
        }

        public void RemoveObserver(GameObserver _Observer)
        {
            m_Observers.Remove(_Observer);
        }

        public List<GameObserver> GetObservers()
        {
            return m_Observers;
        }
    }
    
    public class ObserverNotifyer : GameObservable
    {
        public void RaiseNotify(object _Sender, string _NotifyMessage, params object[] _Args)
        {
            Notify(_Sender, _NotifyMessage, _Args);
        }
    }
}