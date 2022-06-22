using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Network;
using Common.Network.DataFieldFilters;
using Common.Ticker;
using Common.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;

namespace Common.Managers.Scores
{
    public class ScoresEventArgs
    {
        public ScoresEntity ScoresEntity { get; }

        public ScoresEventArgs(ScoresEntity _ScoresEntity)
        {
            ScoresEntity = _ScoresEntity;
        }
    }
    
    public class SavedGameEventArgs
    {
        public object SavedGame { get; }

        public SavedGameEventArgs(object _SavedGame)
        {
            SavedGame = _SavedGame;
        }
    }

    public delegate void ScoresEventHandler(ScoresEventArgs _Args);

    public interface IScoreManager: IInit
    {
        event ScoresEventHandler              ScoresChanged;
        event UnityAction<SavedGameEventArgs> GameSaved;
        void                                  RegisterLeaderboardsMap(Dictionary<ushort, string> _Map);
        ScoresEntity                          GetScoreFromLeaderboard(ushort        _Key, bool _FromCache);
        bool                                  SetScoreToLeaderboard(ushort          _Key, long _Value, bool _OnlyToCache);
        bool                                  ShowLeaderboard(ushort                _Key);
        Entity<object>                        GetSavedGameProgress(string           _FileName, bool _FromCache);
        void                                  SaveGameProgress<T>(T                 _Data,     bool _OnlyToCache) where T : FileNameArgs;
        void                                  DeleteSavedGame(string                _FileName);
        void                 RegisterAchievementsMap(Dictionary<ushort, string> _Map);
        Entity<IAchievement> UnlockAchievement(ushort                           _Key);
    }
    
    public abstract class ScoreManagerBase : InitBase, IScoreManager
    {
        #region nonpublic members

        protected IRemoteSavedGameProvider   RemoteSavedGameProvider { get; }
        private   Dictionary<ushort, string> m_LeaderboardsMap;
        private   Dictionary<ushort, string> m_AchievementsMap;
        private   IAchievement[]             m_Achievements;

        #endregion
        
        #region inject

        private CommonGameSettings   Settings            { get; }
        private IGameClient          GameClient          { get; }
        private ILocalizationManager LocalizationManager { get; }
        private ICommonTicker        Ticker              { get; }

        protected ScoreManagerBase(
            CommonGameSettings       _Settings,
            IGameClient              _GameClient,
            ILocalizationManager     _LocalizationManager,
            ICommonTicker            _Ticker,
            IRemoteSavedGameProvider _RemoteSavedGameProvider)
        {
            Settings                = _Settings;
            GameClient              = _GameClient;
            LocalizationManager     = _LocalizationManager;
            Ticker                  = _Ticker;
            RemoteSavedGameProvider = _RemoteSavedGameProvider;
        }
        
        #endregion
        
        #region api

        public event ScoresEventHandler              ScoresChanged;
        public event UnityAction<SavedGameEventArgs> GameSaved;

        public override void Init()
        {
            if (Initialized)
                return;
            Ticker.Register(this);
            AuthenticatePlatformGameService(() =>
            {
                // FIXME отрефакторить эту хуету
                Social.localUser.Authenticate(_Success =>
                {
                    if (_Success)
                    {
                        Social.LoadAchievements(_Achievements =>
                        {
                            foreach (var ach in m_Achievements)
                            {
                                Dbg.Log("Achievement loaded: " + ach.id);
                            }
                            m_Achievements = _Achievements;
                        });
                    }
                    else
                    {
                        Dbg.LogError("Failed to authenticate to Social");
                    }
                });
                base.Init();
            });
        }

        public void RegisterLeaderboardsMap(Dictionary<ushort, string> _Map)
        {
            m_LeaderboardsMap = _Map;
        }

        public virtual ScoresEntity GetScoreFromLeaderboard(ushort _Key, bool _FromCache)
        {
            if (_FromCache)
                return GetScoreCached(_Key);
            var scoreEntity = new ScoresEntity();
            if (IsAuthenticatedInPlatformGameService())
                return null;
            Dbg.LogWarning($"{nameof(GetScoreFromLeaderboard)}: User is not authenticated");
            scoreEntity = GetScoreCached(_Key, scoreEntity);
            return scoreEntity;
        }

        public virtual bool SetScoreToLeaderboard(ushort _Key, long _Value, bool _OnlyToCache)
        {
            SetScoreCached(_Key, _Value);
            return true;
        }

        public virtual bool ShowLeaderboard(ushort _Key)
        {
            string oopsText = LocalizationManager.GetTranslation("oops");
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                string noIntConnText = LocalizationManager.GetTranslation("no_internet_connection");
                Dbg.LogWarning($"{nameof(ShowLeaderboard)}: {noIntConnText}");
                CommonUtils.ShowAlertDialog(oopsText, noIntConnText);
                return false;
            }
            if (IsAuthenticatedInPlatformGameService()) 
                return true;
            string failedToLoadLeadText = LocalizationManager.GetTranslation("failed_to_load_lead");
            Dbg.LogWarning($"{nameof(ShowLeaderboard)}: {failedToLoadLeadText}");
            CommonUtils.ShowAlertDialog(oopsText, failedToLoadLeadText);
            return false;
        }
        
        public void RegisterAchievementsMap(Dictionary<ushort, string> _Map)
        {
            m_AchievementsMap = _Map;
        }

        public Entity<IAchievement> UnlockAchievement(ushort _Key)
        {
            var entity = new Entity<IAchievement>();
            Entity<IAchievement> Failed(string _Message)
            {
                Dbg.LogError(_Message);
                entity.Result = EEntityResult.Fail;
                return entity;
            }
            if (m_AchievementsMap == null)
                return Failed("Achievements map was not registered.");
            if (m_Achievements == null)
                return Failed("Achievements were not loaded from server.");
            string id = m_AchievementsMap.GetSafe(_Key, out bool containsKey);
            if (!containsKey)
                return Failed("Achievement with this key was not found in key-id map.");
            var achievement = m_Achievements.FirstOrDefault(_Ach => _Ach.id == id);
            if (achievement == null)
                return Failed("Achievement with this key was not found in loaded achievements.");
            achievement.percentCompleted = 100d;
            achievement.ReportProgress(_Success =>
            {
                if (!_Success)
                    Dbg.LogWarning($"Failed to unlock achievement with key {_Key}");
                entity.Result = _Success ? EEntityResult.Success : EEntityResult.Fail;
            });
            entity.Value = achievement;
            return entity;
        }


        public abstract void                 SaveGameProgress<T>(T                              _Data, bool _OnlyToCache) where T : FileNameArgs;
        public abstract void                 DeleteSavedGame(string                             _FileName);
        public abstract Entity<object>       GetSavedGameProgress(string                        _FileName, bool _FromCache);

        #endregion

        #region nonpublic methods

        protected abstract bool IsAuthenticatedInPlatformGameService();
        protected abstract void AuthenticatePlatformGameService(UnityAction _OnFinish);
        
        protected ScoresEntity GetScoreCached(ushort _Id, ScoresEntity _Entity = null)
        {
            var entity = _Entity ?? new ScoresEntity();
            var gdff = new GameDataFieldFilter(
                GameClient,
                GameClientUtils.AccountId, 
                Settings.gameId,
                _Id)
                {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var scoreField = _Fields.FirstOrDefault();
                if (scoreField == null)
                {
                    entity.Result = EEntityResult.Fail;
                }
                else
                {
                    entity.Result = EEntityResult.Success;
                    entity.Value.Add(_Id, scoreField.ToInt());
                }
            });
            return entity;
        }

        private void SetScoreCached(ushort _Id, long _Value)
        {
            var gdff = new GameDataFieldFilter(
                GameClient, GameClientUtils.AccountId, 
                Settings.gameId,
                _Id) {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var scoreField = _Fields.First();
                scoreField.SetValue(_Value).Save(true);
                Cor.RunSync(() =>
                {
                    var entity = new ScoresEntity
                    {
                        Result = EEntityResult.Success,
                        Value = new Dictionary<ushort, long> {{_Id, _Value}}
                    };
                    var args = new ScoresEventArgs(entity);
                    ScoresChanged?.Invoke(args);
                });
            });
        }
        
        protected string GetScoreId(ushort _Key)
        {
            string id = m_LeaderboardsMap.GetSafe(_Key, out bool containsKey);
            if (containsKey) 
                return id;
            Dbg.LogError($"Score with id {_Key} does not exist.");
            return null;
        }

        protected static string AuthMessage(bool _Success, string _AddMessage)
        {
            return $"{(_Success ? "Success" : "Fail")} on authentication to game service: {_AddMessage}";
        }

        protected void SaveGameProgressToCache(object _Data)
        {
            FileNameArgs fileNameData;
            try
            {
                string ser = JsonConvert.SerializeObject(_Data);
                Dbg.Log(ser);
                fileNameData = JsonConvert.DeserializeObject<FileNameArgs>(ser);
            }
            catch (InvalidCastException)
            {
                Dbg.Log(nameof(SaveGameProgressToCache) + ": " + JsonConvert.SerializeObject(_Data));
                throw;
            }
            var gdff = new GameDataFieldFilter(
                GameClient, 
                GameClientUtils.AccountId, 
                Settings.gameId,
                (ushort)CommonUtils.StringToHash(fileNameData.FileName)) 
                {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var field = _Fields.FirstOrDefault();
                if (field == null)
                {
                    Dbg.LogError($"Failed to save game with file name {fileNameData.FileName} to cache.");
                }
                else
                {
                    Dbg.Log($"Successfully save game with file name {fileNameData.FileName} to cache.");
                    field.SetValue(_Data).Save(true);
                    GameSaved?.Invoke(new SavedGameEventArgs(_Data));
                }
            });
        }

        protected Entity<object> GetSavedGameProgressFromCache(string _FileName)
        {
            var entity = new Entity<object>();
            var gdff = new GameDataFieldFilter(
                GameClient,
                GameClientUtils.AccountId, 
                Settings.gameId,
                (ushort)CommonUtils.StringToHash(_FileName))
                {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var field = _Fields.FirstOrDefault();
                if (field == null)
                {
                    Dbg.LogError($"Failed to load saved game with file name {_FileName} from cache.");
                    entity.Result = EEntityResult.Fail;
                }
                else
                {
                    entity.Value = field.GetValue();
                    entity.Result = EEntityResult.Success;
                }
            });
            return entity;
        }
        


        #endregion
    }
}