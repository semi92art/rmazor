using System;

namespace Common.UI
{
    [Flags]
    public enum EUiCategory
    {
        Fake           = 1 << 0,
        Tutorial       = 1 << 1,
        WheelOfFortune = 1 << 2,
        Shop           = 1 << 3,
        Settings       = 1 << 4,
        CharacterDied  = 1 << 5,
        RateGame       = 1 << 6,
        FinishGroup    = 1 << 7
    }
}