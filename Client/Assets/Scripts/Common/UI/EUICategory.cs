using System;

namespace Common.UI
{
    [Flags]
    public enum EUiCategory
    {
        Fake = 1,
        Tutorial = 2,
        WheelOfFortune = 4,
        Shop = 8,
        Settings = 16,
        CharacterDied = 64,
        RateGame = 128
    }
}