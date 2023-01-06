using Firebase;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using UnityEngine;

namespace Common
{
    public interface IFirebaseInitializer : IInit
    {
        DependencyStatus DependencyStatus { get; }
        FirebaseApp      FirebaseApp      { get; }
    }
    
    public class FirebaseInitializer : InitBase, IFirebaseInitializer
    {
        public DependencyStatus DependencyStatus { get; private set; }
        public FirebaseApp      FirebaseApp      { get; private set; }

        public override void Init()
        {
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (Application.isEditor)
                return;
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
                base.Init();
            });
        }
    }
}