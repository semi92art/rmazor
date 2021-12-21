using System.Collections.Generic;
using Exceptions;
using GameHelpers;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.Analytics;
using UnityEngine.Events;

namespace Managers
{
    public interface IAnalyticsManager : IInit
    {
        void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null);
        void OnAdvertiseEvent(EAdsProvider _Provider, string _PlacementId, string _PlacementName, bool _Completed);
    }
    
    public class AnalyticsManager : IAnalyticsManager
    {
        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;

        public async void Init()
        { 
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
            Initialize?.Invoke();
            Initialized = true;
        }

        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            Analytics.CustomEvent(_AnalyticId, _EventData);
            Events.CustomData(_AnalyticId, _EventData);
        }

        public void OnAdvertiseEvent(EAdsProvider _Provider, string _PlacementId, string _PlacementName, bool _Completed)
        {
            var args = new Events.AdImpressionArgs(
                _Completed ? Events.AdCompletionStatus.Completed : Events.AdCompletionStatus.Incomplete,
                GetProvider(_Provider),
                _PlacementId,
                _PlacementName);
            Events.AdImpression(args);
        }

        #endregion

        #region nonpublic methods

        private static Events.AdProvider GetProvider(EAdsProvider _Provider)
        {
            return _Provider switch
            {
                EAdsProvider.GoogleAds => Events.AdProvider.AdMob,
                EAdsProvider.UnityAds  => Events.AdProvider.UnityAds,
                EAdsProvider.Facebook  => Events.AdProvider.Facebook,
                _                      => throw new SwitchCaseNotImplementedException(_Provider)
            };
        }

        #endregion
    }
}
