using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Network;
using UnityEngine;
using Utils;
using UpdateMethodsDict = System.Collections.Generic.Dictionary
    <int, System.Collections.Generic.List<DI.MethodInfoObject>>;

namespace DI
{
    #region types
        
    public class MethodInfoObject
    {
        public bool DoNotDestroyOnLoad { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public object Object { get; set; }
    }
        
    #endregion
    
    public class ContainersManager : MonoBehaviour, ISingleton
    {
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
                if (!GameClient.Instance.IsModuleTestsMode)
                    DontDestroyOnLoad(go);
                return _instance;
            }
        }
        
        #endregion

        #region private members

        private readonly UpdateMethodsDict m_UpdateMethods = new UpdateMethodsDict();
        private readonly UpdateMethodsDict m_FixedUpdateMethods = new UpdateMethodsDict();
        private readonly UpdateMethodsDict m_LateUpdateMethods = new UpdateMethodsDict();

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

        public void UnregisterObject(object _Object)
        {
            if (_Object == null)
                return;
            UnregisterUpdateMethods<UpdateAttribute>(_Object);
            UnregisterUpdateMethods<FixedUpdateAttribute>(_Object);
            UnregisterUpdateMethods<LateUpdateAttribute>(_Object);
        }

        public void Clear(bool _Forced = false)
        {
            ClearMethods(m_UpdateMethods, _Forced);
            ClearMethods(m_FixedUpdateMethods, _Forced);
            ClearMethods(m_LateUpdateMethods, _Forced);
        }
        
        #endregion

        #region engine methods

        private void Update()
        {
            InvokeUpdateMethods(m_UpdateMethods);
        }

        private void FixedUpdate()
        {
            InvokeUpdateMethods(m_FixedUpdateMethods);
        }

        private void LateUpdate()
        {
            InvokeUpdateMethods(m_LateUpdateMethods);
        }

        #endregion

        #region private methods
        
        private void InvokeUpdateMethods(UpdateMethodsDict _Dictionary)
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

        private void RegisterUpdateMethods<T>(object _Object) where T : Attribute, IOrder, IDoNotDestroyOnLoad
        {
            var dict = GetDictByUpdateType<T>();
            var mInfosUpdate = GetMethodInfos<T>(_Object);
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
                    MethodInfo = mInfo,
                    DoNotDestroyOnLoad = attribute.DoNotDestroyOnLoad
                });
            }
        }

        private void UnregisterUpdateMethods<T>(object _Object) where T : Attribute, IOrder, IDoNotDestroyOnLoad
        {
            var dict = GetDictByUpdateType<T>();
            var mInfosUpdate = GetMethodInfos<T>(_Object);
            
            foreach (var mInfo in mInfosUpdate)
            {
                var attribute = mInfo.GetCustomAttributes(true).OfType<T>().First();
                if (!dict.ContainsKey(attribute.Order))
                    continue;
                var mInfoObj = dict[attribute.Order].FirstOrDefault(_Item => _Item.Object == _Object);
                if (mInfoObj == null)
                    continue;
                dict[attribute.Order].Remove(mInfoObj);
            }
        }

        private UpdateMethodsDict GetDictByUpdateType<T>() where T : Attribute, IOrder, IDoNotDestroyOnLoad
        {
            UpdateMethodsDict dict = null;
            if (typeof(T) == typeof(UpdateAttribute))
                dict = m_UpdateMethods;
            else if (typeof(T) == typeof(FixedUpdateAttribute))
                dict = m_FixedUpdateMethods;
            else if (typeof(T) == typeof(LateUpdateAttribute))
                dict = m_LateUpdateMethods;
            if (dict == null)
                throw new NotImplementedException();
            return dict;
        }

        private MethodInfo[] GetMethodInfos<T>(object _Object) where T : Attribute, IOrder, IDoNotDestroyOnLoad
        {
            MethodInfo[] mInfosUpdate = _Object.GetType()
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Instance 
                    | BindingFlags.NonPublic
                    | BindingFlags.Static)
                .Where(_Mi => _Mi.GetCustomAttributes(true).OfType<T>().Any())
                .ToArray();
            return mInfosUpdate;
        }

        private void ClearMethods(UpdateMethodsDict _MethodsDict, bool _Forced)
        {
            foreach (var kvp in _MethodsDict.ToList())
            foreach (var method in kvp.Value.ToArray().Where(_Method => _Forced || !_Method.DoNotDestroyOnLoad))
                _MethodsDict[kvp.Key].Remove(method);
        }

        #endregion
    }
}