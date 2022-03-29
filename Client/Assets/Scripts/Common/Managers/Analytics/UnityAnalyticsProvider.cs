using System;
using System.Collections.Generic;
using Common.Helpers;
using Unity.Services.Analytics;
using Unity.Services.Core;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsProvider : IInit
    {
        void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null);
    }

    public interface IUnityAnalyticsProvider : IAnalyticsProvider { }

    public class UnityAnalyticsProvider : InitBase, IUnityAnalyticsProvider
    {
        #region api

        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                return;
            _EventData ??= new Dictionary<string, object>();
            UnityEngine.Analytics.Analytics.CustomEvent(_AnalyticId, _EventData);
            try
            {
                Events.CustomData(_AnalyticId, _EventData);
            }
            catch (Exception e)
            {
                Dbg.LogError(e.Message);
            }
        }

        #endregion
    }
}
