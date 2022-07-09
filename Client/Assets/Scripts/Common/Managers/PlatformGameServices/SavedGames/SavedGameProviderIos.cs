using Common.Entities;
using Common.Helpers;
using Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders;
using Common.Network;

namespace Common.Managers.PlatformGameServices.SavedGames
{
    public class SavedGameProviderIos : SavedGamesProviderBase
    {
        public SavedGameProviderIos(
            CommonGameSettings       _Settings,
            IGameClient              _GameClient,
            IRemoteSavedGameProvider _RemoteSavedGameProvider)
            : base(
                _Settings, 
                _GameClient, 
                _RemoteSavedGameProvider) { }

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
    }
}