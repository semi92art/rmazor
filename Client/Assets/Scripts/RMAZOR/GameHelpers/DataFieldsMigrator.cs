using Common;
using Common.Entities;
using Common.Network;
using Common.Utils;

namespace RMAZOR.GameHelpers
{
    public static class DataFieldsMigrator
    {
        public static void InitDefaultDataFieldValues(IGameClient _GameClient)
        {
            Dbg.Log(nameof(InitDefaultDataFieldValues));
            var savedGame = new MoneyArgs
            {
                FileName = CommonData.SavedGameFileName,
                Money = 100
            };
            const int accId = GameClientUtils.DefaultAccountId;
            var df = new GameDataField(
                _GameClient,
                savedGame,
                accId,
                1,
                (ushort) CommonUtils.StringToHash(CommonData.SavedGameFileName));
            df.Save(true);
        }
    }
}