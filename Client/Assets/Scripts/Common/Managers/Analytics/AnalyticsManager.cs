using System.Collections.Generic;
using Common.Helpers;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsManager : IAnalyticsProvider { }
    
    public class AnalyticsManager : InitBase, IAnalyticsManager
    {
        #region inject

        private IRemotePropertiesCommon RemoteProperties { get; }
        private IAnalyticsProvidersSet  ProvidersSet     { get; }


        private AnalyticsManager(
            IRemotePropertiesCommon _RemoteProperties,
            IAnalyticsProvidersSet  _ProvidersSet)
        {
            RemoteProperties = _RemoteProperties;
            ProvidersSet     = _ProvidersSet;
        }
        
        #endregion

        #region api
        
        public override void Init()
        {
            if (Initialized)
                return;
            if (!RemoteProperties.DebugEnabled)
                foreach (var provider in ProvidersSet.GetProviders())
                    provider.Init();
            base.Init();
        }
        
        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            // if (RemoteProperties.DebugEnabled && _AnalyticId != AnalyticIds.TestAnalytic)
            //     return;
            foreach (var provider in ProvidersSet.GetProviders())
                provider.SendAnalytic(_AnalyticId, _EventData);
        }
        
        #endregion
    }

    public class AnalyticsManagerFake : InitBase, IAnalyticsManager
    {
        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null) { }
    }
}