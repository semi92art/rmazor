using Common.Entities;
using Common.Helpers;

namespace Common.Managers.Scores
{
    public interface IRemoteSavedGameProvider: IInit
    {
        Entity<object> GetSavedGame(string    _FileName);
        void           SaveGame<T>(T          _Data) where T : FileNameArgs;
        void           DeleteSavedGame(string _FileName);
        void           FetchSavedGames();
    }

    public class FakeRemoteSavedGameProvider : InitBase, IRemoteSavedGameProvider
    {
        public Entity<object> GetSavedGame(string    _FileName)
        {
            return new Entity<object> {Result = EEntityResult.Fail};
        }

        public void SaveGame<T>(T          _Data) where T : FileNameArgs { }
        public void DeleteSavedGame(string _FileName)                    { }
        public void FetchSavedGames()                                    { }
    }
}