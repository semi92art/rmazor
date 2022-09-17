#if FIREBASE
using Common.Helpers;

namespace Common.Managers.Crash_Reporting
{
    public interface ICrashReportingManager : IInit
    {
        
    }
    
    public class CrashReportingManagerFirebaseCrashlytics : InitBase, ICrashReportingManager
    {
        public override void Init()
        {
            // Initialize Firebase
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(_Task => {
                var dependencyStatus = _Task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    // Crashlytics will use the DefaultInstance, as well;
                    // this ensures that Crashlytics is initialized.
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                    // Set a flag here for indicating that your project is ready to use Firebase.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
                base.Init();
            });
        }
    }
}
#endif