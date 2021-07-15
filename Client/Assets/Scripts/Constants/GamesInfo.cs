namespace Constants
{
    public class GameInfo
    {
        public static GameInfo[] Infos => new[]
        {
            new GameInfo(1, "Points Tapper", true),
            new GameInfo(2, "Lines Drawer", false),
            new GameInfo(3, "Math Train", false),
            new GameInfo(4, "Path Finder", true),
            new GameInfo(5, "Points Tapper", false)
        };
        
        public int GameId { get; }
        public string Title { get; }
        public bool Available { get; }

        public GameInfo(int _GameId, string _Title, bool _Available)
        {
            GameId = _GameId;
            Title = _Title;
            Available = _Available;
        }
    }
}