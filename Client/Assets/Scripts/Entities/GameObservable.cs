using System.Collections.Generic;
using Controllers;
using DebugConsole;
using Managers;

namespace Entities
{
    public interface IGameObservable
    {
        void Notify(string _NotifyMessage, params object[] _Args);
        void AddObserver(IGameObserver _Observer);
        void AddObservers(IEnumerable<IGameObserver> _Observers);
        void RemoveObserver(IGameObserver _Observer);
        List<IGameObserver> GetObservers();
    }

    public class GameObservable : IGameObservable
    {
        #region nonpublic members
        
        private readonly List<IGameObserver> m_Observers = new List<IGameObserver>();

        #endregion
        
        #region inject
        
        private ISoundGameObserver SoundGameObserver { get; }

        public GameObservable(ISoundGameObserver _SoundGameObserver)
        {
            SoundGameObserver = _SoundGameObserver;
            AddDefaultObservers();
        }
        
        #endregion

        #region api

        public void Notify(string _NotifyMessage, params object[] _Args)
        {
            foreach (var observer in m_Observers)
                observer.OnNotify(_NotifyMessage, _Args);
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

        public List<IGameObserver> GetObservers()
        {
            return m_Observers;
        }

        #endregion
        
        private void AddDefaultObservers()
        {
            m_Observers.AddRange(new IGameObserver[]
            {
                SoundGameObserver,
                AdsManager.Instance,
                AnalyticsManager.Instance,
                PurchasesManager.Instance
            });

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            m_Observers.Add(DebugConsoleView.Instance.Controller);
#endif
        }
    }
}