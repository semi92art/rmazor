using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Managers;

namespace Entities
{

    public interface IGameObservable
    {
        void AddObserver(GameObserver _Observer);
        void AddObservers(IEnumerable<GameObserver> _Observers);
        void RemoveObserver(GameObserver _Observer);
    }

    public abstract class GameObserver
    {
        protected abstract void OnNotify(object _Sender, string _NotifyMessage, params object[] _Args);
    }

    public abstract class GameObservable : IGameObservable
    {
        private readonly List<GameObserver> m_Observers = new List<GameObserver>();

        protected GameObservable()
        {
            if (!m_Observers.Contains(AdsManager.Instance))
                m_Observers.Add(AdsManager.Instance);
            if (!m_Observers.Contains(AnalyticsManager.Instance))
                m_Observers.Add(AnalyticsManager.Instance);
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

        protected List<GameObserver> GetObservers()
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