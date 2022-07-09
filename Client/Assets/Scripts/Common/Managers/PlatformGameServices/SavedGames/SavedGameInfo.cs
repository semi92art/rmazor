using Common.Entities;
using GooglePlayGames.BasicApi.SavedGame;

namespace Common.Managers.PlatformGameServices.SavedGames
{
    public class SavedGameInfo
    {
        public byte[]             Data     { get; set; }
        public ISavedGameMetadata MetaData { get; set; }
        public Entity<object>     Entity   { get; set; }
    }
}