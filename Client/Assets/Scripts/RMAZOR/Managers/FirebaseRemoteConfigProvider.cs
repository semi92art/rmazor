#if FIREBASE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using mazing.common.Runtime;

namespace RMAZOR.Managers
{
    public class FirebaseRemoteConfigProvider : RemoteConfigProviderBase
    {
        protected override async Task FetchConfigs()
        {
            await InitializeFirebase();
        }
        
        private Task InitializeFirebase()
        {
            if (MazorCommonData.FirebaseApp != null)
            {
                Dbg.Log("Firebase was initialized successfully before.");
                return FetchDataAsync();
            }
            return FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(_Task =>
            {
                if (_Task.Result == DependencyStatus.Available)
                {
                    MazorCommonData.FirebaseApp = FirebaseApp.DefaultInstance;
                    FetchDataAsync();
                    Dbg.Log("Firebase initialized successfully");
                } 
                else
                    Dbg.LogError($"Could not resolve all Firebase dependencies: {_Task.Result}");
            });
        }
        
        private Task FetchDataAsync() 
        {
            var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(_Task =>
            {
                FirebaseRemoteConfig.DefaultInstance
                    .ActivateAsync()
                    .ContinueWithOnMainThread(_Task2 =>
                    {
                        OnFetchConfigsCompletedSuccessfully();
                    });
            });
        }

        protected override void GetRemoteConfig(RemoteConfigPropertyInfo _Info)
        {
            var config = FirebaseRemoteConfig.DefaultInstance;
            if (config == null)
                return;
            var configValue = config.GetValue(_Info.Key);
            var @switch = new Dictionary<Type, Func<object>>
            {
                {typeof(bool),   () => configValue.BooleanValue},
                {typeof(float),  () => (float)configValue.DoubleValue},
                {typeof(double), () => configValue.DoubleValue},
                {typeof(string), () => configValue.StringValue},
                {typeof(int),    () => (int)configValue.LongValue},
                {typeof(long),   () => configValue.LongValue}
            };
            var remoteValue= @switch[_Info.Type]();
            _Info.SetCachedValue(remoteValue);
        }
    }
}
#endif