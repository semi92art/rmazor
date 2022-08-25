using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;

namespace Common.Managers.Analytics
{
    public interface IUnityAnalyticsProvider : IAnalyticsProvider { }

    public class UnityAnalyticsProvider : AnalyticsProviderBase, IUnityAnalyticsProvider
    {
        #region nonpublic methods
        
        protected override void SendAnalyticCore(string _AnalyticId, IDictionary<string, object> _EventData = null)
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

        protected override string GetRealAnalyticId(string  _AnalyticId)
        {
            return _AnalyticId;
        }

        protected override string GetRealParameterId(string _ParameterId)
        {
            return _ParameterId;
        }

        #endregion
    }
}
