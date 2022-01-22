using Common;
using Common.Network;
using Common.Network.Packets;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.Events;


namespace Managers
{
    public enum AuthResult
    {
        LoginSuccess,
        RegisterSuccess,
        LoginFailed,
        RegisterFailed,
        FailedNoInternet
    }
    
    public interface IAuthController : IInit
    {
        void Authenticate(UnityAction<AuthResult> _OnDoIfRegister);
    }
    
    public class AuthController : IAuthController
    {
        #region inject
        
        private IGameClient GameClient { get; }

        public AuthController(IGameClient _GameClient)
        {
            GameClient = _GameClient;
        }

        #endregion
        
        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        public void Init()
        {
#if UNITY_ANDROID
            InitGooglePlayServices();
#elif  UNITY_IPHONE || UNITY_IOS
            // TODO
#endif
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public void Authenticate(UnityAction<AuthResult> _OnDoIfRegister)
        {
#if UNITY_EDITOR
            Login(SystemInfo.deviceUniqueIdentifier, "unity", _OnDoIfRegister);         
#elif UNITY_ANDROID
            AuthenticateWithGoogle(_OnDoIfRegister);
#elif UNITY_IPHONE || UNITY_IOS
            AuthenticateWithApple(_OnDoIfRegister);
#endif
        }
        
        #endregion

        #region nonpublic methods
        
#if UNITY_ANDROID && !UNITY_EDITOR
        
        private void AuthenticateWithGoogle(UnityAction<AuthResult> _OnResult)
        {
            // authenticate user:
            PlayGamesPlatform.Instance.Authenticate(
                SignInInteractivity.CanPromptOnce, _Result =>
            {
                Dbg.Log($"{_Result}");
                switch (_Result)
                {
                    case SignInStatus.Success:
                        string login = PlayGamesPlatform.Instance.GetUserEmail();
                        string password = "google";
                        Login(login, password, _OnResult);
                        break;
                    case SignInStatus.Canceled:
                    case SignInStatus.Failed:
                    case SignInStatus.DeveloperError:
                    case SignInStatus.InternalError:
                    case SignInStatus.NotAuthenticated:
                    case SignInStatus.AlreadyInProgress:
                    case SignInStatus.UiSignInRequired:
                        _OnResult?.Invoke(AuthResult.LoginFailed);
                        break;
                    case SignInStatus.NetworkError:
                        _OnResult?.Invoke(AuthResult.FailedNoInternet);
                        break;
                    default:
                        throw new Common.Exceptions.SwitchCaseNotImplementedException(_Result);
                }
            });
        }

#elif  UNITY_IPHONE && !UNITY_EDITOR

        private void AuthenticateWithApple(UnityAction<AuthResult> _OnResult)
        {
            Social.localUser.Authenticate(_Success =>
            {
                if (!_Success)
                    _OnResult?.Invoke(AuthResult.LoginFailed);
                else
                {
                    string login = Social.localUser.id;
                    string password = "apple";
                    Login(login, password, _OnResult);
                }
            });
        }
        
#endif
        
#if UNITY_ANDROID
        
        //https://github.com/playgameservices/play-games-plugin-for-unity
        private void InitGooglePlayServices()
        {
            Dbg.Log("Play Games Init Start");
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .RequestEmail()
                //.RequestIdToken()
                //.RequestServerAuthCode(false)
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
#if DEVELOPMENT_BUILD
            PlayGamesPlatform.DebugLogEnabled = true;
#endif
            PlayGamesPlatform.Activate();
            Dbg.Log("Play Games Init End");
        }
        
#endif

        private void Login(string _Login, string _PasswordHash, UnityAction<AuthResult> _OnResult)
        {
            var loginPacket = new LoginUserPacket(new LoginUserPacketRequestArgs
                {
                    Name = _Login,
                    PasswordHash = _PasswordHash
                });
                loginPacket.OnSuccess(() =>
                    {
                        Dbg.Log("Login successfully");
                        GameClientUtils.AccountId = loginPacket.Response.Id;
                        _OnResult?.Invoke(AuthResult.LoginSuccess);
                    }
                );
                loginPacket.OnFail(() =>
                {
                    int errorId = loginPacket.ErrorMessage.Id;
                    if (errorId == ServerErrorCodes.AccountDoesNotExist)
                    {
                        Register(_Login, _PasswordHash, _OnResult);
                        Dbg.LogWarning("Account does not exist");
                    }
                    else if (errorId == ServerErrorCodes.WrongLoginOrPassword)
                    {
                        _OnResult?.Invoke(AuthResult.LoginFailed);
                        Dbg.LogError("Login failed: Wrong login or password");
                    }
                    else
                    {
                        _OnResult?.Invoke(AuthResult.LoginFailed);
                        Dbg.LogError(loginPacket.ErrorMessage);
                    }
                });
                GameClient.Send(loginPacket);
        }

        private void Register(string _Login, string _PasswordHash, UnityAction<AuthResult> _OnResult)
        {
            if (string.IsNullOrEmpty(_Login))
                return;
            
            var registerPacket = new RegisterUserPacket(
                new RegisterUserPacketRequestArgs
                {
                    Name = _Login,
                    PasswordHash = _PasswordHash,
                    GameId = GameClientUtils.GetDefaultGameId()
                });
            registerPacket.OnSuccess(() =>
                {
                    Dbg.Log("Registered successfully");
                    GameClientUtils.AccountId = registerPacket.Response.Id;
                    _OnResult?.Invoke(AuthResult.RegisterSuccess);
                })
                .OnFail(() =>
                {
                    _OnResult?.Invoke(AuthResult.RegisterFailed);
                    Dbg.LogError(registerPacket.ErrorMessage);
                });
            GameClient.Send(registerPacket);
        }

        #endregion
    }
}