using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Network;
using Common.Ticker;
using Common.Utils;
using UnityEngine.Events;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;

namespace Managers.Scores
{
    public class AndroidScoreManager : ScoreManagerBase, IApplicationPause, IUpdateTick
    {
        #region types

        private class SavedGameInfo
        {
            public byte[]             Data     { get; set; }
            public ISavedGameMetadata MetaData { get; set; }
            public Entity<object>     Entity   { get; set; }
        }

        private class SaveGameOperationInfo
        {
            /// <summary>
            /// <param>true</param>
            /// <c>true</c> when need to read and <c>false</c> when need to write
            /// </summary>
            public bool ReadOrWrite { get; set; }

            public SavedGameInfo SavedGame { get; set; }
            public string        FileName  { get; set; }
            public bool          Started  { get; set; }
            public bool          Finished  { get; set; }
        }

        #endregion
        
        #region nonpublic members

        private          bool                         m_OperationInfosQueueNotEmpty;
        private readonly Queue<SaveGameOperationInfo> m_OperationInfosQueue = new Queue<SaveGameOperationInfo>();
        private          SaveGameOperationInfo        m_CurrentOperationCached;

        #endregion

        #region inject
        
        public AndroidScoreManager(
            IGameClient          _GameClient,
            ILocalizationManager _LocalizationManager,
            ICommonTicker        _Ticker)
            : base(_GameClient, _LocalizationManager, _Ticker) { }

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
            var info = new SavedGameInfo
            {
                Data = CommonUtils.ToByteArray(_Data)
            };
            m_OperationInfosQueue.Enqueue(new SaveGameOperationInfo
            {
                SavedGame = info,
                FileName = _Data.FileName,
                ReadOrWrite = false
            });
            m_OperationInfosQueueNotEmpty = true;
        }
        
        public override Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache)
        {
            if (_FromCache)
                return GetSavedGameProgressFromCache(_FileName);
            var info = new SavedGameInfo
            {
                Entity = new Entity<object>{Result = EEntityResult.Pending}
            };
            m_OperationInfosQueue.Enqueue(new SaveGameOperationInfo
            {
                SavedGame = info,
                FileName = _FileName,
                ReadOrWrite = true
            });
            m_OperationInfosQueueNotEmpty = true;
            return info.Entity;
        }
        
        public override void DeleteSavedGame(string _FileName)
        {
            DeleteGameData(_FileName);
        }

        public void OnApplicationPause(bool _Pause)
        {
            // FIXME ебаный костыль
            if (!_Pause)
                GetScoreAndroid(DataFieldIds.Level);
        }
        
        public void UpdateTick()
        {
            if (!m_OperationInfosQueueNotEmpty)
                return;
            var operation = m_CurrentOperationCached ??= m_OperationInfosQueue.Peek();
            if (!operation.Started)
            {
                void OnDelay()
                {
                    if (!m_OperationInfosQueue.Any())
                        return;
                    if (!ReferenceEquals(m_OperationInfosQueue.Peek(), operation))
                        return;
                    if (operation.SavedGame?.Entity != null)
                        operation.SavedGame.Entity.Result = EEntityResult.Fail;
                    operation.Finished = true;
                }
                operation.Started = true;
                Cor.Run(Cor.Delay(10f, OnDelay));
                OpenSavedGame(operation.FileName);
                return;
            }
            if (!operation.Finished)
                return;
            m_CurrentOperationCached = null;
            m_OperationInfosQueue.Dequeue();
            m_OperationInfosQueueNotEmpty = m_OperationInfosQueue.Any();
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
                // .RequestServerAuthCode(false)
                // .RequestIdToken()
                // .RequestEmail()
                .Build();
            PlayGamesPlatform.InitializeInstance(config);
            // PlayGamesPlatform.DebugLogEnabled = true;
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

        private ScoresEntity GetScoreAndroid(ushort _Id)
        {
            var scoreEntity = new ScoresEntity{Result = EEntityResult.Pending};
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
        
        private void OpenSavedGame(string _Filename)
        {
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                Dbg.LogWarning("Not authenticated to PlayGames");
                return;
            }
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            if (savedGameClient == null)
            {
                Dbg.LogWarning(nameof(savedGameClient) + " is null");
                return;
            }
            savedGameClient.OpenWithAutomaticConflictResolution(_Filename, DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, OnSavedGameOpened);
        }

        private void OnSavedGameOpened(SavedGameRequestStatus _Status, ISavedGameMetadata _Game) 
        {
            if (_Status == SavedGameRequestStatus.Success)
            {
                if (!m_OperationInfosQueue.Any())
                    return;
                var operation = m_OperationInfosQueue.Peek();
                operation.SavedGame.MetaData = _Game;
                if (operation.ReadOrWrite)
                {
                    LoadGameData(_Game);
                }
                else
                {
                    SaveGame(
                        operation.SavedGame.MetaData, 
                        operation.SavedGame.Data, 
                        TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble));
                }
            } 
            else
            {
                if (_Game == null)
                   Dbg.LogError($"Failed to open saved game, code: {_Status}");
                else
                   Dbg.LogError($"Failed to open saved game with file name {_Game.Filename}, code: {_Status}");
            }
        }
        
        private void SaveGame(ISavedGameMetadata _Game, byte[] _SavedData, TimeSpan _TotalPlaytime)
        {
            var data = CommonUtils.FromByteArray<object>(_SavedData);
            SaveGameProgressToCache(data);
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
            builder = builder
                .WithUpdatedPlayedTime(_TotalPlaytime)
                .WithUpdatedDescription("Saved game at " + DateTime.Now);
            SavedGameMetadataUpdate updatedMetadata = builder.Build();
            savedGameClient.CommitUpdate(_Game, updatedMetadata, _SavedData, OnSavedGameWritten);
        }

        private void OnSavedGameWritten(SavedGameRequestStatus _Status, ISavedGameMetadata _Game)
        {
            if (_Status == SavedGameRequestStatus.Success)
            {
                Dbg.Log($"Saved game with file name {_Game.Filename} was successfully written.");
            }
            else
            {
                Dbg.LogError(_Game == null
                    ? $"Failed to save game, code: {_Status}"
                    : $"Failed to save game with file name {_Game.Filename}, code: {_Status}");
            }
            if (m_OperationInfosQueue.Any())
                m_OperationInfosQueue.Peek().Finished = true;
        }
        
        private void LoadGameData(ISavedGameMetadata _Game) 
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.ReadBinaryData(_Game, OnSavedGameDataRead);
        }

        private void OnSavedGameDataRead(SavedGameRequestStatus _Status, byte[] _Data)
        {
            var operation = m_OperationInfosQueue.Peek();
            var info = operation.SavedGame;
            if (_Status == SavedGameRequestStatus.Success)
            {
                FileNameArgs fileNameData = null;
                try
                {
                    fileNameData = CommonUtils.FromByteArray<FileNameArgs>(_Data);
                }
                catch (SerializationException e)
                {
                    Dbg.Log(e.Message);
                }
                if (fileNameData?.FileName == null)
                {
                    Dbg.LogError($"Failed to read saved game, code: {_Status}");
                    return;
                }
                info.Data = _Data;
                if (info.Entity == null)
                {
                    info.Entity = new Entity<object>
                    {
                        Value = CommonUtils.FromByteArray<object>(_Data),
                        Result = EEntityResult.Success
                    };
                }
                else
                {
                    info.Entity.Value = CommonUtils.FromByteArray<object>(_Data);
                    info.Entity.Result = EEntityResult.Success;
                }
                Dbg.Log($"Saved game with file name {fileNameData.FileName} was successfully written.");
            } 
            else 
            {
                info.Entity = new Entity<object>
                {
                    Value = CommonUtils.FromByteArray<object>(_Data),
                    Result = EEntityResult.Fail
                };
                Dbg.LogError($"Failed to read saved game, code: {_Status}");
            }
            operation.Finished = true;
        }
        
        private static void DeleteGameData(string _FileName) 
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(_FileName, DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, DeleteSavedGame);
        }

        private static void DeleteSavedGame(SavedGameRequestStatus _Status, ISavedGameMetadata _Game)
        {
            if (_Status == SavedGameRequestStatus.Success)
            {
                ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
                savedGameClient.Delete(_Game);
                Dbg.Log($"Saved game with file name {_Game.Filename} was deleted successfully.");
            } 
            else
            {
                Dbg.LogError(_Game == null
                    ? "Failed to delete saved game"
                    : $"Failed to delete saved game with file name {_Game.Filename}");
            }
        }
        
        #endregion

        #region destructor

        ~AndroidScoreManager()
        {
            SaveGameOperationInfo lastWriteOperation = null;
            while (m_OperationInfosQueue.Any())
            {
                var op = m_OperationInfosQueue.Peek();
                if (!op.ReadOrWrite)
                    lastWriteOperation = op;
            }
            if (lastWriteOperation != null)
            {
                SaveGame(
                    lastWriteOperation.SavedGame.MetaData, 
                    lastWriteOperation.SavedGame.Data, 
                    TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble));
            }
        }

        #endregion
    }
}