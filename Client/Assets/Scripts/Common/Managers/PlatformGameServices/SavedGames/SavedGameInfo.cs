using Common.Entities;
using GooglePlayGames.BasicApi.SavedGame;
using mazing.common.Runtime.Entities;

namespace Common.Managers.PlatformGameServices.SavedGames
{
    public class SavedGameInfo
    {
        public byte[]             Data     { get; set; }
        public ISavedGameMetadata MetaData { get; set; }
        public Entity<object>     Entity   { get; set; }
    }
}