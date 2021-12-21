using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Utils;

namespace Games.RazorMaze.Editor
{
    public partial class LevelDesignerEditor
    {
        [FixUtil]
        public static void FixTrapsReact()
        {
            var levels = LevelsList.Levels;
            foreach (var l in levels)
            {
                var trapReactMazeItems = l.MazeItems
                    .Where(_Item => _Item.Type == EMazeItemType.TrapReact)
                    .ToList();
                if (trapReactMazeItems.Any())
                {
                    var groups = trapReactMazeItems.GroupBy(_Item => new
                    {
                        _Item.Position,
                        _Item.Direction
                    });
                    foreach (var g in groups
                        .Where(_G => _G.Count() > 1))
                    {
                        var first = g.First();
                        foreach (var item in g.Except(new[] {first}))
                            l.MazeItems.Remove(item);
                    }
                }
                
                foreach (var item in l.MazeItems)
                {
                    item.Directions = new List<V2Int> { item.Direction };
                }
            }
            LevelsList.Save();
        }

        [FixUtil]
        public static void FindLevelsWithNonPairPortals()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var portals = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.Portal)
                    .ToList();
                if (!portals.Any())
                    continue;
                if (portals.All(_I => _I.Pair != default))
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid portals.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All portals are valid.");
        }

        [FixUtil]
        public static void FindLevelsWithTurretsInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var turrets = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.Turret)
                    .ToList();
                if (!turrets.Any())
                    continue;
                if (!turrets.Any(_I => !_I.Directions.Any() || _I.Directions.First() == default))
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid turrets.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All turrets are valid.");
        }
        
        [FixUtil]
        public static void FindLevelsWithReactsTrapsInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var turrets = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.TrapReact)
                    .ToList();
                if (!turrets.Any())
                    continue;
                if (!turrets.Any(_I => !_I.Directions.Any() || _I.Directions.Any(_D => _D == default))) 
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid react traps.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All trap reacts are valid.");
        }
        
        [FixUtil]
        public static void FindLevelsWithMovingTrapsInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var turrets = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.TrapMoving)
                    .ToList();
                if (!turrets.Any())
                    continue;
                if (!turrets.Any(_I => !_I.Path.Any() || _I.Path.Count != _I.Path.Distinct().Count())) 
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid moving traps.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All moving traps are valid.");
        }
        
        [FixUtil]
        public static void FindLevelsWithGravityBlocksInvalid()
        {
            bool errors = false;
            var levels = LevelsList.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                var l = levels[i];
                var turrets = l.MazeItems
                    .Where(_I => _I.Type == EMazeItemType.GravityBlock)
                    .ToList();
                if (!turrets.Any())
                    continue;
                if (!turrets.Any(_I => !_I.Path.Any() || _I.Path.Count != _I.Path.Distinct().Count())) 
                    continue;
                Dbg.LogError($"Level {i + 1} has invalid gravity blocks.");
                errors = true;
            }
            if (!errors)
                Dbg.Log("All gravity blocks are valid.");
        }
    }
}