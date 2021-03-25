using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace UnityGameLoopDI
{
    public class TickerManager : MonoBehaviour, ISingleton
    {
        #region singleton
        
        private static TickerManager _instance;
        public static TickerManager Instance => 
            CommonUtils.MonoBehSingleton(ref _instance, "Ticker Manager");

        #endregion

        #region nonpublic members

        private readonly List<IOnUpdate> m_UpdateInfoDict = new List<IOnUpdate>();
        private readonly List<IOnFixedUpdate> m_FixedUpdateInfoDict = new List<IOnFixedUpdate>();
        private readonly List<IOnLateUpdate> m_LateUpdateInfoDict = new List<IOnLateUpdate>();
        private readonly List<IOnDrawGizmos> m_DrawGizmosInfoDict = new List<IOnDrawGizmos>();
        
        #endregion

        #region api

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
            if (_Object is IOnUpdate onUpdateObj)
                m_UpdateInfoDict.Remove(onUpdateObj);
            if (_Object is IOnFixedUpdate onFixedUpdateObj)
                m_FixedUpdateInfoDict.Remove(onFixedUpdateObj);
            if (_Object is IOnLateUpdate onLateUpdateObj)
                m_LateUpdateInfoDict.Remove(onLateUpdateObj);
            if (_Object is IOnDrawGizmos onDrawGizmosObj)
                m_DrawGizmosInfoDict.Remove(onDrawGizmosObj);
        }

        public void Clear()
        {
            m_UpdateInfoDict.Clear();
            m_DrawGizmosInfoDict.Clear();
            m_FixedUpdateInfoDict.Clear();
            m_DrawGizmosInfoDict.Clear();
        }
        
        #endregion

        #region engine methods

        private void Update()
        {
            if (!m_UpdateInfoDict.Any())
                return;
            foreach (var obj in m_UpdateInfoDict)
                obj?.OnUpdate();
        }

        private void FixedUpdate()
        {
            if (!m_FixedUpdateInfoDict.Any())
                return;
            foreach (var obj in m_FixedUpdateInfoDict)
                obj?.OnFixedUpdate();
        }

        private void LateUpdate()
        {
            if (!m_LateUpdateInfoDict.Any())
                return;
            foreach (var obj in m_LateUpdateInfoDict)
                obj?.OnLateUpdate();
        }
        
        private void OnDrawGizmos()
        {
            if (!m_DrawGizmosInfoDict.Any())
                return;
            foreach (var obj in m_DrawGizmosInfoDict)
                obj?.OnDrawGizmos();
        }

        #endregion

        #region nonpublic methods
        
        private void RegisterUpdateMethods(object _Object)
        {
            if (_Object is IOnUpdate onUpdateObj)
                m_UpdateInfoDict.Add(onUpdateObj);
            if (_Object is IOnFixedUpdate onFixedUpdateObj)
                m_FixedUpdateInfoDict.Add(onFixedUpdateObj);
            if (_Object is IOnLateUpdate onLateUpdateObj)
                m_LateUpdateInfoDict.Add(onLateUpdateObj);
            if (_Object is IOnDrawGizmos onDrawGizmosObj)
                m_DrawGizmosInfoDict.Add(onDrawGizmosObj);
            
            RemoveNullObjects();
        }

        private void RemoveNullObjects()
        {
            m_UpdateInfoDict.RemoveAll(_Obj => _Obj == null);
            m_FixedUpdateInfoDict.RemoveAll(_Obj => _Obj == null);
            m_LateUpdateInfoDict.RemoveAll(_Obj => _Obj == null);
            m_DrawGizmosInfoDict.RemoveAll(_Obj => _Obj == null);
        }

        #endregion
    }
}