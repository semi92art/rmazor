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
        // load level commands
        LoadCurrentLevel,
        LoadFirstLevelFromCurrentGroup,
        LoadNextLevel,
        LoadLevelByIndex,
        LoadRandomLevel,
        LoadRandomLevelWithRotation,
        LoadFirstLevelFromRandomGroup,
        // level staging commands
        ReadyToStartLevel,
        StartOrContinueLevel,
        FinishLevel,
        ReadyToUnloadLevel,
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
        DisableDebug
    }
}