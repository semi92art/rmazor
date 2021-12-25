using Games.RazorMaze.Models;

namespace Entities
{
    public static class SaveKeysInEditor
    {
        public static SaveKey<int>      DesignerSelectedLevel => new SaveKey<int>("designer_selected_level");
        public static SaveKey<int>      DesignerHeapIndex     => new SaveKey<int>("designer_heap_index");
        public static SaveKey<string>   ServerUrl             => new SaveKey<string>("debug_server_url");
        public static SaveKey<MazeInfo> DesignerMazeInfo      => new SaveKey<MazeInfo>("designer_maze_info");
    }
}