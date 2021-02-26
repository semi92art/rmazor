using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Utils;

namespace GameHelpers
{
    public static class DataFieldsMigrator
    {
        public static void InitDefaultDataFieldValues()
        {
            if (SaveUtils.GetValue<bool>(SaveKey.NotFirstLaunch))
                return;
            int accId = GameClientUtils.DefaultAccountId;
            var gdfFirstCurr = new GameDataField(10, accId, 1, DataFieldIds.FirstCurrency);
            var gdfSecondCurr = new GameDataField(10, accId, 1, DataFieldIds.SecondCurrency);
            var gdfInfLevScr = new GameDataField(0, accId, 1, DataFieldIds.InfiniteLevelScore);
            gdfFirstCurr.Save(true);
            gdfSecondCurr.Save(true);
            gdfInfLevScr.Save(true);
            SaveUtils.PutValue(SaveKey.NotFirstLaunch, true);
        }

        public static void MigrateFromDefault()
        {
            var levelOpendedIds = Enumerable
                .Range(1, 10000)
                .Select(DataFieldIds.LevelOpened).ToList();
            var gameFieldIds = new List<ushort>
            {
                DataFieldIds.FirstCurrency,
                DataFieldIds.SecondCurrency,
                DataFieldIds.InfiniteLevelScore
            }.Concat(levelOpendedIds);
            
            var gdff = new GameDataFieldFilter(GameClientUtils.DefaultAccountId, 1, gameFieldIds.ToArray());
            gdff.Filter(_DefaultFields =>
            {
                foreach (var fld in _DefaultFields)
                {
                    var gdf = new GameDataField(fld.GetValue(), GameClientUtils.AccountId, 1, fld.FieldId);
                    gdf.Save();
                }
            });
        }

        public static void MigrateFromDatabase()
        {
            var levelOpendedIds = Enumerable
                .Range(1, 10000)
                .Select(DataFieldIds.LevelOpened).ToList();
            var gameFieldIds = new List<ushort>
            {
                DataFieldIds.FirstCurrency,
                DataFieldIds.SecondCurrency,
                DataFieldIds.InfiniteLevelScore
            }.Concat(levelOpendedIds);
            var gdff = new GameDataFieldFilter(GameClientUtils.AccountId, 1, gameFieldIds.ToArray());
            gdff.Filter(_GameFieldsFromDB =>
            {
                foreach (var gdf in _GameFieldsFromDB)
                    gdf.Save(true);
            });
        }
    }
}