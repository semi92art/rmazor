using Common.Entities;
using Common.Helpers;
using Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders;
using Common.Network;

namespace Common.Managers.PlatformGameServices.SavedGames
{
    public class SavedGameProviderGooglePlay : SavedGamesProviderBase
    {
        public SavedGameProviderGooglePlay(CommonGameSettings _Settings, IGameClient _GameClient, IRemoteSavedGameProvider _RemoteSavedGameProvider) : base(_Settings, _GameClient, _RemoteSavedGameProvider)
        {
        }

        public override void           SaveGameProgress<T>(T       _Data,     bool _OnlyToCache)
        {
            throw new System.NotImplementedException();
        }

        public override Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache)
        {
            throw new System.NotImplementedException();
        }
    }
}