using Common.Helpers;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;

namespace Common.Managers.Notifications
{
    public interface IPushNotificationsProvider : IInit { }
    
    public class PushNotificationsProviderFirebase : InitBase, IPushNotificationsProvider
    {
        private const string           Topic = "Topic";
        private       DependencyStatus m_DependencyStatus;
        
        public override void Init()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(_Task => {
                m_DependencyStatus = _Task.Result;
                if (m_DependencyStatus == DependencyStatus.Available) {
                    InitMessaging();
                } else {
                    Dbg.LogError(
                        "Could not resolve all Firebase dependencies: " + m_DependencyStatus);
                }
            });
            base.Init();
        }

        private static void InitMessaging()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            FirebaseMessaging.SubscribeAsync(Topic).ContinueWithOnMainThread(_Task => {
                LogTaskCompletion(_Task, "SubscribeAsync");
            });
            Dbg.Log("Firebase Messaging Initialized");

            // This will display the prompt to request permission to receive
            // notifications if the prompt has not already been displayed before. (If
            // the user already responded to the prompt, thier decision is cached by
            // the OS and can be changed in the OS settings).
            FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
                _Task => {
                    LogTaskCompletion(_Task, "RequestPermissionAsync");
                }
            );
        }
        
        private static bool LogTaskCompletion(System.Threading.Tasks.Task _Task, string _Operation) 
        {
            bool complete = false;
            if (_Task.IsCanceled) 
            {
                Dbg.LogWarning(_Operation + " canceled.");
            } 
            else if (_Task.IsFaulted)
            {
                Dbg.LogError(_Operation + " encounted an error.");
                foreach (System.Exception exception in _Task.Exception!.Flatten().InnerExceptions) 
                {
                    string errorCode = "";
                    if (exception is FirebaseException firebaseEx)
                        errorCode = $"Error.{((Error) firebaseEx.ErrorCode).ToString()}: ";
                    Dbg.LogError(errorCode + exception);
                }
            } else if (_Task.IsCompleted) {
                Dbg.Log(_Operation + " completed");
                complete = true;
            }
            return complete;
        }

        private static void OnTokenReceived(object _Sender, TokenReceivedEventArgs _Args)
        {
            Dbg.Log("Received Registration Token: " + _Args.Token);
        }
        
        private static void OnMessageReceived(object _Sender, MessageReceivedEventArgs _Args)
        {
            Dbg.Log("Received a new message from: " + _Args.Message.From);
        }
    }
}