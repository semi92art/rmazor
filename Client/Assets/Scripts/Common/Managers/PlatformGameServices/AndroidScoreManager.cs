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
        
        private AndroidScoreManager(
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
        
        public override ScoresEntity GetScoreFromLeaderboard(ushort _Key, bool _FromCache)
        {
            var entity = base.GetScoreFromLeaderboard(_Key, _FromCache);
            return entity ?? GetScoreAndroid(_Key);
        }

        public override bool SetScoreToLeaderboard(ushort _Key, long _Value, bool _OnlyToCache)
        {
            base.SetScoreToLeaderboard(_Key, _Value, _OnlyToCache);
            if (!_OnlyToCache)
                SetScoreAndroid(_Key, _Value);
            return true;
        }

        public override bool ShowLeaderboard(ushort _Key)
        {
            if (!base.ShowLeaderboard(_Key))
                return false;
            ShowLeaderboardAndroid(_Key);
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
                SignInInteractivity.CanPromptOnce,
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

        private ScoresEntity GetScoreAndroid(ushort _Key)
        {
            var scoreEntity = new ScoresEntity();
            PlayGamesPlatform.Instance.LoadScores(
                GetScoreId(_Key),
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
                                scoreEntity.Value.Add(_Key, _Data.PlayerScore.value);
                                scoreEntity.Result = EEntityResult.Success;
                            }
                            else
                            {
                                Dbg.LogWarning("Remote score data PlayerScore is null");
                                scoreEntity = GetScoreCached(_Key, scoreEntity);
                            }
                        }
                        else
                        {
                            Dbg.LogWarning($"Remote score data status: {_Data.Status}");
                            scoreEntity = GetScoreCached(_Key, scoreEntity);
                        }
                    }
                    else
                    {
                        Dbg.LogWarning("Remote score data is not valid.");
                        scoreEntity = GetScoreCached(_Key, scoreEntity);
                    }
                });
            return scoreEntity;
        }
        
        private void SetScoreAndroid(ushort _Key, long _Value)
        {
            if (!IsAuthenticatedInPlatformGameService())
            {
                Dbg.LogWarning($"{nameof(SetScoreAndroid)}: User is not authenticated to ");
                return;
            }
            Dbg.Log(nameof(SetScoreAndroid));
            PlayGamesPlatform.Instance.ReportScore(
                _Value,
                GetScoreId(_Key),
                _Success =>
                {
                    if (!_Success)
                        Dbg.LogWarning("Failed to post leaderboard score");
                    else Dbg.Log($"Successfully put score {_Value} to leaderboard {DataFieldIds.GetDataFieldName(_Key)}");
                });
        }
        
        private void ShowLeaderboardAndroid(ushort _Key)
        {
            string id = GetScoreId(_Key);
            PlayGamesPlatform.Instance.ShowLeaderboardUI(id);
        }
        
        #endregion
    }
}
#endif