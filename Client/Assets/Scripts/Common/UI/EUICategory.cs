using System;

namespace Common.UI
{
    [Flags]
    public enum EUiCategory
    {
        Loading = 1,
        DailyBonus = 2,
        WheelOfFortune = 4,
        Shop = 8,
        Settings = 16,
        CharacterDied = 64,
        RateGame = 128
    }
}