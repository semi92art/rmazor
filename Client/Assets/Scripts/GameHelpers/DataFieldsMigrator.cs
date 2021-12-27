using Constants;
using Entities;
using Network;
using Utils;

namespace GameHelpers
{
    public static class DataFieldsMigrator
    {
        public static void InitDefaultDataFieldValues(IGameClient _GameClient)
        {
            Dbg.Log(nameof(InitDefaultDataFieldValues));
            const int accId = GameClientUtils.DefaultAccountId;
            new GameDataField(_GameClient, 100, accId, 1, DataFieldIds.Money).Save(true);
        }

        public static void MigrateFromPrevious(IGameClient _GameClient)
        {
            var gameFieldIds = new []
            {
                DataFieldIds.Money
            };
            var gdff = new GameDataFieldFilter(_GameClient, GameClientUtils.PreviousAccountId, 1, gameFieldIds);
            gdff.OnlyLocal = true;
            gdff.Filter(_DefaultFields =>
            {
                foreach (var fld in _DefaultFields)
                {
                    if (fld == null)
                        continue;
                    var gdf = new GameDataField(_GameClient, fld.GetValue(), GameClientUtils.AccountId, 1, fld.FieldId);
                    gdf.Save();
                }
            });
        }

        public static void MigrateFromDatabase(IGameClient _GameClient)
        {
            var gameFieldIds = new []
            {
                DataFieldIds.Money
            };
            var gdff = new GameDataFieldFilter(_GameClient, GameClientUtils.AccountId, 1, gameFieldIds);
            gdff.Filter(_GameFieldsFromDB =>
            {
                foreach (var gdf in _GameFieldsFromDB)
                    gdf.Save(true);
            });
        }
    }
}