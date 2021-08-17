using UnityEngine.SceneManagement;

namespace UnityGameLoopDI
{
    public class Ticker
    {
        private bool m_WasUnregistered;
        
        protected Ticker()
        {
            TickerManager.Instance.RegisterObject(this);
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        protected void Unregister(bool _ChangeScene = false)
        {
            if (m_WasUnregistered)
                return;
            if (_ChangeScene)
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            TickerManager.Instance.UnregisterObject(this);
            m_WasUnregistered = true;
        }

        private void OnActiveSceneChanged(Scene _Prev, Scene _Next)
        {
            //TODO
        }
    }
}