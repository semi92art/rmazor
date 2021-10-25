namespace Games.RazorMaze.Models
{
    public static class InputCommands
    {
        public const int MoveUp                         = 1;
        public const int MoveDown                       = 2;
        public const int MoveLeft                       = 3;
        public const int MoveRight                      = 4;
        public const int RotateClockwise                = 5;
        public const int RotateCounterClockwise         = 6;
        public const int LoadCurrentLevel               = 7;
        public const int LoadFirstLevelFromCurrentGroup = 8;
        public const int LoadNextLevel                  = 9;
        public const int UnloadLevel                    = 10;
        public const int ReadyToStartLevel              = 11;
        public const int StartOrContinueLevel           = 12;
        public const int PauseLevel                     = 13;
        public const int UnPauseLevel                   = 14;
        public const int FinishLevel                    = 15;
        public const int KillCharacter                  = 16;
        public const int LoadRandomLevel                = 17;
        public const int ShopMenu                       = 18;
        public const int SettingsMenu                   = 19;
        public const int LoadRandomLevelWithRotation    = 20;
        public const int ReadyToUnloadLevel             = 21;
    }
}