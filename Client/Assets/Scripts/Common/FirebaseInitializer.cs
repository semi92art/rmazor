#if FIREBASE
using System;
using Firebase;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using UnityEngine;

namespace Common
{
    public class FirebaseInitializer : InitBase, IFirebaseInitializer
    {
        private DependencyStatus DependencyStatus { get; set; }
        private FirebaseApp      FirebaseApp      { get; set; }

        public override void Init()
        {
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (Application.isEditor)
                return;
            try
            {
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(_Task =>
                {
                    if (_Task.Result == DependencyStatus.Available)
                    {
                        FirebaseApp = FirebaseApp.DefaultInstance;
                        Dbg.Log("Firebase initialized successfully");
                    } 
                    else
                        Dbg.LogError($"Could not resolve all Firebase dependencies: {_Task.Result}");
                    DependencyStatus = _Task.Result;
                    Cor.Run(Cor.WaitNextFrame(() => base.Init()));
                });
            }
            catch (Exception ex)
            {
                Dbg.LogError(ex);
            }
        }
    }
}
#endif