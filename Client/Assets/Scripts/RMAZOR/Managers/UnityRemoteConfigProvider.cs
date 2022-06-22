using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Entities;
using Common.Ticker;
using Common.Utils;
using Unity.RemoteConfig;
using UnityEngine;

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
            Cor.Run(Cor.Delay(3f, null, () => FinishFetching(null)));
            ConfigManager.FetchCompleted += _instance.OnFetchCompleted;
            ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
            return null;
        }
        
        private void OnFetchCompleted(ConfigResponse _Response)
        {
            if (_Response.status == ConfigRequestStatus.Success)
                OnFetchConfigsCompletedSuccessfully();
            FinishFetching(_Response);
        }
        
        private void FinishFetching(ConfigResponse? _Response)
        {
            if (Initialized)
                return;
            if (_Response.HasValue)
                Dbg.Log("Remote Config Initialized with status: " + _Response.Value.status);
            base.Init();
        }

        protected override void GetRemoteConfig(RemoteConfigPropertyInfo _Info)
        {
            var config = ConfigManager.appConfig;
            var entity = _Info.GetCachedValueEntity;
            Cor.Run(Cor.WaitWhile(
                () => entity.Result == EEntityResult.Pending,
                () =>
                {
                    if (entity.Result != EEntityResult.Success)
                        return;
                    object result = entity.Value;
                    var result1 = result;
                    var @switch = new Dictionary<Type, Func<object>>
                    {
                        {typeof(bool),   () => config.GetBool(  _Info.Key, Convert.ToBoolean(result1))},
                        {typeof(float),  () => config.GetFloat( _Info.Key, Convert.ToSingle(result1))},
                        {typeof(string), () => config.GetString(_Info.Key, Convert.ToString(result1))},
                        {typeof(int),    () => config.GetInt(   _Info.Key, Convert.ToInt32(result1))},
                        {typeof(long),   () => config.GetLong(  _Info.Key, Convert.ToInt64(result1))}
                    };
                    result = !_Info.IsJson ? @switch[_Info.Type]() : config.GetJson(_Info.Key);
                    _Info.SetPropertyValue(result);
                }));
        }

        #endregion

        #region engine methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
            if (_instance != null)
                ConfigManager.FetchCompleted -= _instance.OnFetchCompleted;
        }

        #endregion
    }
}