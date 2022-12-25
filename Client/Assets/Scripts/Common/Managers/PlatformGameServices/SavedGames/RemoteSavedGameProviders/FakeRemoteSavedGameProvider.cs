using Common.Entities;
using Common.Helpers;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Helpers;

namespace Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders
{
    public class FakeRemoteSavedGameProvider : InitBase, IRemoteSavedGameProvider
    {
        public Entity<object> GetSavedGame(string _FileName)
        {
            return new Entity<object> {Result = EEntityResult.Fail};
        }

        public void SaveGame<T>(T          _Data) where T : FileNameArgs { }
        public void DeleteSavedGame(string _FileName)                    { }
        public void FetchSavedGames()                                    { }
    }
}