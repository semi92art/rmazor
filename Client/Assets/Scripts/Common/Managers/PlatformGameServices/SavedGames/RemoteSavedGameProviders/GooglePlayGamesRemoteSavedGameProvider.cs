#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Common.Entities;
using Common.Managers.PlatformGameServices.SavedGames;
using Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders;
using Common.Ticker;
using Common.Utils;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;

namespace Common.Managers.Scores
{
    public class GooglePlayGamesRemoteSavedGameProvider : RemoteSavedGameProviderBase, IUpdateTick
    {
        #region types
        
        private class SaveGameOperationInfo
        {
            /// <summary>
            /// <param>true</param>
            /// <c>true</c> when need to read and <c>false</c> when need to write
            /// </summary>
            public bool ReadOrWrite { get; set; }

            public SavedGameInfo SavedGame { get; set; }
            public string        FileName  { get; set; }
            public bool          Started   { get; set; }
            public bool          Finished  { get; set; }
        }

        #endregion
        
        #region nonpublic members

        private          bool                         m_OperationInfosQueueNotEmpty;
        private readonly Queue<SaveGameOperationInfo> m_OperationInfosQueue = new Queue<SaveGameOperationInfo>();
        private          SaveGameOperationInfo        m_CurrentOperationCached;

        #endregion

        #region inject
        
        private IViewGameTicker Ticker { get; }

        private GooglePlayGamesRemoteSavedGameProvider(IViewGameTicker _Ticker)
        {
            Ticker = _Ticker;
        }

        #endregion

        #region api

        public override void Init()
        {
            Ticker.Register(this);
            base.Init();
        }

        #endregion
        
        public override Entity<object> GetSavedGame(string _FileName)
        {
            var info = new SavedGameInfo
            {
                Entity = new Entity<object>()
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

        public override void SaveGame<T>(T _Data)
        {
            var info = new SavedGameInfo
            {
                Data = ToByteArray(_Data)
            };
            m_OperationInfosQueue.Enqueue(new SaveGameOperationInfo
            {
                SavedGame = info,
                FileName = _Data.FileName,
                ReadOrWrite = false
            });
            m_OperationInfosQueueNotEmpty = true;
        }

        public override void DeleteSavedGame(string _FileName)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(_FileName, DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, DeleteSavedGame);
        }

        public override void FetchSavedGames()
        {
            
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
                Cor.Run(Cor.Delay(10f, Ticker, OnDelay));
                OpenSavedGame(operation.FileName);
                return;
            }
            if (!operation.Finished)
                return;
            m_CurrentOperationCached = null;
            m_OperationInfosQueue.Dequeue();
            m_OperationInfosQueueNotEmpty = m_OperationInfosQueue.Any();
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
        
        private void SaveGame(ISavedGameMetadata _Game, byte[] _SavedData, TimeSpan _TotalPlaytime)
        {
            var data = FromByteArray<object>(_SavedData);
            // SaveGameProgressToCache(data);
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
                FileNameArgs fileNameData;
                try
                {
                    fileNameData = FromByteArray<FileNameArgs>(_Data);
                }
                catch (SerializationException e)
                {
                    Dbg.Log(e.Message);
                    info.Entity = new Entity<object>
                    {
                        Value = null,
                        Result = EEntityResult.Fail
                    };
                    return;
                }
                if (fileNameData?.FileName == null)
                {
                    Dbg.LogWarning("Failed to read saved game, fileNameData is null");
                    if (info.Entity == null)
                    {
                        info.Entity = new Entity<object>
                        {
                            Value = null,
                            Result = EEntityResult.Fail
                        };
                    }
                    else
                    {
                        info.Entity.Value = null;
                        info.Entity.Result = EEntityResult.Fail;
                    }
                    return;
                }
                info.Data = _Data;
                if (info.Entity == null)
                {
                    info.Entity = new Entity<object>
                    {
                        Value = FromByteArray<object>(_Data),
                        Result = EEntityResult.Success
                    };
                }
                else
                {
                    info.Entity.Value = FromByteArray<object>(_Data);
                    info.Entity.Result = EEntityResult.Success;
                }
                Dbg.Log($"Saved game with file name {fileNameData.FileName} was successfully written.");
            } 
            else 
            {
                info.Entity = new Entity<object>
                {
                    Value = FromByteArray<object>(_Data),
                    Result = EEntityResult.Fail
                };
                Dbg.LogError($"Failed to read saved game, code: {_Status}");
            }
            operation.Finished = true;
        }

        ~GooglePlayGamesRemoteSavedGameProvider()
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
    }
}
#endif