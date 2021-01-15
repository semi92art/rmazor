using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Constants;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using Managers;
using Network;
using Network.Packets;
using UnityEngine;
using Utils;

namespace Controllers
{
    public class AuthController
    {
        #region nonpulic members
        
        private readonly FirebaseAuth m_Auth;
        
        #endregion

        #region constructor

        public AuthController()
        {
            FirebaseApp.Create();
            m_Auth = FirebaseAuth.DefaultInstance;
        }

        #endregion

        #region api
        
        public void AuthenticateWithGoogleOnAndroid()
        {
            GoogleSignIn.DefaultInstance.SignOut();
            
            if (GoogleSignIn.Configuration == null)
            {
                GoogleSignIn.Configuration = new GoogleSignInConfiguration
                {
                    RequestIdToken = true,
                    WebClientId = "619485590991-nq6hj3duurrbero3b4q80tbqtvr6d4f6.apps.googleusercontent.com"
                };
            }

            var signIn = GoogleSignIn.DefaultInstance.SignIn();
            var signInCompleted = new TaskCompletionSource<FirebaseUser>();
            signIn.ContinueWith(_Task =>
            {
                if (_Task.IsCanceled)
                {
                    signInCompleted.SetCanceled();
                }
                else if (_Task.IsFaulted && _Task.Exception != null)
                {
                    signInCompleted.SetException(_Task.Exception);
                }
                else
                {
                    string googleIdToken = _Task.Result.IdToken;
                    var credential = GoogleAuthProvider.GetCredential(googleIdToken, null);
                    m_Auth.SignInWithCredentialAsync(credential).ContinueWith(_Tsk =>
                    {
                        if (_Tsk.IsCanceled)
                        {
                            Debug.LogError("SignInWithCredentialAsync was canceled.");
                            return;
                        }
                        if (_Tsk.IsFaulted)
                        {
                            Debug.LogError("SignInWithCredentialAsync encountered an error: " + _Tsk.Exception);
                            return;
                        }

                        FirebaseUser newUser = _Tsk.Result;
                        Debug.LogFormat("User signed in successfully: {0} ({1})",
                            newUser.DisplayName, newUser.UserId);
                        
                        Login(_Tsk.Result.Email, Md5.GetMd5String("google"));
                    });
                }
            });
        }

        public void AuthenticateWithGoogleOnIos()
        {
            
        }

        public void AuthenticateWithAppleIdOnAndroid()
        {
            var providerData = new FederatedOAuthProviderData();
            providerData.ProviderId = "apple.com";
            providerData.Scopes = new[] {"email", "name"};
            providerData.CustomParameters = new Dictionary<string, string>();
            //providerData.CustomParameters.Add("language", "en");
            var provider = new FederatedOAuthProvider();
            provider.SetProviderData(providerData);
            
            m_Auth.SignInWithProviderAsync(provider).ContinueWithOnMainThread(_Task =>
            {
                if (_Task.IsCanceled)
                {
                    Debug.LogError("SignInWithProviderAsync was canceled.");
                    return;
                }
                if (_Task.IsFaulted)
                {
                    Debug.LogError("SignInWithProviderAsync encountered an error: " +
                                   _Task.Exception);
                    return;
                }

                SignInResult signInResult = _Task.Result;
                FirebaseUser user = signInResult.User;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    user.DisplayName, user.UserId);
            });
        }

        public void AuthenticateWithAppleOnIos()
        {
            
        }
        
        #endregion

        #region nonpublic methods

        private void Login(string _Login, string _PasswordHash)
        {
            var loginPacket = new LoginUserPacket(new LoginUserPacketRequestArgs
                {
                    Name = _Login,
                    PasswordHash = _PasswordHash,
                    DeviceId = GameClient.Instance.DeviceId
                });
                loginPacket.OnSuccess(() =>
                    {
                        Debug.Log("Login successfully");
                        GameClient.Instance.AccountId = loginPacket.Response.Id;
                    }
                );
                loginPacket.OnFail(() =>
                {
                    if (loginPacket.ErrorMessage.Id == ServerErrorCodes.AccountNotFoundByDeviceId)
                    {
                        Debug.LogWarning(loginPacket.ErrorMessage);
                        Register(_Login, _PasswordHash);
                    }
                    else if (loginPacket.ErrorMessage.Id == ServerErrorCodes.WrongLoginOrPassword)
                    {
                        Debug.LogError("Login failed: Wrong login or password");
                    }
                    else
                    {
                        Debug.LogError(loginPacket.ErrorMessage);
                    }
                });
                GameClient.Instance.Send(loginPacket);
        }

        private void Register(string _Login, string _PasswordHash)
        {
            var registerPacket = new RegisterUserPacket(
                new RegisterUserPacketRequestArgs
                {
                    Name = _Login,
                    PasswordHash = _PasswordHash,
                    DeviceId = GameClient.Instance.DeviceId,
                    GameId = GameClient.Instance.DefaultGameId
                });
            registerPacket.OnSuccess(() =>
                {
                    Debug.Log("Registered successfully");
                    GameClient.Instance.AccountId = registerPacket.Response.Id;
                    BankManager.Instance.GetBank(true);
                })
                .OnFail(() => { Debug.LogError(registerPacket.ErrorMessage); });
            GameClient.Instance.Send(registerPacket);
        }

        #endregion
    }
}