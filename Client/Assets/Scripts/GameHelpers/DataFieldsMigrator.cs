using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Games;
using Network;
using Newtonsoft.Json;
using Utils;

namespace GameHelpers
{
    public static class DataFieldsMigrator
    {
        public static void InitDefaultDataFieldValues(IGameClient _GameClient)
        {
            if (SaveUtils.GetValue(SaveKeys.NotFirstLaunch))
                return;
            Dbg.Log(nameof(InitDefaultDataFieldValues));
            int accId = GameClientUtils.DefaultAccountId;
            new GameDataField(_GameClient, 100, accId, 1, DataFieldIds.Money).Save(true);
            SaveUtils.PutValue(SaveKeys.NotFirstLaunch, true);
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