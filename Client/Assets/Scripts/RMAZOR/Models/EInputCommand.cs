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
        LoadFirstLevelFromCurrentGroup,
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
        // ui commands
        ShopPanel,
        SettingsPanel,
        RateGamePanel,
        FinishLevelGroupPanel,
        PlayBonusLevelPanel,
        // debug commands
        EnableDebug,
        DisableDebug,
        // debug load level commands
        LoadCurrentLevel,
        LoadRandomLevel,
        LoadRandomLevelWithRotation,
        LoadFirstLevelFromRandomGroup,
    }
}