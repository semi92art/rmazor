using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using Newtonsoft.Json;
using UnityEngine.Events;

namespace RMAZOR.Managers
{
    public class RemoteConfigManager : InitBase, IRemoteConfigManager
    {
        private bool m_FetchCompletedActionDone;

        private       IRemotePropertiesInfoProvider RemotePropertiesInfoProvider { get; }
        private       IRemoteConfigProvider         RemoteConfigProvider         { get; }
        private       CommonGameSettings            CommonGameSettings           { get; }

        protected RemoteConfigManager(
            IRemotePropertiesInfoProvider _RemotePropertiesInfoProvider,
            IRemoteConfigProvider         _RemoteConfigProvider,
            CommonGameSettings            _CommonGameSettings)
        {
            RemotePropertiesInfoProvider = _RemotePropertiesInfoProvider;
            RemoteConfigProvider         = _RemoteConfigProvider;
            CommonGameSettings           = _CommonGameSettings;
        }
        
        public override void Init()
        {
            if (Initialized)
                return;
            RemoteConfigProvider.Initialize += () =>
            {
                LoadPropertiesFromCache(FetchConfigs);
            };
            RemotePropertiesInfoProvider.Initialize += () =>
            {
                var infos = RemotePropertiesInfoProvider.GetInfos();
                RemoteConfigProvider.SetRemoteCachedPropertyInfos(infos);
                RemoteConfigProvider.Init();
            };
            RemotePropertiesInfoProvider.Init();
        }

        private void SignalInit()
        {
            base.Init();
        }
        
        private void LoadPropertiesFromCache(UnityAction _OnFinish)
        {
            var infos = RemoteConfigProvider.GetFetchedInfos();
            var entity = SetValuesOfPropertyInfos(infos);
            Cor.Run(Cor.WaitWhile(
                () => entity.Result == EEntityResult.Pending,
                () =>
                {
                    if (entity.Result == EEntityResult.Success)
                        _OnFinish?.Invoke();
                }));
        }

        private void FetchConfigs()
        {
            Cor.Run(Cor.WaitWhile(
                () => !m_FetchCompletedActionDone,
                SignalInit));
            var infos = RemoteConfigProvider.GetFetchedInfos().ToList();
            var entity = SetValuesOfPropertyInfos(infos);
            Cor.Run(Cor.WaitWhile(
                () => entity.Result == EEntityResult.Pending,
                FetchTestDevices));
        }

        private static Entity<bool> SetValuesOfPropertyInfos(IEnumerable<RemoteConfigPropertyInfo> _Infos)
        {
            var finalEntity = new Entity<bool>();
            if (_Infos == null)
            {
                finalEntity.Result = EEntityResult.Fail;
                return finalEntity;
            }
            var infos = _Infos.ToArray();
            var valsReady = new bool[infos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                var entity = info.GetCachedValueEntity;
                var info1 = info;
                Cor.Run(Cor.WaitWhile(() => entity.Result == EEntityResult.Pending,
                    () =>
                    {
                        if (entity.Result == EEntityResult.Success)
                            info1.SetPropertyValue(entity.Value);
                        valsReady[i] = true;
                    }));
            }
            bool IsNotAllReady() => valsReady.Any(_V => !_V);
            Cor.Run(Cor.WaitWhile(IsNotAllReady,
                () =>
                {
                    finalEntity.Result = EEntityResult.Success;
                    finalEntity.Value = true;
                }));
            return finalEntity;
        }

        private void FetchTestDevices()
        {
            var infos = RemoteConfigProvider.GetFetchedInfos();
            var info = infos.First(_I => _I.Key == "test_device_ids");
            var entity = info.GetCachedValueEntity;
            Cor.Run(Cor.WaitWhile(
                () => entity.Result == EEntityResult.Pending,
                () =>
                {
                    if (entity.Result == EEntityResult.Fail)
                    {
                        Dbg.LogError("Failed to load test devices");
                        return;
                    }
                    string testDeviceIdfasJson = Convert.ToString(entity.Value);
                    string[] deviceIds;
                    try
                    {
                        deviceIds = JsonConvert.DeserializeObject<string[]>(testDeviceIdfasJson);
                        if (deviceIds.NullOrEmpty())
                        {
                            m_FetchCompletedActionDone = true;
                            return;
                        }
                    }
                    catch (SerializationException ex)
                    {
                        Dbg.LogException(ex);
                        m_FetchCompletedActionDone = true;
                        return;
                    }
                    var idfaEntity = CommonUtils.GetIdfa();
                    Cor.Run(Cor.WaitWhile(() => idfaEntity.Result == EEntityResult.Pending,
                        () =>
                        {
                            bool isThisDeviceForTesting = deviceIds!.Any(
                                _Idfa => _Idfa.Equals(
                                    idfaEntity.Value,
                                    StringComparison.InvariantCultureIgnoreCase));
                            if (isThisDeviceForTesting)
                                Dbg.Log("This device is test");
                            CommonGameSettings.debugEnabled = isThisDeviceForTesting;
                            CommonGameSettings.testAds = isThisDeviceForTesting;
                            m_FetchCompletedActionDone = true;
                        }));
                }));
        }
    }
}