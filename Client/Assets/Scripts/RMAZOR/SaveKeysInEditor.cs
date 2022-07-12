using Common.Entities;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR
{
    public static class SaveKeysInEditor
    {
        public static SaveKey<int>      DesignerSelectedLevel => new SaveKey<int>("designer_selected_level");
        public static SaveKey<int>      DesignerHeapIndex     => new SaveKey<int>("designer_heap_index");
        public static SaveKey<int>      DesignerGameId        => new SaveKey<int>("designer_game_id");
        public static SaveKey<MazeInfo> DesignerMazeInfo      => new SaveKey<MazeInfo>("designer_maze_info");
        public static SaveKey<int>      StartHeapIndex        => new SaveKey<int>("start_heap_index");
    }
}