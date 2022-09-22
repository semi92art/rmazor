using Common.Entities;
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
}