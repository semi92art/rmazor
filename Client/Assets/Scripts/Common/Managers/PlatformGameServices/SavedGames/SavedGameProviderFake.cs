using Common.Entities;
using Common.Helpers;
using Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders;
using Common.Network;

namespace Common.Managers.PlatformGameServices.SavedGames
{
    public class SavedGameProviderFake : SavedGamesProviderBase
    {
        #region inject

        public SavedGameProviderFake(
            CommonGameSettings       _Settings,
            IGameClient              _GameClient,
            IRemoteSavedGameProvider _RemoteSavedGameProvider)
            : base(
                _Settings,
                _GameClient,
                _RemoteSavedGameProvider) { }
        
        #endregion

        #region api

        public override void SaveGameProgress<T>(T _Data, bool _OnlyToCache)
        {
            SaveGameProgressToCache(_Data);
        }

        public override void DeleteSavedGame(string _FileName)
        {
            Dbg.Log("Available only on device");
        }

        public override Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache)
        {
            return GetSavedGameProgressFromCache(_FileName);
        }
        
        #endregion
    }
}