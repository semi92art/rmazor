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
        #region nonpublic members
        
        private DependencyStatus m_DependencyStatus;
        private FirebaseApp      m_FirebaseApp;

        #endregion

        #region api
        
        public bool   DependenciesAreOk => m_DependencyStatus == Firebase.DependencyStatus.Available;
        public string DependencyStatus  => m_DependencyStatus.ToString();
        
        public override void Init()
        {
            InitializeFirebase();
        }

        #endregion

        #region nonpublic methods
        
        private void InitializeFirebase()
        {
            if (Application.isEditor)
                return;
            try
            {
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(_Task =>
                {
                    if (_Task.Result == Firebase.DependencyStatus.Available)
                    {
                        m_FirebaseApp = FirebaseApp.DefaultInstance;
                        Dbg.Log("Firebase initialized successfully");
                    } 
                    else
                        Dbg.LogError($"Could not resolve all Firebase dependencies: {_Task.Result}");
                    m_DependencyStatus = _Task.Result;
                    Cor.Run(Cor.WaitNextFrame(() => base.Init()));
                });
            }
            catch (Exception ex)
            {
                Dbg.LogError(ex);
            }
        }

        #endregion
    }
}
#endif