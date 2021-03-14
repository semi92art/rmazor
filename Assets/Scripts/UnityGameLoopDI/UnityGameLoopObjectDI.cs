using System;
using UnityEngine.SceneManagement;

namespace UnityGameLoopDI
{
    public interface IOnUpdate { [Obsolete("Only for internal usage")] void OnUpdate(); }
    public interface IOnFixedUpdate { [Obsolete("Only for internal usage")] void OnFixedUpdate(); }
    public interface IOnLateUpdate { [Obsolete("Only for internal usage")] void OnLateUpdate(); }
    public interface IOnDrawGizmos { [Obsolete("Only for internal usage")] void OnDrawGizmos(); }
    
    public class UnityGameLoopObjectDI
    {
        private bool m_WasUnregistered;
        
        protected UnityGameLoopObjectDI()
        {
            UnityGameLoopDIManager.Instance.RegisterObject(this);
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        protected void Unregister(bool _ChangeScene = false)
        {
            if (m_WasUnregistered)
                return;
            if (_ChangeScene)
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            UnityGameLoopDIManager.Instance.UnregisterObject(this);
            m_WasUnregistered = true;
        }

        private void OnActiveSceneChanged(Scene _Prev, Scene _Next)
        {
            //Unregister(true);
        }
    }
}