using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using UnityEditor;

namespace Games.RazorMaze.Editor
{
    public partial class LevelDesignerEditor
    {
        // [FixUtil]
        // public static void FixPaths()
        // {
        //     var levels = LevelsList.Levels.ToList();
        //     int k = 0;
        //     var last = levels.Last();
        //     foreach (var lev in levels)
        //     {
        //         float progress = (float)k / levels.Count;
        //         lev.Path = lev.Path.Distinct().ToList();
        //         foreach (var mazeItem in lev.MazeItems.Where(_Item => _Item.Path.Any()))
        //             mazeItem.Path = mazeItem.Path.Distinct().ToList();
        //         EditorUtility.DisplayProgressBar("Fixing paths", $"Fixing paths: {k + 1}/{levels.Count}", progress);
        //         LevelsList.Save();
        //     }
        //     EditorUtility.ClearProgressBar();
        // }

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
    }
}