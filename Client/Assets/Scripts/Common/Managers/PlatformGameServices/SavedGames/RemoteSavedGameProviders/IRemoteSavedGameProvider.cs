using Common.Entities;

namespace Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders
{
    public interface IRemoteSavedGameProvider: IInit
    {
        Entity<object> GetSavedGame(string    _FileName);
        void           SaveGame<T>(T          _Data) where T : FileNameArgs;
        void           DeleteSavedGame(string _FileName);
        void           FetchSavedGames();
    }
}