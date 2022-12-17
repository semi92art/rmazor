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
    public class SavedGamesProvider : InitBase, ISavedGameProvider
    {
        #region inject

        private IGameClient              GameClient              { get; }
        private IRemoteSavedGameProvider RemoteSavedGameProvider { get; }

        protected SavedGamesProvider(
            IGameClient              _GameClient,
            IRemoteSavedGameProvider _RemoteSavedGameProvider)
        {
            GameClient              = _GameClient;
            RemoteSavedGameProvider = _RemoteSavedGameProvider;
        }

        #endregion

        #region api
        
        public event GameSavedAction GameSaved;

        public void FetchSavedGames()
        {
            RemoteSavedGameProvider.FetchSavedGames();
        }
        
        public virtual void DeleteSavedGame(string _FileName)
        {
            RemoteSavedGameProvider.DeleteSavedGame(_FileName);
        }

        public void SaveGameProgress<T>(T _Data, bool _OnlyToCache) where T : FileNameArgs
        {
            SaveGameProgressToCache(_Data);
            if (_OnlyToCache)
                return;
            RemoteSavedGameProvider.SaveGame(_Data);
        }

        public Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache)
        {
            return _FromCache
                ? GetSavedGameProgressFromCache(_FileName)
                : RemoteSavedGameProvider.GetSavedGame(_FileName);
        }

        #endregion

        #region nonpublic methods
        
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
            var gdff = new GameDataFieldFilter(
                GameClient, 
                GameClientUtils.AccountId, 
                CommonData.GameId,
                (ushort)CommonUtils.StringToHash(fileNameData!.FileName)) 
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
                    field.SetValue(_Data).Save(true);
                    GameSaved?.Invoke(new SavedGameEventArgs(_Data));
                }
            });
        }

        private Entity<object> GetSavedGameProgressFromCache(string _FileName)
        {
            var entity = new Entity<object>();
            var gdff = new GameDataFieldFilter(
                GameClient,
                GameClientUtils.AccountId, 
                CommonData.GameId,
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