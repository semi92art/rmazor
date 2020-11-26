using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exceptions;
using UnityEngine;
using Utils;

namespace DI
{
    public class ContainersManager : MonoBehaviour, ISingleton
    {
        #region types
        
        private class MethodInfoObject
        {
            public MethodInfo MethodInfo { get; set; }
            public object Object { get; set; }
        }
        
        #endregion

        private enum UpdateType { Update, FixedUpdate, LateUpdate }
        
        #region singleton
        
        private static ContainersManager _instance;
        
        public static ContainersManager Instance
        {
            get
            {
                if (_instance is ContainersManager ptm && !ptm.IsNull()) 
                    return _instance;
                GameObject go = new GameObject("Containers Manager");
                _instance = go.AddComponent<ContainersManager>();
                DontDestroyOnLoad(go);
                return _instance;
            }
            set => _instance = value;
        }
        
        #endregion

        #region private members

        private readonly Dictionary<int, List<MethodInfoObject>> m_UpdateMethods =
            new Dictionary<int, List<MethodInfoObject>>();
        private readonly Dictionary<int, List<MethodInfoObject>> m_FixedUpdateMethods =
            new Dictionary<int, List<MethodInfoObject>>();
        private readonly Dictionary<int, List<MethodInfoObject>> m_LateUpdateMethods =
            new Dictionary<int, List<MethodInfoObject>>();

        #endregion

        #region api

        public void RegisterObject(object _Object)
        {
            if (_Object == null)
                return;
            RegisterUpdateMethods<UpdateAttribute>(_Object);
            RegisterUpdateMethods<FixedUpdateAttribute>(_Object);
            RegisterUpdateMethods<LateUpdateAttribute>(_Object);
        }
        
        #endregion

        #region engine methods

        private void Update()
        {
            InvokeUpdateMethods(UpdateType.Update);
        }

        private void FixedUpdate()
        {
            InvokeUpdateMethods(UpdateType.FixedUpdate);
        }

        private void LateUpdate()
        {
            InvokeUpdateMethods(UpdateType.LateUpdate);
        }

        #endregion

        #region private methods

        private void InvokeUpdateMethods(UpdateType _UpdateType)
        {
            switch (_UpdateType)
            {
                case UpdateType.Update:
                    InvokeUpdateMethods(m_UpdateMethods);
                    break;
                case UpdateType.FixedUpdate:
                    InvokeUpdateMethods(m_FixedUpdateMethods);
                    break;
                case UpdateType.LateUpdate:
                    InvokeUpdateMethods(m_LateUpdateMethods);
                    break;
                default:
                    throw new InvalidEnumArgumentExceptionEx(_UpdateType);
            }
        }

        private void InvokeUpdateMethods(Dictionary<int, List<MethodInfoObject>> _Dictionary)
        {
            var methodsByOrder = _Dictionary
                .ToList()
                .OrderBy(_Kvp => _Kvp.Key)
                .Select(_Kvp => _Kvp.Value);

            foreach (var methods in methodsByOrder)
            foreach (var method in methods.ToArray())
            {
                if (method.Object == null)
                {
                    methods.Remove(method);
                    continue;
                }
                method.MethodInfo.Invoke(method.Object, null);
            }
        }

        private void RegisterUpdateMethods<T>(object _Object) where T : Attribute, IOrder
        {
            Dictionary<int, List<MethodInfoObject>> dict = null;
            if (typeof(T) == typeof(UpdateAttribute))
                dict = m_UpdateMethods;
            else if (typeof(T) == typeof(FixedUpdateAttribute))
                dict = m_FixedUpdateMethods;
            else if (typeof(T) == typeof(LateUpdateAttribute))
                dict = m_LateUpdateMethods;
            if (dict == null)
                throw new NotImplementedException();
            
            MethodInfo[] mInfosUpdate = _Object.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(_Mi => _Mi.GetCustomAttributes(true).OfType<T>().Any())
                .ToArray();
            
            foreach (var mInfo in mInfosUpdate)
            {
                if (mInfo.IsPublic)
                {
                    Debug.LogError($"Method {mInfo.Name} of class {_Object.GetType().Name} can't be public." +
                                   $"Methods with attribute {nameof(T)} must be private or protected.");
                }
                var attribute = mInfo.GetCustomAttributes(true).OfType<T>().First();

                if (!dict.ContainsKey(attribute.Order))
                    dict.Add(attribute.Order, new List<MethodInfoObject>()); 
                dict[attribute.Order].Add(new MethodInfoObject
                {
                    Object = _Object,
                    MethodInfo = mInfo
                });
            }
        }

        #endregion
    }
}