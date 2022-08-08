using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Entities;
using Common.Extensions;
using RMAZOR.Models.MazeInfos;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable once CheckNamespace

namespace RMAZOR.Editor
{
    public partial class LevelDesignerEditor
    {
        [FixUtil]
        private void FindEmptyLevels()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var items = l.PathItems;
                if (items.Any())
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid portals.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All levels are not empty.");
        }

        [FixUtil]
        private void FindLevelsWithPortalsInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var items = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.Portal)
                    .ToList();
                if (!items.Any())
                    continue;
                if (items.All(_I => _I.Pair != default))
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid portals.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All portals are valid.");
        }

        [FixUtil]
        private void FindLevelsWithTurretsInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var items = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.Turret)
                    .ToList();
                if (!items.Any())
                    continue;
                if (!items.Any(_I => !_I.Directions.Any() || _I.Directions.First() == default))
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid turrets.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All turrets are valid.");
        }
        
        [FixUtil]
        private void FindLevelsWithReactsTrapsInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var items = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.TrapReact)
                    .ToList();
                if (!items.Any())
                    continue;
                if (!items.Any(_I => !_I.Directions.Any() || _I.Directions.Any(_D => _D == default))) 
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid react traps.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All trap reacts are valid.");
        }
        
        [FixUtil]
        private void FindLevelsWithMovingTrapsInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var items = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.TrapMoving)
                    .ToList();
                if (!items.Any())
                    continue;
                bool errorsOnLevel = false;
                foreach (var item in items)
                {
                    var path = item.Path;
                    int count = path.Count;
                    if (path.Count < 2)
                        continue;
                    if (path.Last() == path[count - 2])
                        continue;
                    bool predicate = path.Last() == path[count - 2] 
                                     || (count == 3 
                                         && (path[0].X == path[1].X || path[0].Y == path[1].Y)
                                         && path[0] == path[2]);
                    if (!predicate)
                        continue;
                    errorsOnLevel = true;
                }
                if (!errorsOnLevel)
                    continue;
                Dbg.LogWarning($"Level {i + 1} possibly has invalid moving traps.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All moving traps are valid.");
        }
        
        [FixUtil]
        private void FindLevelsWithGravityBlocksInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var items = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.GravityBlock)
                    .ToList();
                if (!items.Any())
                    continue;
                if (!items.Any(_I => !_I.Path.Any()
                                      || _I.Path.Count != _I.Path.Distinct().Count()))
                {
                    continue;
                } 
                Dbg.LogError($"Level {i + 1} has invalid gravity blocks.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All gravity blocks are valid.");
        }
        
        [FixUtil]
        private void FindLevelsWithMazeSizeInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var items = l.MazeItems;
                if (!items.Any())
                    continue;
                if (items.Max(_I => _I.Position.X) + 1 == l.Size.X
                    || items.Max(_I => _I.Position.X) + 1 == l.Size.Y)
                {
                    continue;
                }
                Dbg.LogError($"Level {i + 1} has invalid maze size.");
                errors = true;
            }
            
            if (!errors)
                Dbg.Log("All levels have valid maze size.");
        }
        
        [FixUtil(FixUtilColor.Blue)]
        private void FixTrapsMoving()
        {
            var levels = LevelsList.Levels;
            bool errors = false;
            foreach (var l in levels)
            {
                var items = l.MazeItems
                    .Where(_Item => _Item.Type == EMazeItemType.TrapMoving)
                    .ToList();
                if (!items.Any())
                    continue;
                bool errorsOnLevel = false;
                foreach (var item in items)
                {
                    var path = item.Path;
                    int count = path.Count;
                    if (path.Count < 2)
                        continue;
                    if (path.Last() == path[count - 2])
                        continue;
                    bool predicate = path.Last() == path[count - 2] 
                                     || (count == 3 
                                         && (path[0].X == path[1].X || path[0].Y == path[1].Y)
                                         && path[0] == path[2]);
                    if (!predicate)
                        continue;
                    path.RemoveAt(count - 1);
                    item.Path = path;
                    errorsOnLevel = true;
                }
                if (!errorsOnLevel)
                    continue;
                Dbg.Log($"Moving traps on level {levels.IndexOf(l) + 1} fixed.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All traps moving are valid");
            LevelsList.Save();
        }

        [FixUtil(FixUtilColor.Blue)]
        private void SortLevels()
        {
            var levels = LevelsList.Levels;
            var levelsTutorial = levels.GetRange(0, 43);
            var levelsToSort = levels.GetRange(44, levels.Count - 44);
            levelsToSort.Shuffle();
            var sortedLevels = levelsTutorial.Concat(levelsToSort).ToList();
            LevelsList.Levels = LevelsList.Levels = sortedLevels;
            LevelsList.Save();
        }
        
        [FixUtil(FixUtilColor.Blue)]
        private void FixMazeSizes()
        {
            var levels = LevelsList.Levels;
            foreach (var level in levels)
            {
                var mazeItems = level.MazeItems.ToList();
                var pathItems = level.PathItems;
                int maxX = mazeItems.Any() ? mazeItems.Max(_Item => _Item.Position.X + 1) : 0;
                maxX = System.Math.Max(maxX, pathItems.Max(_Item => _Item.Position.X + 1));
                int maxY = mazeItems.Any() ? mazeItems.Max(_Item => _Item.Position.Y + 1) : 0;
                maxY = System.Math.Max(maxY, pathItems.Max(_Item => _Item.Position.Y + 1));
                level.Size = new V2Int(maxX, maxY);
            }
            LevelsList.Levels = levels;
            LevelsList.Save();
        }

        [FixUtil]
        private void FindDuplicatedMazes()
        {
            var duplicatedLevelIndicesList = new List<System.Tuple<int, int>>();
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var iLevel = levels[i];
                for (int j = i + 1; j < levels.Count; j++)
                {
                    var jLevel = levels[j];
                    if (iLevel != jLevel)
                        continue;
                    duplicatedLevelIndicesList.Add(new System.Tuple<int, int>(i, j));
                }
            }
            if (!duplicatedLevelIndicesList.Any())
            {
                Dbg.Log("Duplicated levels were not found.");
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("Found duplicated levels:");
            foreach (var duplIndices in duplicatedLevelIndicesList)
                sb.AppendLine($"{duplIndices.Item1} and {duplIndices.Item2}");
            Dbg.LogWarning(sb.ToString());
        }
    }
}