// ReSharper disable UnusedType.Global
#if UNITY_ANDROID
using Common.Constants;
using Common.Entities;
using Common.Helpers;
using Common.Network;
using Common.Ticker;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.Events;

namespace Common.Managers.Scores
{
    public class AndroidScoreManager : ScoreManagerBase, IApplicationPause
    {
        #region inject
        
        public AndroidScoreManager(
            CommonGameSettings       _Settings,
            IGameClient              _GameClient,
            ILocalizationManager     _LocalizationManager,
            ICommonTicker            _Ticker,
            IRemoteSavedGameProvider _RemoteSavedGameProvider)
            : base(
                _Settings,
                _GameClient,
                _LocalizationManager,
                _Ticker,
                _RemoteSavedGameProvider) { }

        #endregion

        #region api
        
        public override ScoresEntity GetScoreFromLeaderboard(ushort _Id, bool _FromCache)
        {
            var entity = base.GetScoreFromLeaderboard(_Id, _FromCache);
            return entity ?? GetScoreAndroid(_Id);
        }

        public override bool SetScoreToLeaderboard(ushort _Id, long _Value, bool _OnlyToCache)
        {
            base.SetScoreToLeaderboard(_Id, _Value, _OnlyToCache);
            if (!_OnlyToCache)
                SetScoreAndroid(_Id, _Value);
            return true;
        }

        public override bool ShowLeaderboard(ushort _Id)
        {
            if (!base.ShowLeaderboard(_Id))
                return false;
            ShowLeaderboardAndroid(_Id);
            return true;
        }

        public override void SaveGameProgress<T>(T _Data, bool _OnlyToCache)
        {
            SaveGameProgressToCache(_Data);
            if (_OnlyToCache)
                return;
            RemoteSavedGameProvider.SaveGame(_Data);
        }
        
        public override Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache)
        {
            return _FromCache ? 
                GetSavedGameProgressFromCache(_FileName) 
                : RemoteSavedGameProvider.GetSavedGame(_FileName);
        }
        
        public override void DeleteSavedGame(string _FileName)
        {
            RemoteSavedGameProvider.DeleteSavedGame(_FileName);
        }

        public void OnApplicationPause(bool _Pause)
        {
            // обращаемся к Google Play Services чтобы popup приветствия
            // появился именно на UnPause, а не в любой другой момент
            if (!_Pause)
                GetScoreAndroid(DataFieldIds.Level);
        }

        #endregion

        #region nonpublic methods
        
        protected override bool IsAuthenticatedInPlatformGameService()
        {
            return PlayGamesPlatform.Instance.IsAuthenticated();
        }

        protected override void AuthenticatePlatformGameService(UnityAction _OnFinish)
        {
            AuthenticateAndroid(_OnFinish);
        }
        
        private static void AuthenticateAndroid(UnityAction _OnFinish)
        {
            var config = new PlayGamesClientConfiguration.Builder()
                .EnableSavedGames()
                .Build();
            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(
                SignInInteractivity.NoPrompt,
                _Status =>
                {
                    if (_Status == SignInStatus.Success)
                    {
                        Dbg.Log(AuthMessage(true, string.Empty));
                        _OnFinish?.Invoke();
                    }
                    else
                        Dbg.LogWarning(AuthMessage(false, _Status.ToString()));
                });
        }

        private ScoresEntity GetScoreAndroid(ushort _Id)
        {
            var scoreEntity = new ScoresEntity();
            PlayGamesPlatform.Instance.LoadScores(
                GetScoreKey(_Id),
                LeaderboardStart.PlayerCentered,
                1,
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                _Data =>
                {
                    if (_Data.Valid)
                    {
                        if (_Data.Status == ResponseStatus.Success)
                        {
                            if (_Data.PlayerScore != null)
                            {
                                scoreEntity.Value.Add(_Id, _Data.PlayerScore.value);
                                scoreEntity.Result = EEntityResult.Success;
                            }
                            else
                            {
                                Dbg.LogWarning("Remote score data PlayerScore is null");
                                scoreEntity = GetScoreCached(_Id, scoreEntity);
                            }
                        }
                        else
                        {
                            Dbg.LogWarning($"Remote score data status: {_Data.Status}");
                            scoreEntity = GetScoreCached(_Id, scoreEntity);
                        }
                    }
                    else
                    {
                        Dbg.LogWarning("Remote score data is not valid.");
                        scoreEntity = GetScoreCached(_Id, scoreEntity);
                    }
                });
            return scoreEntity;
        }
        
        private void SetScoreAndroid(ushort _Id, long _Value)
        {
            if (!IsAuthenticatedInPlatformGameService())
            {
                Dbg.LogWarning($"{nameof(SetScoreAndroid)}: User is not authenticated to ");
                return;
            }
            Dbg.Log(nameof(SetScoreAndroid));
            PlayGamesPlatform.Instance.ReportScore(
                _Value,
                GetScoreKey(_Id),
                _Success =>
                {
                    if (!_Success)
                        Dbg.LogWarning("Failed to post leaderboard score");
                    else Dbg.Log($"Successfully put score {_Value} to leaderboard {DataFieldIds.GetDataFieldName(_Id)}");
                });
        }
        
        private void ShowLeaderboardAndroid(ushort _Id)
        {
            string key = GetScoreKey(_Id);
            PlayGamesPlatform.Instance.ShowLeaderboardUI(key);
        }
        
        #endregion
    }
}
#endif