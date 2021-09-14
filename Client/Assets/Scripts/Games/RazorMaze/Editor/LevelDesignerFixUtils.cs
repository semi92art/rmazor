using System.Linq;
using Games.RazorMaze.Models;
using UnityEditor;

namespace Games.RazorMaze.Editor
{
    public static class LevelDesignerFixUtils
    {
        public static void FixPaths()
        {
            var levels = LevelDesignerEditor.ReorderableLevels.Levels.ToList();
            int k = 0;
            var last = levels.Last();
            foreach (var lev in levels)
            {
                float progress = (float)k / levels.Count;
                lev.Path = lev.Path.Distinct().ToList();
                foreach (var mazeItem in lev.MazeItems.Where(_Item => _Item.Path.Any()))
                    mazeItem.Path = mazeItem.Path.Distinct().ToList();
                EditorUtility.DisplayProgressBar("Fixing paths", $"Fixing paths: {k + 1}/{levels.Count}", progress);
                // MazeLevelUtils.SaveLevelToHeap(LevelDesignerEditor.GameId, lev, k++, LevelDesignerEditor.HeapIndex, lev == last);
            }
            EditorUtility.ClearProgressBar();
        }
    }
}