using System.Text;
using Common.Constants;
using Common.Entities;
using Common.Network;
using Common.Ticker;
using SA.iOS.Foundation;
using SA.iOS.GameKit;
using UnityEngine.Events;
// ReSharper disable UnusedType.Global

namespace Common.Managers.Scores
{
    public class IosScoreManager : ScoreManagerBase
    {
        #region inject

        public IosScoreManager(
            IGameClient              _GameClient,
            ILocalizationManager     _LocalizationManager,
            ICommonTicker            _Ticker,
            IRemoteSavedGameProvider _RemoteSavedGameProvider) 
            : base(
                _GameClient,
                _LocalizationManager, 
                _Ticker, 
                _RemoteSavedGameProvider) { }

        #endregion

        #region api

        public override void Init()
        {
            RemoteSavedGameProvider.Init();
            base.Init();
        }

        public override ScoresEntity GetScoreFromLeaderboard(ushort _Id, bool _FromCache)
        {
            var entity = base.GetScoreFromLeaderboard(_Id, _FromCache);
            return entity ?? GetScoreIos(_Id);
        }

        public override bool SetScoreToLeaderboard(ushort _Id, long _Value, bool _OnlyToCache)
        {
            base.SetScoreToLeaderboard(_Id, _Value, _OnlyToCache);
            SetScoreIos(_Id, _Value);
            return true;
        }

        public override bool ShowLeaderboard(ushort _Id)
        {
            if (!base.ShowLeaderboard(_Id))
                return false;
            ShowLeaderboardIos(_Id);
            return true;
        }

        public override void DeleteSavedGame(string _FileName)
        {
            RemoteSavedGameProvider.DeleteSavedGame(_FileName);
        }

        public override Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache)
        {
            return _FromCache ? 
                GetSavedGameProgressFromCache(_FileName) : RemoteSavedGameProvider.GetSavedGame(_FileName);
        }

        public override void SaveGameProgress<T>(T _Data, bool _OnlyToCache)
        {
            SaveGameProgressToCache(_Data);
            if (_OnlyToCache)
                return;
            RemoteSavedGameProvider.SaveGame(_Data);
        }

        #endregion
        
        #region nonpublic methods
        
        protected override bool IsAuthenticatedInPlatformGameService()
        {
            return ISN_GKLocalPlayer.LocalPlayer.Authenticated;
        }

        protected override void AuthenticatePlatformGameService(UnityAction _OnFinish)
        {
            AuthenticateIos(_OnFinish);
        }

        private void AuthenticateIos(UnityAction _OnFinish)
        {
            ISN_GKLocalPlayer.SetAuthenticateHandler(_Result =>
            {
                if (_Result.IsSucceeded)
                {
                    var player = ISN_GKLocalPlayer.LocalPlayer;
                    var sb = new StringBuilder();
                    sb.AppendLine($"player game id: {player.GamePlayerId}");
                    sb.AppendLine($"player team id: {player.TeamPlayerId}");
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
                            RemoteSavedGameProvider.FetchSavedGames();
                        } 
                        else
                        {
                            Dbg.LogError("IdentityVerificationSignature has failed: " +
                                         $"{_SignatureResult.Error.FullMessage}");
                        }
                        _OnFinish?.Invoke();
                        _OnFinish = null;
                    });
                }
                else 
                {
                    Dbg.LogError(AuthMessage(false, 
                        $"Error with code: {_Result.Error.Code} and description: {_Result.Error.Message}"));
                }
            });
        }

        private void SetScoreIos(ushort _Id, long _Value)
        {
            if (!IsAuthenticatedInPlatformGameService())
            {
                Dbg.LogWarning("User is not authenticated to Game Center");
                return;
            }
            var scoreReporter = new ISN_GKScore(GetScoreKey(_Id))
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
        
        private ScoresEntity GetScoreIos(ushort _Id)
        {
            var scoreEntity = new ScoresEntity();
            var leaderboardRequest = new ISN_GKLeaderboard
            {
                Identifier = GetScoreKey(_Id),
                PlayerScope = ISN_GKLeaderboardPlayerScope.Global,
                TimeScope = ISN_GKLeaderboardTimeScope.AllTime,
                Range = new ISN_NSRange(1, 25)
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
                    scoreEntity = GetScoreCached(_Id, scoreEntity);
                    Dbg.LogWarning("Score Load failed! Code: " + _Result.Error.Code + " Message: " + _Result.Error.Message);
                }
            });
            return scoreEntity;
        }
        
        private void ShowLeaderboardIos(ushort _Id)
        {
            var viewController = new ISN_GKGameCenterViewController
            {
                ViewState = ISN_GKGameCenterViewControllerState.Leaderboards,
                LeaderboardIdentifier = GetScoreKey(_Id)
            };
            viewController.Show();
        }
        
        #endregion
    }
}