using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using GameHelpers;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public delegate void ScoresEventHandler(ScoresEventArgs _Args);

    public class ScoresEventArgs
    {
        public ScoresEntity ScoresEntity { get; }

        public ScoresEventArgs(ScoresEntity _ScoresEntity)
        {
            ScoresEntity = _ScoresEntity;
        }
    }

    public interface IScoreManager : IInit
    {
        event ScoresEventHandler OnScoresChanged;
        ScoresEntity GetScore(ushort _Id);
        void SetScore(ushort _Id, long _Value);
        void ShowLeaderboard();
    }
    
    public class ScoreManager : IScoreManager
    {
        #region types

        private class ScoreArgs
        {
            public ushort Id         { get; }
            public string Key        { get; }
            public bool   OnlyCached { get; }

            public ScoreArgs(ushort _Id, string _Key, bool _OnlyCached)
            {
                Id = _Id;
                Key = _Key;
                OnlyCached = _OnlyCached;
            }
        }

        #endregion
        
        #region nonpublic members

        private readonly IReadOnlyList<ScoreArgs> m_ScoreArgsList = new List<ScoreArgs>
        {
            new ScoreArgs(DataFieldIds.Money, Application.platform == RuntimePlatform.Android ? GPGSIds.coins : "money", false),
            new ScoreArgs(DataFieldIds.Level, "level", true)
        };

        #region inject

        private ILocalizationManager LocalizationManager { get; }
        
        public ScoreManager(ILocalizationManager _LocalizationManager)
        {
            LocalizationManager = _LocalizationManager;
        }

        #endregion

        #endregion
        
        #region api

        public event ScoresEventHandler OnScoresChanged;
        
        public event UnityAction Initialized;
        public void Init()
        {
            AuthenticatePlatformGameService(() => Initialized?.Invoke());
        }

        public ScoresEntity GetScore(ushort _Id)
        {
            if (IsScoreOnlyCached(_Id))
                return GetScoreCached(_Id);
            
            var scoreEntity = new ScoresEntity{Result = EEntityResult.Pending};
            if (!IsAuthenticatedInPlatformGameService())
            {
                Dbg.LogWarning($"{nameof(GetScore)}: User is not authenticated");
                GetScoreCached(_Id, ref scoreEntity);
                return scoreEntity;
            }
            
#if UNITY_EDITOR
            return GetScoreCached(_Id);
#elif UNITY_ANDROID
            return GetScoreAndroid(_Id);
#elif UNITY_IPHONE || UNITY_IOS
            return GetScoreIos(_Id);
#endif
        }

        public void SetScore(ushort _Id, long _Value)
        {
            SetScoreCache(_Id, _Value);
            if (IsScoreOnlyCached(_Id))
                return;
#if UNITY_ANDROID && !UNITY_EDITOR
            SetScoreAndroid(_Id, _Value);
#elif UNITY_IPHONE && !UNITY_EDITOR
            SetScoreIos(_Id, _Value);
#endif
        }

        public void ShowLeaderboard()
        {
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                string noIntConnText = LocalizationManager.GetTranslation("no_internet_connection");
                Dbg.LogWarning($"{nameof(ShowLeaderboard)}: {noIntConnText}");
                CommonUtils.ShowAlertDialog("OOPS!!!", noIntConnText);
                return;
            }
            if (!IsAuthenticatedInPlatformGameService())
            {
                string failedToLoadLeadText = LocalizationManager.GetTranslation("failed_to_load_lead");
                Dbg.LogWarning($"{nameof(ShowLeaderboard)}: {failedToLoadLeadText}");
                CommonUtils.ShowAlertDialog("OOPS!!!", failedToLoadLeadText);
                return;
            }
#if UNITY_EDITOR
            CommonUtils.ShowAlertDialog("Предупреждение", "Доступно только на девайсе.");
#elif UNITY_ANDROID
            ShowLeaderboardAndroid();
#elif UNITY_IPHONE || UNITY_IOS
            ShowLeaderboardIos();
#endif
        }

        #endregion

        #region nonpublic methods
        
        private static bool IsAuthenticatedInPlatformGameService()
        {
#if UNITY_EDITOR
            return true;
#elif UNITY_ANDROID
            return GooglePlayGames.PlayGamesPlatform.Instance.IsAuthenticated();
#elif UNITY_IPHONE || UNITY_IOS
            return SA.iOS.GameKit.ISN_GKLocalPlayer.LocalPlayer.Authenticated;
#endif
        }

        private static void AuthenticatePlatformGameService(UnityAction _OnSucceed)
        {
#if UNITY_EDITOR
            return;
#endif
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                Dbg.LogWarning(AuthMessage(false, "No internet connection"));
                return;
            }
            
#if UNITY_ANDROID
            AuthenticateAndroid(_OnSucceed);
#elif UNITY_IPHONE || UNITY_IOS
            AuthenticateIos(_OnSucceed);
#endif
        }

        private static void GetScoreCached(ushort _Id, ref ScoresEntity _ScoresEntity)
        {
            _ScoresEntity = GetScoreCached(_Id);
        }
        
        private static ScoresEntity GetScoreCached(ushort _Id)
        {
            Dbg.Log(nameof(GetScoreCached) + ": " + DataFieldIds.GetDataFieldName(_Id));
            var scores = new ScoresEntity{Result = EEntityResult.Pending};
            var gdff = new GameDataFieldFilter(GameClientUtils.AccountId, GameClientUtils.GameId,
                _Id) {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var scoreField = _Fields.First();
                scores.Value.Add(_Id, scoreField.ToInt());
                scores.Result = EEntityResult.Success;
            });
            return scores;
        }

        private void SetScoreCache(ushort _Id, long _Value)
        {
            var gdff = new GameDataFieldFilter(GameClientUtils.AccountId, GameClientUtils.GameId,
                _Id) {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var scoreField = _Fields.First();
                scoreField.SetValue(_Value).Save(true);
            });
            Dbg.Log(nameof(OnScoresChanged));
            OnScoresChanged?.Invoke(new ScoresEventArgs(GetScoreCached(_Id)));
        }
        
        private string GetScoreKey(ushort _Id)
        {
            var args = m_ScoreArgsList.FirstOrDefault(_Args => _Args.Id == _Id);
            if (args != null) 
                return m_ScoreArgsList.FirstOrDefault(_Args => _Args.Id == _Id)?.Key;
            Dbg.LogError($"Score with id {_Id} does not exist.");
            return null;
        }

        private bool IsScoreOnlyCached(ushort _Id)
        {
            var args = m_ScoreArgsList.FirstOrDefault(_Args => _Args.Id == _Id);
            if (args != null) 
                return args.OnlyCached;
            Dbg.LogError($"Score with id {_Id} does not exist.");
            return true;
        }

#if UNITY_ANDROID

        private static void AuthenticateAndroid(UnityAction _OnSucceed)
        {
            GooglePlayGames.PlayGamesPlatform.Instance.Authenticate((_Success, _Messsage) =>
            {
                Dbg.LogWarning(AuthMessage(_Success, _Messsage));
                if (_Success)
                    _OnSucceed?.Invoke();
            });
        }

        private ScoresEntity GetScoreAndroid(ushort _Id)
        {
            var scoreEntity = new ScoresEntity{Result = EEntityResult.Pending};
            GooglePlayGames.PlayGamesPlatform.Instance.LoadScores(
                GetScoreKey(_Id),
                GooglePlayGames.BasicApi.LeaderboardStart.PlayerCentered,
                1,
                GooglePlayGames.BasicApi.LeaderboardCollection.Public,
                GooglePlayGames.BasicApi.LeaderboardTimeSpan.AllTime,
                _Data =>
                {
                    if (_Data.Valid)
                    {
                        if (_Data.Status == GooglePlayGames.BasicApi.ResponseStatus.Success)
                        {
                            if (_Data.PlayerScore != null)
                            {
                                scoreEntity.Value.Add(_Id, _Data.PlayerScore.value);
                                scoreEntity.Result = EEntityResult.Success;
                            }
                            else
                            {
                                Dbg.LogWarning($"Remote score data PlayerScore is null");
                                GetScoreCached(_Id, ref scoreEntity);
                            }
                        }
                        else
                        {
                            Dbg.LogWarning($"Remote score data status: {_Data.Status}");
                            GetScoreCached(_Id, ref scoreEntity);
                        }
                    }
                    else
                    {
                        Dbg.LogWarning("Remote score data is not valid.");
                        GetScoreCached(_Id, ref scoreEntity);
                    }
                });
            return scoreEntity;
        }
        
        private void SetScoreAndroid(ushort _Id, long _Value)
        {
            if (!IsAuthenticatedInPlatformGameService())
            {
                Dbg.LogWarning($"{nameof(SetScoreAndroid)}: User is not authenticated to GooglePlayGames.");
                return;
            }
            GooglePlayGames.PlayGamesPlatform.Instance.ReportScore(
                _Value,
                GetScoreKey(_Id),
                _Success =>
                {
                    if (!_Success)
                        Dbg.LogWarning("Failed to post leaderboard score");
                });
        }
        
        private static void ShowLeaderboardAndroid()
        {
            GooglePlayGames.PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.coins);
        }

#elif UNITY_IPHONE || UNITY_IOS

        private static void AuthenticateIos(UnityAction _OnSuccess)
        {
            SA.iOS.GameKit.ISN_GKLocalPlayer.SetAuthenticateHandler(_Result =>
            {
                if (_Result.IsSucceeded)
                {
                    var player = SA.iOS.GameKit.ISN_GKLocalPlayer.LocalPlayer;
                    var sb = new System.Text.StringBuilder();
                    sb.Append($"player id: {player.PlayerID}\n");
                    sb.Append($"player Alias: {player.Alias}\n");
                    sb.Append($"player DisplayName: {player.DisplayName}\n");
                    sb.Append($"player Authenticated: {player.Authenticated}\n");
                    sb.Append($"player Underage: {player.Underage}\n");

                    player.GenerateIdentityVerificationSignatureWithCompletionHandler(_SignatureResult =>
                    {
                        if(_SignatureResult.IsSucceeded) 
                        {
                            Dbg.Log($"signatureResult.PublicKeyUrl: {_SignatureResult.PublicKeyUrl}");
                            Dbg.Log($"signatureResult.Timestamp: {_SignatureResult.Timestamp}");
                            Dbg.Log($"signatureResult.Salt.Length: {_SignatureResult.Salt.Length}");
                            Debug.Log($"signatureResult.Signature.Length: {_SignatureResult.Signature.Length}");
                        } 
                        else 
                        {
                            Dbg.LogError($"IdentityVerificationSignature has failed: {_SignatureResult.Error.FullMessage}");
                        }
                    });
                }
                else 
                {
                    Dbg.LogError(AuthMessage(false, 
                        $"Error with code: {_Result.Error.Code} and description: {_Result.Error.Message}"));
                }
            });
        }

        private ScoresEntity GetScoreIos(ushort _Id)
        {
            var scoreEntity = new ScoresEntity{Result = EEntityResult.Pending};

            var leaderboardRequest = new SA.iOS.GameKit.ISN_GKLeaderboard
            {
                Identifier = GetScoreKey(_Id),
                PlayerScope = SA.iOS.GameKit.ISN_GKLeaderboardPlayerScope.Global,
                TimeScope = SA.iOS.GameKit.ISN_GKLeaderboardTimeScope.AllTime,
                Range = new SA.iOS.Foundation.ISN_NSRange(1, 25)
            };
            
            leaderboardRequest.LoadScores(_Result => 
            {
                if (_Result.IsSucceeded) 
                {
                    scoreEntity.Value.Add(_Id, (int)leaderboardRequest.LocalPlayerScore.Value);
                    scoreEntity.Result = EEntityResult.Success;
                } else
                {
                    GetScoreCached(_Id, ref scoreEntity);
                    Dbg.LogWarning("Score Load failed! Code: " + _Result.Error.Code + " Message: " + _Result.Error.Message);
                }
            });
            return scoreEntity;
        }
        
        private void SetScoreIos(ushort _Id, long _Value)
        {
            if (!IsAuthenticatedInPlatformGameService())
            {
                Dbg.LogWarning("User is not authenticated to Game Center");
                return;
            }
            
            var scoreReporter = new SA.iOS.GameKit.ISN_GKScore(GetScoreKey(_Id))
            {
                Value = _Value
            };

            scoreReporter.Report(_Result =>
            {
                if (_Result.IsSucceeded) 
                    Dbg.Log("Score Report Success");
                else
                {
                    Dbg.LogError("Score Report failed! Code: " + 
                                 _Result.Error.Code + " Message: " + _Result.Error.Message);
                }
            });
        }
        
        private static void ShowLeaderboardIos()
        {
            var viewController = new SA.iOS.GameKit.ISN_GKGameCenterViewController
            {
                ViewState = SA.iOS.GameKit.ISN_GKGameCenterViewControllerState.Leaderboards
            };
            viewController.Show();
        }

#endif
        
        private static string AuthMessage(bool _Success, string _AddMessage)
        {
            return $"{(_Success ? "Success" : "Fail")} on authentication to game service: {_AddMessage}";
        }

        #endregion
    }
}