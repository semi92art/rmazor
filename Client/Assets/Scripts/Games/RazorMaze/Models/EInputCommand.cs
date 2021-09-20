namespace Games.RazorMaze.Models
{
    public enum EInputCommand
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        RotateClockwise,
        RotateCounterClockwise,
        // Debug commands
        LoadLevel,
        ReadyToContinueLevel,
        ContinueLevel,
        PauseLevel,
        FinishLevel,
        UnloadLevel
    }
}