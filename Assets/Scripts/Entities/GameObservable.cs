using System.Collections.Generic;

namespace Entities
{
    public interface IGameObserver
    {
        void OnNotify(object _Sender, int _NotifyId, params object[] _Args);
    }
    
    public interface IGameObservable
    {
        void AddObserver(IGameObserver _Observer);
        void AddObservers(IEnumerable<IGameObserver> _Observers);
        void RemoveObserver(IGameObserver _Observer);
    }

    public class GameObservable : IGameObservable
    {
        private readonly List<IGameObserver> m_Observers = new List<IGameObserver>();
        
        protected void Notify(object _Sender, int _NotifyId, params object[] _Args)
        {
            foreach (var observer in m_Observers)
                observer.OnNotify(_Sender, _NotifyId, _Args);
        }

        public void AddObserver(IGameObserver _Observer)
        {
            m_Observers.Add(_Observer);
        }

        public void AddObservers(IEnumerable<IGameObserver> _Observers)
        {
            m_Observers.AddRange(_Observers);
        }

        public void RemoveObserver(IGameObserver _Observer)
        {
            m_Observers.Remove(_Observer);
        }

        protected List<IGameObserver> GetObservers()
        {
            return m_Observers;
        }
    }
    
    public class ObserverNotifyer : GameObservable
    {
        public void RaiseNotify(object _Sender, int _NotifyId, params object[] _Args)
        {
            Notify(_Sender, _NotifyId, _Args);
        }
    }
}