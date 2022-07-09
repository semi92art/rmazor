using System;
using System.Linq;
using Common.Entities;
using Common.Helpers;
using Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders;
using Common.Network;
using Common.Network.DataFieldFilters;
using Common.Utils;
using Newtonsoft.Json;
using UnityEngine.Events;

namespace Common.Managers.PlatformGameServices.SavedGames
{
    public interface ISavedGameProvider : IInit
    {
        event UnityAction<SavedGameEventArgs> GameSaved;

        Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache);
        void           SaveGameProgress<T>(T       _Data,     bool _OnlyToCache) where T : FileNameArgs;
        void           DeleteSavedGame(string      _FileName);
        void           FetchSavedGames();
    }
    
    public abstract class SavedGamesProviderBase : InitBase, ISavedGameProvider
    {
        #region inject

        private   CommonGameSettings       Settings                { get; }
        private   IGameClient              GameClient              { get; }
        protected IRemoteSavedGameProvider RemoteSavedGameProvider { get; }

        protected SavedGamesProviderBase(
            CommonGameSettings       _Settings,
            IGameClient              _GameClient,
            IRemoteSavedGameProvider _RemoteSavedGameProvider)
        {
            Settings                = _Settings;
            GameClient              = _GameClient;
            RemoteSavedGameProvider = _RemoteSavedGameProvider;
        }

        #endregion

        #region api
        
        public event UnityAction<SavedGameEventArgs> GameSaved;

        public void FetchSavedGames()
        {
            RemoteSavedGameProvider.FetchSavedGames();
        }
        
        public virtual void DeleteSavedGame(string _FileName)
        {
            RemoteSavedGameProvider.DeleteSavedGame(_FileName);
        }
        
        public abstract void           SaveGameProgress<T>(T  _Data, bool _OnlyToCache) where T : FileNameArgs;
        public abstract Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache);

        #endregion

        #region nonpublic methods
        
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