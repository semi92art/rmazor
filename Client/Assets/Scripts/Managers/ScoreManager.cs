using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Network;
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
        ScoresEntity             GetScore(ushort _Id, bool _FromCache);
        void                     SetScore(ushort _Id, long _Value, bool _OnlyToCache);
        void                     ShowLeaderboard();
    }
    
    public class ScoreManager : IScoreManager
    {
        #region types

        private class ScoreArgs
        {
            public ushort Id  { get; }
            public string Key { get; }

            public ScoreArgs(ushort _Id, string _Key)
            {
                Id = _Id;
                Key = _Key;
            }
        }

        #endregion
        
        #region nonpublic members

        private readonly IReadOnlyList<ScoreArgs> m_ScoreArgsList = new List<ScoreArgs>
        {
            new ScoreArgs(DataFieldIds.Money, Application.platform == RuntimePlatform.Android ? GPGSIds.coins : "coins"),
            new ScoreArgs(DataFieldIds.Level, "level")
        };
        
        private readonly Dictionary<ushort, bool> m_GlobalScoresLoaded = new Dictionary<ushort, bool>();

        #endregion
        
        #region inject

        private IGameClient          GameClient          { get; }
        private ILocalizationManager LocalizationManager { get; }
        
        public ScoreManager(IGameClient _GameClient, ILocalizationManager _LocalizationManager)
        {
            GameClient = _GameClient;
            LocalizationManager = _LocalizationManager;
        }
        
        #endregion
        
        #region api

        public event ScoresEventHandler OnScoresChanged;

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        public void Init()
        {
            AuthenticatePlatformGameService(() => Initialize?.Invoke());
        }

        public ScoresEntity GetScore(ushort _Id, bool _FromCache)
        {
            if (_FromCache)
                return GetScoreCached(_Id);
            
            var scoreEntity = new ScoresEntity{Result = EEntityResult.Pending};
            if (!IsAuthenticatedInPlatformGameService())
            {
                Dbg.LogWarning($"{nameof(GetScore)}: User is not authenticated");
                GetScoreCached(_Id, ref scoreEntity);
                return scoreEntity;
            }
            
#if UNITY_ANDROID
            return GetScoreAndroid(_Id);
#elif UNITY_IPHONE || UNITY_IOS
            return GetScoreIos(_Id);
#endif
        }

        public void SetScore(ushort _Id, long _Value, bool _OnlyToCache)
        {
            SetScoreCache(_Id, _Value);
            if (_OnlyToCache)
                return;
#if UNITY_ANDROID
            SetScoreAndroid(_Id, _Value);
#elif UNITY_IPHONE
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
            return false;
#elif UNITY_ANDROID
            return GooglePlayGames.PlayGamesPlatform.Instance.IsAuthenticated();
#elif UNITY_IPHONE || UNITY_IOS
            return SA.iOS.GameKit.ISN_GKLocalPlayer.LocalPlayer.Authenticated;
#endif
        }

        private static void AuthenticatePlatformGameService(UnityAction _OnFinish)
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
            AuthenticateAndroid(_OnFinish);
#elif UNITY_IPHONE || UNITY_IOS
            AuthenticateIos(_OnFinish);
#endif
        }

        private void GetScoreCached(ushort _Id, ref ScoresEntity _ScoresEntity)
        {
            _ScoresEntity = GetScoreCached(_Id);
        }
        
        private ScoresEntity GetScoreCached(ushort _Id)
        {
            Dbg.Log(nameof(GetScoreCached) + ": " + DataFieldIds.GetDataFieldName(_Id));
            var scores = new ScoresEntity{Result = EEntityResult.Pending};
            var gdff = new GameDataFieldFilter(GameClient, GameClientUtils.AccountId, GameClientUtils.GameId,
                _Id) {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var scoreField = _Fields.FirstOrDefault();
                if (scoreField == null)
                {
                    scores.Result = EEntityResult.Fail;
                }
                else
                {
                    scores.Result = EEntityResult.Success;
                    scores.Value.Add(_Id, scoreField.ToInt());
                }
            });
            return scores;
        }

        private void SetScoreCache(ushort _Id, long _Value)
        {
            var gdff = new GameDataFieldFilter(GameClient, GameClientUtils.AccountId, GameClientUtils.GameId,
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

#if UNITY_ANDROID

        private static void AuthenticateAndroid(UnityAction _OnFinish)
        {
            GooglePlayGames.PlayGamesPlatform.Instance.Authenticate((_Success, _Messsage) =>
            {
                _OnFinish?.Invoke();
                if (_Success)
                {
                    Dbg.Log(AuthMessage(true, _Messsage));
                }
                else 
                    Dbg.LogWarning(AuthMessage(true, _Messsage));
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
                                m_GlobalScoresLoaded.SetSafe(_Id, true);
                                scoreEntity.Value.Add(_Id, _Data.PlayerScore.value);
                                scoreEntity.Result = EEntityResult.Success;
                            }
                            else
                            {
                                Dbg.LogWarning("Remote score data PlayerScore is null");
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

        private static void AuthenticateIos(UnityAction _OnFinish)
        {
            SA.iOS.GameKit.ISN_GKLocalPlayer.SetAuthenticateHandler(_Result =>
            {
                if (_Result.IsSucceeded)
                {
                    var player = SA.iOS.GameKit.ISN_GKLocalPlayer.LocalPlayer;
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"player id: {player.PlayerID}");
                    sb.AppendLine($"player Alias: {player.Alias}");
                    sb.AppendLine($"player DisplayName: {player.DisplayName}");
                    sb.AppendLine($"player Authenticated: {player.Authenticated}");
                    sb.AppendLine($"player Underage: {player.Underage}");
                    Dbg.Log(sb.ToString());

                    player.GenerateIdentityVerificationSignatureWithCompletionHandler(_SignatureResult =>
                    {
                        if(_SignatureResult.IsSucceeded)
                        {
                            sb.Clear();
                            sb.AppendLine($"signatureResult.PublicKeyUrl: {_SignatureResult.PublicKeyUrl}");
                            sb.AppendLine($"signatureResult.Timestamp: {_SignatureResult.Timestamp}");
                            sb.AppendLine($"signatureResult.Salt.Length: {_SignatureResult.Salt.Length}");
                            sb.AppendLine($"signatureResult.Signature.Length: {_SignatureResult.Signature.Length}");
                            Dbg.Log(sb.ToString());
                        } 
                        else 
                        {
                            Dbg.LogError($"IdentityVerificationSignature has failed: {_SignatureResult.Error.FullMessage}");
                        }
                        _OnFinish?.Invoke();
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
                    Dbg.Log($"Score Load Succeed: {DataFieldIds.GetDataFieldName(_Id)}: {leaderboardRequest.LocalPlayerScore.Value}");
                    scoreEntity.Value.Add(_Id, leaderboardRequest.LocalPlayerScore.Value);
                    scoreEntity.Result = EEntityResult.Success;
                } 
                else
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