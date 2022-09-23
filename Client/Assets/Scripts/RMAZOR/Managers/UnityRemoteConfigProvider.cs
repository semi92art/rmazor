using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Entities;
using Common.Ticker;
using Common.Utils;
using Unity.Services.RemoteConfig;
using UnityEngine;
using ConfigResponse = Unity.Services.RemoteConfig.ConfigResponse;

namespace RMAZOR.Managers
{
    public class UnityRemoteConfigProvider : RemoteConfigProviderBase
    {

        #region nonpublic members

        private static UnityRemoteConfigProvider _instance;
        
        #endregion
        
        #region types
        
        private struct UserAttributes { }
        private struct AppAttributes { }
        
        #endregion

        #region inject

        private ICommonTicker CommonTicker { get; }
        
        public UnityRemoteConfigProvider(ICommonTicker _CommonTicker)
        {
            CommonTicker = _CommonTicker;
            _instance = this;
        }

        #endregion

        #region nonpublic methods

        protected override Task FetchConfigs()
        {
            Cor.Run(Cor.Delay(3f, CommonTicker, () =>
            {
                if (Initialized)
                    return;
                var response = new ConfigResponse {status = ConfigRequestStatus.None};
                FinishFetching(response);
            }));
            RemoteConfigService.Instance.SetEnvironmentID("production");
            RemoteConfigService.Instance.FetchCompleted += _instance.OnFetchCompleted;
            RemoteConfigService.Instance.FetchConfigs(new UserAttributes(), new AppAttributes());
            return null;
        }
        
        private void OnFetchCompleted(ConfigResponse _Response)
        {
            if (_Response.status == ConfigRequestStatus.Success)
            {
                OnFetchConfigsCompletedSuccessfully();
            }
            FinishFetching(_Response);
        }
        
        private void FinishFetching(ConfigResponse _Response)
        {
            if (Initialized)
                return;
            Dbg.Log("Remote Config Initialized with status: " + _Response.status);
            base.Init();
        }

        protected override void GetRemoteConfig(RemoteConfigPropertyInfo _Info)
        {
            var config = RemoteConfigService.Instance.appConfig;
            var entity = _Info.GetCachedValueEntity;
            Cor.Run(Cor.WaitWhile(
                () => entity.Result == EEntityResult.Pending,
                () =>
                {
                    if (entity.Result != EEntityResult.Success)
                    {
                        Dbg.LogWarning($"Remote Config entity with key {_Info.Key} result: {entity.Result}");
                        return;
                    }
                    object value = entity.Value;
                    var value1 = value;
                    var @switch = new Dictionary<Type, Func<object>>
                    {
                        {typeof(bool),   () => config.GetBool(  _Info.Key, Convert.ToBoolean(value1))},
                        {typeof(float),  () => config.GetFloat( _Info.Key, Convert.ToSingle( value1))},
                        {typeof(string), () => config.GetString(_Info.Key, Convert.ToString( value1))},
                        {typeof(int),    () => config.GetInt(   _Info.Key, Convert.ToInt32(  value1))},
                        {typeof(long),   () => config.GetLong(  _Info.Key, Convert.ToInt64(  value1))}
                    };
                    value = !_Info.IsJson ? @switch[_Info.Type]() : config.GetJson(_Info.Key);
                    _Info.SetPropertyValue(@value);
                }, _Seconds: 2f));
        }

        #endregion

        #region engine methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
            if (_instance != null)
                RemoteConfigService.Instance.FetchCompleted -= _instance.OnFetchCompleted;
        }

        #endregion
    }
}