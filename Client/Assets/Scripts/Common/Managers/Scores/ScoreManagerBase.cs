using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.Network;
using Common.Network.DataFieldFilters;
using Common.Ticker;
using Common.Utils;
using Newtonsoft.Json;
using UnityEngine.Events;

namespace Managers.Scores
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
    
    public class LeaderBoardIdKeyPair
    {
        public ushort Id  { get; }
        public string Key { get; }

        public LeaderBoardIdKeyPair(ushort _Id, string _Key)
        {
            Id = _Id;
            Key = _Key;
        }
    }

    public interface IScoreManager : IInit
    {
        event ScoresEventHandler OnScoresChanged;
        void                     RegisterLeaderboards(List<LeaderBoardIdKeyPair> _Args);
        ScoresEntity             GetScoreFromLeaderboard(ushort _Id, bool _FromCache);
        bool                     SetScoreToLeaderboard(ushort _Id, long _Value, bool _OnlyToCache);
        bool                     ShowLeaderboard(ushort _Id);
        Entity<object>           GetSavedGameProgress(string _FileName, bool _FromCache);
        void                     SaveGameProgress<T>(T _Data, bool _OnlyToCache) where T : FileNameArgs;
        void DeleteSavedGame(string _FileName);
    }
    
    public abstract class ScoreManagerBase : InitBase, IScoreManager
    {
        #region nonpublic members

        private            IReadOnlyList<LeaderBoardIdKeyPair> m_ScoreArgsList    = new List<LeaderBoardIdKeyPair>();

        #endregion
        
        #region inject

        private IGameClient          GameClient          { get; }
        private ILocalizationManager LocalizationManager { get; }
        private ICommonTicker        Ticker              { get; }

        protected ScoreManagerBase(
            IGameClient          _GameClient,
            ILocalizationManager _LocalizationManager,
            ICommonTicker        _Ticker)
        {
            GameClient = _GameClient;
            LocalizationManager = _LocalizationManager;
            Ticker = _Ticker;
        }
        
        #endregion
        
        #region api

        public event ScoresEventHandler OnScoresChanged;
        
        public override void Init()
        {
            Ticker.Register(this);
            AuthenticatePlatformGameService(() => base.Init());
        }

        public void RegisterLeaderboards(List<LeaderBoardIdKeyPair> _Args)
        {
            m_ScoreArgsList = _Args;
        }

        public virtual ScoresEntity GetScoreFromLeaderboard(ushort _Id, bool _FromCache)
        {
            if (_FromCache)
                return GetScoreCached(_Id);
            var scoreEntity = new ScoresEntity{Result = EEntityResult.Pending};
            if (IsAuthenticatedInPlatformGameService())
                return null;
            Dbg.LogWarning($"{nameof(GetScoreFromLeaderboard)}: User is not authenticated");
            scoreEntity = GetScoreCached(_Id, scoreEntity);
            return scoreEntity;
        }

        public virtual bool SetScoreToLeaderboard(ushort _Id, long _Value, bool _OnlyToCache)
        {
            SetScoreCache(_Id, _Value);
            return true;
        }

        public virtual bool ShowLeaderboard(ushort _Id)
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


        public abstract void           SaveGameProgress<T>(T       _Data, bool _OnlyToCache) where T : FileNameArgs;
        public abstract void           DeleteSavedGame(string      _FileName);
        public abstract Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache);

        #endregion

        #region nonpublic methods

        protected abstract bool IsAuthenticatedInPlatformGameService();
        protected abstract void AuthenticatePlatformGameService(UnityAction _OnFinish);
        
        protected ScoresEntity GetScoreCached(ushort _Id, ScoresEntity _Entity = null)
        {
            var entity = _Entity ?? new ScoresEntity{Result = EEntityResult.Pending};
            var gdff = new GameDataFieldFilter(GameClient, GameClientUtils.AccountId, GameClientUtils.GameId,
                _Id) {OnlyLocal = true};
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

        private void SetScoreCache(ushort _Id, long _Value)
        {
            var gdff = new GameDataFieldFilter(
                GameClient, GameClientUtils.AccountId, 
                GameClientUtils.GameId,
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
                    OnScoresChanged?.Invoke(args);
                });
            });
        }
        
        protected string GetScoreKey(ushort _Id)
        {
            var args = m_ScoreArgsList.FirstOrDefault(_Args => _Args.Id == _Id);
            if (args != null) 
                return m_ScoreArgsList.FirstOrDefault(_Args => _Args.Id == _Id)?.Key;
            Dbg.LogError($"Score with id {_Id} does not exist.");
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
                fileNameData = JsonConvert.DeserializeObject<FileNameArgs>(ser);
            }
            catch (InvalidCastException)
            {
                Dbg.Log(nameof(SaveGameProgressToCache) + ": " + JsonConvert.SerializeObject(_Data));
                throw;
            }
            
            var gdff = new GameDataFieldFilter(GameClient, GameClientUtils.AccountId, GameClientUtils.GameId,
                (ushort)CommonUtils.StringToHash(fileNameData.FileName)) {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var field = _Fields.FirstOrDefault();
                if (field == null)
                {
                    Dbg.LogError($"Failed to save game with file name {fileNameData.FileName} to cache.");
                }
                else
                {
                    field.SetValue(_Data).Save(true);
                }
            });
        }

        protected Entity<object> GetSavedGameProgressFromCache(string _FileName)
        {
            var entity = new Entity<object>{Result = EEntityResult.Pending};
            var gdff = new GameDataFieldFilter(GameClient, GameClientUtils.AccountId, GameClientUtils.GameId,
                (ushort)CommonUtils.StringToHash(_FileName)) {OnlyLocal = true};
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