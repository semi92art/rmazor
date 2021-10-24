using System;

namespace UI
{
    [Flags]
    public enum EUiCategory
    {
        Nothing = 0,
        Loading = 1,
        MainMenu = 2,
        SelectGame = 4,
        DailyBonus = 8,
        WheelOfFortune = 16,
        Shop = 32,
        Settings = 64,
        LevelStart = 128,
        LevelFinish = 256,
    }
}