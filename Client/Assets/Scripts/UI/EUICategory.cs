using System;

namespace UI
{
    [Flags]
    public enum EUiCategory
    {
        Loading = 1,
        DailyBonus = 8,
        WheelOfFortune = 16,
        Shop = 32,
        Settings = 64,
        CharacterDied = 128
    }
}