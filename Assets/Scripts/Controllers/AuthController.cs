using Firebase.Auth;
using UnityEngine;

namespace Controllers
{
    public class AuthController
    {
        private readonly FirebaseAuth m_Auth;
        
        public AuthController()
        {
            m_Auth = FirebaseAuth.DefaultInstance;
        }
        
        public void AuthenticateWithGoogle()
        {
            var tokenTask = m_Auth.CurrentUser.TokenAsync(false);
            tokenTask.RunSynchronously();
            string token = tokenTask.Result;
            var credential = GoogleAuthProvider.GetCredential(token, Application.identifier);
            m_Auth.SignInWithCredentialAsync(credential).ContinueWith(_Task => {
                if (_Task.IsCanceled) {
                    Debug.LogError("SignInWithCredentialAsync was canceled.");
                    return;
                }
                if (_Task.IsFaulted) {
                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + _Task.Exception);
                    return;
                }

                FirebaseUser newUser = _Task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);
            });
        }

    }
}