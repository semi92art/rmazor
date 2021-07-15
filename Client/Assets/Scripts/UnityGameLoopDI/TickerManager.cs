﻿using System.Collections.Generic;
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

        private readonly List<IUpdateTick> m_UpdateInfoDict = new List<IUpdateTick>();
        private readonly List<IFixedUpdateTick> m_FixedUpdateInfoDict = new List<IFixedUpdateTick>();
        private readonly List<ILateUpdateTick> m_LateUpdateInfoDict = new List<ILateUpdateTick>();
        private readonly List<IDrawGizmosTick> m_DrawGizmosInfoDict = new List<IDrawGizmosTick>();
        
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
            if (_Object is IUpdateTick onUpdateObj)
                m_UpdateInfoDict.Remove(onUpdateObj);
            if (_Object is IFixedUpdateTick onFixedUpdateObj)
                m_FixedUpdateInfoDict.Remove(onFixedUpdateObj);
            if (_Object is ILateUpdateTick onLateUpdateObj)
                m_LateUpdateInfoDict.Remove(onLateUpdateObj);
            if (_Object is IDrawGizmosTick onDrawGizmosObj)
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
            for (int i = 0; i < m_UpdateInfoDict.Count; i++)
                m_UpdateInfoDict[i]?.UpdateTick();
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < m_FixedUpdateInfoDict.Count; i++)
                m_FixedUpdateInfoDict[i]?.FixedUpdateTick();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < m_LateUpdateInfoDict.Count; i++)
                m_LateUpdateInfoDict[i]?.LateUpdateTick();
        }
        
        private void OnDrawGizmos()
        {
            if (!m_DrawGizmosInfoDict.Any())
                return;
            foreach (var obj in m_DrawGizmosInfoDict)
                obj?.DrawGizmosTick();
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