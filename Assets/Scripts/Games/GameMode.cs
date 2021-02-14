using System.Collections.Generic;

namespace Games
{
    public enum GameMode
    {
        MainMode
    }

    public class GameModeNames
    {
        public static Dictionary<GameMode, string> Names = new Dictionary<GameMode, string>
        {
            {GameMode.MainMode, "main_mode"}
        };
    }
}