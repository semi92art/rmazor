using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Ticker
{
    public class TickerProceeder : MonoBehaviour
    {
        #region nonpublic members

        private          bool                    m_Paused;
        private          float                   m_Delta;
        private          float                   m_FixedDelta;
        private readonly List<IUpdateTick>       m_UpdateInfoDict       = new List<IUpdateTick>();
        private readonly List<IFixedUpdateTick>  m_FixedUpdateInfoDict  = new List<IFixedUpdateTick>();
        private readonly List<ILateUpdateTick>   m_LateUpdateInfoDict   = new List<ILateUpdateTick>();
        private readonly List<IDrawGizmosTick>   m_DrawGizmosInfoDict   = new List<IDrawGizmosTick>();
        private readonly List<IApplicationPause> m_ApplicationPauseDict = new List<IApplicationPause>();
        private readonly List<IApplicationFocus> m_ApplicationFocusDict = new List<IApplicationFocus>();
        private readonly List<IDestroy>          m_DestroyDict          = new List<IDestroy>();

        #endregion

        #region api

        public event UnityAction Paused;
        public event UnityAction UnPaused;
        public float             Time      { get; private set; }
        public float             FixedTime { get; private set; }

        public bool Pause
        {
            get => m_Paused;
            set
            {
                m_Paused = value;
                if (value)
                    Paused?.Invoke();
                else
                    UnPaused?.Invoke();
            }
        }

        public void Reset()
        {
            Pause = false;
            Time = 0f;
            m_Delta = 0f;
        }

        public void RegisterObject(object _Object)
        {
            if (_Object == null)
                return;
            RegisterUpdateMethods(_Object);
        }

        public void UnregisterObject(object _Object)
        {
            if (_Object == null)
                return;
            if (_Object is IUpdateTick onUpdateObj)
                m_UpdateInfoDict.Remove(onUpdateObj);
            if (_Object is IFixedUpdateTick onFixedUpdateObj)
                m_FixedUpdateInfoDict.Remove(onFixedUpdateObj);
            if (_Object is ILateUpdateTick onLateUpdateObj)
                m_LateUpdateInfoDict.Remove(onLateUpdateObj);
            if (_Object is IDrawGizmosTick onDrawGizmosObj)
                m_DrawGizmosInfoDict.Remove(onDrawGizmosObj);
            if (_Object is IApplicationPause onAppPauseObj)
                m_ApplicationPauseDict.Remove(onAppPauseObj);
            if (_Object is IApplicationFocus onAppFocusObj)
                m_ApplicationFocusDict.Remove(onAppFocusObj);
            if (_Object is IDestroy onDestroy)
                m_DestroyDict.Remove(onDestroy);
        }

        public void Clear()
        {
            m_UpdateInfoDict      .Clear();
            m_DrawGizmosInfoDict  .Clear();
            m_FixedUpdateInfoDict .Clear();
            m_DrawGizmosInfoDict  .Clear();
            m_ApplicationPauseDict.Clear();
            m_ApplicationFocusDict.Clear();
            m_DestroyDict         .Clear();
        }

        #endregion

        #region engine methods

        private void Update()
        {
            if (Pause)
            {
                m_Delta += UnityEngine.Time.deltaTime;
                return;
            }
            Time = UnityEngine.Time.time - m_Delta;
            for (int i = 0; i < m_UpdateInfoDict.Count; i++)
                m_UpdateInfoDict[i]?.UpdateTick();
        }

        private void FixedUpdate()
        {
            if (Pause)
            {
                m_FixedDelta += UnityEngine.Time.fixedDeltaTime;
                return;
            }
            FixedTime = UnityEngine.Time.fixedTime - m_FixedDelta;
            for (int i = 0; i < m_FixedUpdateInfoDict.Count; i++)
                m_FixedUpdateInfoDict[i]?.FixedUpdateTick();
        }

        private void LateUpdate()
        {
            if (Pause)
                return;
            for (int i = 0; i < m_LateUpdateInfoDict.Count; i++)
                m_LateUpdateInfoDict[i]?.LateUpdateTick();
        }

        private void OnDrawGizmos()
        {
            if (Pause)
                return;
            if (!m_DrawGizmosInfoDict.Any())
                return;
            foreach (var obj in m_DrawGizmosInfoDict)
                obj?.DrawGizmosTick();
        }

        private void OnApplicationPause(bool _Pause)
        {
            for (int i = 0; i < m_ApplicationPauseDict.Count; i++)
                m_ApplicationPauseDict[i]?.OnApplicationPause(_Pause);
        }

        private void OnApplicationFocus(bool _HasFocus)
        {
            for (int i = 0; i < m_ApplicationFocusDict.Count; i++)
                m_ApplicationFocusDict[i]?.OnApplicationFocus(_HasFocus);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < m_DestroyDict.Count; i++)
                m_DestroyDict[i]?.OnDestroy();
        }

        #endregion

        #region nonpublic methods

        private void RegisterUpdateMethods(object _Object)
        {
            if (_Object is IUpdateTick onUpdateObj)
                m_UpdateInfoDict.Add(onUpdateObj);
            if (_Object is IFixedUpdateTick onFixedUpdateObj)
                m_FixedUpdateInfoDict.Add(onFixedUpdateObj);
            if (_Object is ILateUpdateTick onLateUpdateObj)
                m_LateUpdateInfoDict.Add(onLateUpdateObj);
            if (_Object is IDrawGizmosTick onDrawGizmosObj)
                m_DrawGizmosInfoDict.Add(onDrawGizmosObj);
            if (_Object is IApplicationPause onAppPauseObj)
                m_ApplicationPauseDict.Add(onAppPauseObj);
            if (_Object is IApplicationFocus onAppFocusObj)
                m_ApplicationFocusDict.Add(onAppFocusObj);
            if (_Object is IDestroy onDestroy)
                m_DestroyDict.Add(onDestroy);

            m_UpdateInfoDict      .RemoveAll(_Obj => _Obj == null);
            m_FixedUpdateInfoDict .RemoveAll(_Obj => _Obj == null);
            m_LateUpdateInfoDict  .RemoveAll(_Obj => _Obj == null);
            m_DrawGizmosInfoDict  .RemoveAll(_Obj => _Obj == null);
            m_ApplicationPauseDict.RemoveAll(_Obj => _Obj == null);
            m_ApplicationFocusDict.RemoveAll(_Obj => _Obj == null);
            m_DestroyDict         .RemoveAll(_Obj => _Obj == null);
        }

        #endregion
    }
}