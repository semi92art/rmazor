namespace RMAZOR.Models
{
    public enum EInputCommand
    {
        // move commands
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        // rotate commands
        RotateClockwise,
        RotateCounterClockwise,
        // level staging commands
        LoadLevelByIndex,
        LoadNextLevel,
        ReadyToStartLevel,
        StartOrContinueLevel,
        FinishLevel,
        StartUnloadingLevel,
        UnloadLevel,
        PauseLevel,
        UnPauseLevel,
        KillCharacter,
        ExitLevelStaging,
        // ui commands
        ShopPanel,
        DisableAdsPanel,
        SettingsPanel,
        DailyGiftPanel,
        LevelsPanel,
        RateGamePanel,
        FinishLevelGroupPanel,
        PlayBonusLevelPanel,
        TutorialPanel,
        MainMenuPanel,
        RateGameFromGameUi,
        // debug commands
        EnableDebug,
        DisableDebug,
        // debug load level commands
        LoadCurrentLevel,
    }
}