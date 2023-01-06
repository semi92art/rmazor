#if FIREBASE
using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using mazing.common.Runtime;
using mazing.common.Runtime.Utils;

namespace RMAZOR.Managers
{
    public class FirebaseRemoteConfigProvider : RemoteConfigProviderBase
    {
        #region inject
        
        private IFirebaseInitializer FirebaseInitializer { get; }

        private FirebaseRemoteConfigProvider(IFirebaseInitializer _FirebaseInitializer)
        {
            FirebaseInitializer = _FirebaseInitializer;
        }

        #endregion

        #region nonpublic methods

        protected override void FetchConfigs()
        {
            Cor.Run(InitializeRemoteConfigCoroutine());
        }

        private IEnumerator InitializeRemoteConfigCoroutine()
        {
            if (!FirebaseInitializer.Initialized)
                yield return null;
            if (FirebaseInitializer.DependencyStatus != DependencyStatus.Available)
            {
                Dbg.LogError("Failed to initialize Firebase Remote Config," +
                             $" dependency status: {FirebaseInitializer.DependencyStatus}");
                yield break;
            }
            FetchDataAsync();
        }
        
        private void FetchDataAsync() 
        {
            var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            fetchTask.ContinueWithOnMainThread(_Task =>
            {
                FirebaseRemoteConfig.DefaultInstance
                    .ActivateAsync()
                    .ContinueWithOnMainThread(_Task2 =>
                    {
                        OnFetchConfigsCompletedSuccessfully();
                        Dbg.Log("Firebase Remote Config Initialization finished");
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

        #endregion
    }
}
#endif