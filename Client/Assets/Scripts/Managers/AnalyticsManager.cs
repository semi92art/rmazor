using System;
using System.Collections.Generic;
using Common;
using Common.Exceptions;
using Common.Helpers;
using GameHelpers;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;

namespace Managers
{
    public interface IAnalyticsManager : IInit
    {
        void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null);
        void OnAdvertiseEvent(EAdsProvider _Provider, string _PlacementId, string _PlacementName, bool _Completed);
    }
    
    public class AnalyticsManager : InitBase, IAnalyticsManager
    {
        #region api
        
        public override async void Init()
        { 
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
            base.Init();
        }

        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            if (!CommonUtils.IsRunningOnDevice())
                return;
            _EventData ??= new Dictionary<string, object>();
            Analytics.CustomEvent(_AnalyticId, _EventData);
            try
            {
                Events.CustomData(_AnalyticId, _EventData);
            }
            catch (Exception e)
            {
                Dbg.LogError(e.Message);
            }
        }

        public void OnAdvertiseEvent(EAdsProvider _Provider, string _PlacementId, string _PlacementName, bool _Completed)
        {
            if (Application.isEditor)
                return;
            try
            {
                var args = new Events.AdImpressionArgs(
                    _Completed ? Events.AdCompletionStatus.Completed : Events.AdCompletionStatus.Incomplete,
                    GetProvider(_Provider),
                    _PlacementId,
                    _PlacementName);
                Events.AdImpression(args);
            }
            catch (Exception e)
            {
                Dbg.LogError(e.Message);
            }
        }

        #endregion

        #region nonpublic methods

        private static Events.AdProvider GetProvider(EAdsProvider _Provider)
        {
            return _Provider switch
            {
                EAdsProvider.AdMob => Events.AdProvider.AdMob,
                EAdsProvider.UnityAds  => Events.AdProvider.UnityAds,
                _                      => throw new SwitchCaseNotImplementedException(_Provider)
            };
        }

        #endregion
    }
}
