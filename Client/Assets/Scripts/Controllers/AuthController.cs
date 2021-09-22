using Network;
using Network.Packets;
using UnityEngine;
using UnityEngine.Events;
using Utils;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif


namespace Controllers
{
    public class AuthController
    {
        #region api

        public enum AuthResult
        {
            LoginSuccess,
            RegisterSuccess,
            LoginFailed,
            RegisterFailed,
            FailedNoInternet
        }

        public AuthController()
        {
#if UNITY_ANDROID
            InitGooglePlayServices();
#elif  UNITY_IPHONE

#endif
        }

        public void Authenticate(UnityAction<AuthResult> _OnDoIfRegister)
        {
#if UNITY_EDITOR
            Login(SystemInfo.deviceUniqueIdentifier, "unity", _OnDoIfRegister);         
#elif UNITY_ANDROID
            AuthenticateWithGoogle(_OnDoIfRegister);
#elif UNITY_IPHONE
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
                Utils.Dbg.Log($"{_Result}");
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
                        throw new SwitchCaseNotImplementedException(_Result);
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
                GameClient.Instance.Send(loginPacket);
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
                    GameId = GameClientUtils.DefaultGameId
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
            GameClient.Instance.Send(registerPacket);
        }

        #endregion
    }
}

//--------------------Apple Game Center integration example 1------------------

// using UnityEngine;
// using UnityEngine.SocialPlatforms;
//
// public class SocialExample : MonoBehaviour {
//     
//     void Start () {
//         // Authenticate and register a ProcessAuthentication callback
//         // This call needs to be made before we can proceed to other calls in the Social API
//         Social.localUser.Authenticate (ProcessAuthentication);
//     }
//
//     // This function gets called when Authenticate completes
//     // If the operation is successful, Social.localUser will contain data from the server. 
//     void ProcessAuthentication (bool success) {
//         if (success) {
//             Dbg.Log ("Authenticated, checking achievements");
//
//             // Request loaded achievements, and register a callback for processing them
//             Social.LoadAchievements (ProcessLoadedAchievements);
//         }
//         else
//             Dbg.Log ("Failed to authenticate");
//     }
//
//     // This function gets called when the LoadAchievement call completes
//     void ProcessLoadedAchievements (IAchievement[] achievements) {
//         if (achievements.Length == 0)
//             Dbg.Log ("Error: no achievements found");
//         else
//             Dbg.Log ("Got " + achievements.Length + " achievements");
//      
//         // You can also call into the functions like this
//         Social.ReportProgress ("Achievement01", 100.0, result => {
//             if (result)
//                 Dbg.Log ("Successfully reported achievement progress");
//             else
//                 Dbg.Log ("Failed to report achievement");
//         });
//     }
// }

//-----------------Apple Game Center integration example 2------------------

// using UnityEngine;
// using System.Collections;
// using UnityEngine.SocialPlatforms;
// using UnityEngine.SocialPlatforms.GameCenter;
//
// public class GameCenter : MonoBehaviour {
// 	
// 	static ILeaderboard m_Leaderboard;
// 	public int highScoreInt = 1000;
// 	
// 	public string leaderboardName = "leaderboard01";
// 	public string leaderboardID = "com.company.game.leaderboardname";
// 	
// 	public string achievement1Name = "com.compnayname.demo.10m";
// 	
// 	public string achievement2Name = "com.companyname.demo.achiev1";
// 	public string achievement3Name = "com.companyname.demo.achievement3";
// 	public string achievement4Name = "com.companyname.demo.achievement4";
// 	
// 	bool gameOver = false;
// 	
// // THIS MAKES SURE THE GAME CENTER INTEGRATION WILL ONLY WORK WHEN OPERATING ON AN APPLE IOS DEVICE (iPHONE, iPOD TOUCH, iPAD)
// //#if UNITY_IPHONE
// 	
// 	// Use this for initialization
// 	void Start () {
// 	
// 		 // AUTHENTICATE AND REGISTER A ProcessAuthentication CALLBACK
// 		// THIS CALL NEEDS OT BE MADE BEFORE WE CAN PROCEED TO OTHER CALLS IN THE Social API
//         Social.localUser.Authenticate (ProcessAuthentication);
// 		
// 		// GET INSTANCE OF LEADERBOARD
// 		DoLeaderboard();
// 		
// 	}
// 	
// 	// Update is called once per frame
// 	void Update () {
// 	
// 		#if UNITY_IPHONE
// 		if(gameOver == true) {
// 			// REPORT THE PLAYER'S SCORE WHEN THE GAME IS OVER USE A GUI BUTTON TO FLIP THE BOOLEAN FROM FALSE TO TRUE SO THIS GETS CALLED
// 			ReportScore(highScoreInt, leaderboardID);
// 		}
// 		#endif
// 		
// 	}
// 	
// 	// THE UI BELOW CONTAINING GUI BUTTONS IS USED TO DEMONSTRATE THE GAME CENTER INTEGRATION
// 	// HERE, YOU ARE ABLE TO:
// 	// (1) VIEW LEADERBOARDS 
// 	// (2) VIEW ACHIEVEMENTS
// 	// (3) SUBMIT HIGH SCORE TO LEADERBOARD
// 	// (4) REPORT ACHIEVEMENTS ACQUIRED
// 	// (5) RESET ACHIEVEMENTS.
// 	void OnGUI () {	
// 		
// 		// COLUMN 1
// 		// SHOW LEADERBOARDS WITHIN GAME CENTER
// 		if(GUI.Button(new Rect(20, 20, 200, 75), "View Leaderboard")) {
// 			Social.ShowLeaderboardUI();
// 		}
// 		
// 		// SHOW ACHIEVEMENTS WITHIN GAME CENTER
// 		if(GUI.Button(new Rect(20, 100, 200, 75), "View Achievements")) {
// 			Social.ShowAchievementsUI();
// 		}
// 		
// 		// SET GAME OVER SWITCH
// 		if(GUI.Button(new Rect(20, 180, 200, 75), "Game Over Switch")) {
// 			// ONCE TRUE, THE UPDATE WILL HIT AND HIGH SCORE WILL BE SUBMITTED
// 			gameOver = true;
// 		}
// 		
// 		// RESET ALL ACHIEVEMENTS
// 		if(GUI.Button(new Rect(20, 260, 200, 75), "Reset Achievements")) {
// 			GameCenterPlatform.ResetAllAchievements((resetResult) => {
// 				Dbg.Log(resetResult ? "Achievements have been Reset" : "Achievement reset failure.");
// 			});
// 		}
// 		
// 		// COLUMN 2
// 		// ENABLE ACHIEVEMENT 1
// 		if(GUI.Button(new Rect(225, 20, 200, 75), "Report Achievement 1")) {
// 			ReportAchievement(achievement1Name, 100.00);
// 		}
// 		
// 		// ENABLE ACHIEVEMENT 2
// 		if(GUI.Button(new Rect(225, 100, 200, 75), "Report Achievement 2")) {
// 			ReportAchievement(achievement2Name, 100.00);
// 		}
// 		
// 		// ENABLE ACHIEVEMENT 3
// 		if(GUI.Button(new Rect(225, 180, 200, 75), "Report Achievement 3")) {
// 			ReportAchievement(achievement3Name, 100.00);
// 		}
// 		
// 		// ENABLE ACHIEVEMENT 4
// 		if(GUI.Button(new Rect(225, 260, 200, 75), "Report Achievement 4")) {
// 			ReportAchievement(achievement4Name, 100.00);
// 		}
// 	
//     }
// 	
// 	///////////////////////////////////////////////////
// 	// INITAL AUTHENTICATION (MUST BE DONE FIRST)
// 	///////////////////////////////////////////////////
// 	
// 	// THIS FUNCTION GETS CALLED WHEN AUTHENTICATION COMPLETES
// 	// NOTE THAT IF THE OPERATION IS SUCCESSFUL Social.localUser WILL CONTAIN DATA FROM THE GAME CENTER SERVER
//     void ProcessAuthentication (bool success) {
//         if (success) {
//             Dbg.Log ("Authenticated, checking achievements");
//
// 			// MAKE REQUEST TO GET LOADED ACHIEVEMENTS AND REGISTER A CALLBACK FOR PROCESSING THEM
//             Social.LoadAchievements (ProcessLoadedAchievements); // ProcessLoadedAchievements FUNCTION CAN BE FOUND BELOW
// 			
// 			Social.LoadScores(leaderboardName, scores => {
//     			if (scores.Length > 0) {
// 					// SHOW THE SCORES RECEIVED
//         			Dbg.Log ("Received " + scores.Length + " scores");
//         			string myScores = "Leaderboard: \n";
//         			foreach (IScore score in scores)
//             			myScores += "\t" + score.userID + " " + score.formattedValue + " " + score.date + "\n";
//         			Dbg.Log (myScores);
//     			}
//     			else
//         			Dbg.Log ("No scores have been loaded.");
// 				});
//         }
//         else
//             Dbg.Log ("Failed to authenticate with Game Center.");
//     }
// 	
// 	
// 	// THIS FUNCTION GETS CALLED WHEN THE LoadAchievements CALL COMPLETES
//     void ProcessLoadedAchievements (IAchievement[] achievements) {
//         if (achievements.Length == 0)
//             Dbg.Log ("Error: no achievements found");
//         else
//             Dbg.Log ("Got " + achievements.Length + " achievements");
//
//         // You can also call into the functions like this
//         Social.ReportProgress ("Achievement01", 100.0, result => {
//             if (result)
//                 Debug.Log ("Successfully reported achievement progress");
//             else
//                 Debug.Log ("Failed to report achievement");
//         });
// 		//Social.ShowAchievementsUI();
//     }
// 	
// 	///////////////////////////////////////////////////
// 	// GAME CENTER ACHIEVEMENT INTEGRATION
// 	///////////////////////////////////////////////////
//
// 	void ReportAchievement( string achievementId, double progress ){
// 		Social.ReportProgress( achievementId, progress, (result) => {
// 			Debug.Log( result ? string.Format("Successfully reported achievement {0}", achievementId) 
// 			          : string.Format("Failed to report achievement {0}", achievementId));
// 		});
// 	}
//
// 	#region Game Center Integration
// 	///////////////////////////////////////////////////
// 	// GAME CENTER LEADERBOARD INTEGRATION
// 	///////////////////////////////////////////////////
// 	
// 	
// 	/// <summary>
// 	/// Reports the score to the leaderboards.
// 	/// </summary>
// 	/// <param name="score">Score.</param>
// 	/// <param name="leaderboardID">Leaderboard I.</param>
// 	void ReportScore (long score, string leaderboardID) {
//     	Debug.Log ("Reporting score " + score + " on leaderboard " + leaderboardID);
//     	Social.ReportScore (score, leaderboardID, success => {
//         	Debug.Log(success ? "Reported score to leaderboard successfully" : "Failed to report score");
//     	});
// 	}
// 	
// 	/// <summary>
// 	/// Get the leaderboard.
// 	/// </summary>
// 	void DoLeaderboard () {
//     	m_Leaderboard = Social.CreateLeaderboard();
//     	m_Leaderboard.id = leaderboardID;  // YOUR CUSTOM LEADERBOARD NAME
//     	m_Leaderboard.LoadScores(result => DidLoadLeaderboard(result));
// 	}
//
// 	/// <summary>
// 	/// RETURNS THE NUMBER OF LEADERBOARD SCORES THAT WERE RECEIVED BY THE APP
// 	/// </summary>
// 	/// <param name="result">If set to <c>true</c> result.</param>
// 	void DidLoadLeaderboard (bool result) {
//     	Debug.Log("Received " + m_Leaderboard.scores.Length + " scores");
//     	foreach (IScore score in m_Leaderboard.scores) {
//         	Debug.Log(score);
// 		}
// 		//Social.ShowLeaderboardUI();
// 	}
//
// 	#endregion
// }
//
// //#endif