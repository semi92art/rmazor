using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Games;
using Newtonsoft.Json;
using Utils;

namespace GameHelpers
{
    public static class DataFieldsMigrator
    {
        public static void InitDefaultDataFieldValues()
        {
            if (SaveUtils.GetValue<bool>(SaveKey.NotFirstLaunch))
                return;
            Dbg.Log(nameof(InitDefaultDataFieldValues));
            int accId = GameClientUtils.DefaultAccountId;
            new GameDataField(100, accId, 1, DataFieldIds.Money).Save(true);
            SaveUtils.PutValue(SaveKey.NotFirstLaunch, true);
        }

        public static void MigrateFromPrevious()
        {
            var gameFieldIds = new []
            {
                DataFieldIds.Money
            };
            var gdff = new GameDataFieldFilter(GameClientUtils.PreviousAccountId, 1, gameFieldIds);
            gdff.OnlyLocal = true;
            gdff.Filter(_DefaultFields =>
            {
                foreach (var fld in _DefaultFields)
                {
                    if (fld == null)
                        continue;
                    var gdf = new GameDataField(fld.GetValue(), GameClientUtils.AccountId, 1, fld.FieldId);
                    gdf.Save();
                }
            });
        }

        public static void MigrateFromDatabase()
        {
            var gameFieldIds = new []
            {
                DataFieldIds.Money
            };
            var gdff = new GameDataFieldFilter(GameClientUtils.AccountId, 1, gameFieldIds);
            gdff.Filter(_GameFieldsFromDB =>
            {
                foreach (var gdf in _GameFieldsFromDB)
                    gdf.Save(true);
            });
        }
    }
}