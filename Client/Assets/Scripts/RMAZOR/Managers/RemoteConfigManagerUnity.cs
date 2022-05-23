using System;
using System.Collections.Generic;
using Common;
using Common.Entities;
using Common.Helpers;
using Unity.RemoteConfig;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization;
using Common.Extensions;
using Common.Network;
using UnityEngine.Events;

namespace RMAZOR.Managers
{
    public class RemoteConfigManagerUnity : InitBase, IRemoteConfigManager
    {
        // #region nonpublic members
        //
        // private static ConfigResponse? _configResponse;
        //
        // #endregion
        //
        // #region types
        //
        // private struct UserAttributes { }
        // private struct AppAttributes { }
        //
        // #endregion
        //
        // #region inject
        //
        // public RemoteConfigManagerUnity(
        //     IGameClient        _GameClient,
        //     CommonGameSettings _CommonGameSettings,
        //     ModelSettings      _ModelSettings,
        //     ViewSettings       _ViewSettings,
        //     RemoteProperties   _RemoteProperties) 
        //     : base(
        //         _GameClient, 
        //         _CommonGameSettings,
        //         _ModelSettings, 
        //         _ViewSettings, 
        //         _RemoteProperties) { }
        //
        // #endregion
        //
        // #region api
        //
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // public static void ResetState()
        // {
        //     ConfigManager.FetchCompleted -= OnFetchCompleted;
        // }
        //
        // #endregion
        //
        // #region nonpblic methods
        //
        // protected override void FetchConfigs()
        // {
        //     Cor.Run(Cor.Delay(3f, () => FinishFetching(false)));
        //     if (!Application.isEditor || CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
        //         ConfigManager.FetchCompleted += OnFetchCompleted;
        //     ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
        // }
        //
        // private static void FinishFetching(bool _OnFetchCompleted)
        // {
        //     if (!_configResponse.HasValue)
        //     {
        //         if (!_OnFetchCompleted)
        //             Instance.SignalInit();
        //         return;
        //     }
        //     Dbg.Log("Remote Config Initialized with status: " + _configResponse.Value.status);
        //     if (_OnFetchCompleted)
        //         Instance.SignalInit();
        // }
        //
        // private static void OnFetchCompleted(ConfigResponse _Response)
        // {
        //     Cor.Run(Cor.WaitWhile(
        //         () => !_fetchCompletedActionDone,
        //         () => FinishFetching(true)));
        //     if (Application.isEditor && !Instance.CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
        //     {
        //         _fetchCompletedActionDone = true;
        //         return;
        //     }
        //     
        //     if (_Response.status != ConfigRequestStatus.Success)
        //     {
        //         Dbg.Log("Remote Config Initialized with status: " + _Response.status);
        //         _fetchCompletedActionDone = true;
        //         return;
        //     }
        //     FetchCommonSettings();
        //     FetchModelSettings();
        //     FetchViewSettings();
        //     FetchColorSets();
        //     FetchBackgroundTextureSets();
        //     FetchTestDevices();
        //     Dbg.Log("Remote Config fetch completed");
        // }


    }
}