using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Exceptions;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
using Games.RazorMaze.Views.MazeItems;
using Malee.List;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Games.RazorMaze
{
    [InitializeOnLoad]
    public class LevelDesigner : MonoBehaviour
    {
        public static LevelDesigner Instance => FindObjectOfType<LevelDesigner>();
        
        [Serializable] public class LengthsList : ReorderableArray<int> { }
        
        private const int MinSize = 5;
        private const int MaxSize = 20;

        [Header("Path lengths"), Reorderable(paginate = true, pageSize = 5)] public LengthsList pathLengths;
        
        [HideInInspector] public List<int> sizes = Enumerable.Range(MinSize, MaxSize).ToList();
        [HideInInspector] public int sizeIdx;
        [HideInInspector] public float aParam;
        [HideInInspector] public bool valid;
        [HideInInspector] public List<ViewMazeItemProt> maze;

        private static MazeInfo MazeInfo
        {
            get => SaveUtils.GetValue<MazeInfo>(SaveKey.DesignerMazeInfo);
            set => SaveUtils.PutValue(SaveKey.DesignerMazeInfo, value);
        }
        
        static LevelDesigner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange _Change)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.Contains(SceneNames.Prototyping))
                return;
            
            if (_Change != PlayModeStateChange.EnteredPlayMode)
                return;
            
            MazeInfo = Instance.GetLevelInfoFromScene();
            GameClientUtils.GameId = 1;
            GameClientUtils.GameMode = (int)EGameMode.Prototyping;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
             
            SceneManager.sceneLoaded += (_Scene, _Mode) =>
            {
                RazorMazeGameManager.Instance.PreInit();
                RazorMazeGameManager.Instance.SetMazeInfo(MazeInfo);
                RazorMazeGameManager.Instance.Init();
            };
            SceneManager.LoadScene(SceneNames.Level);
        }
        
        public MazeInfo GetLevelInfoFromScene()
        {
            var protItemStart = maze.FirstOrDefault(_Item => _Item.Props.IsStartNode);
            if (protItemStart == null)
            {
                Dbg.LogError("Maze must contain start item");
                return null;
            }
            var path = maze
                .Where(_Item => _Item.Props.IsNode && !_Item.Props.IsStartNode)
                .Select(_Item => _Item.Props.Position)
                .ToList();
            path.Insert(0, protItemStart.Props.Position);
            var mazeProtItems = maze
                .Where(_Item => !_Item.Props.IsNode)
                .Select(_Item => _Item.Props).ToList();
            foreach (var item in mazeProtItems.ToList())
            {
                switch (item.Type)
                {
                    case EMazeItemType.TrapReact:
                    case EMazeItemType.Turret:
                    case EMazeItemType.BlockTransformingToNode:
                        mazeProtItems.Add(new ViewMazeItemProps
                        {
                            Position = item.Position,
                            Type = EMazeItemType.Block
                        });
                        break;
                    case EMazeItemType.TrapIncreasing:
                    case EMazeItemType.Block: 
                    case EMazeItemType.Portal:
                        // do nothing
                        break;
                    case EMazeItemType.TrapMoving:
                    case EMazeItemType.TurretRotating:
                    case EMazeItemType.BlockMovingGravity:
                    case EMazeItemType.TrapMovingGravity:
                        path.Add(item.Position);
                        break;
                    default: throw new SwitchCaseNotImplementedException(item.Type);
                }
            }
            return new MazeInfo{
                Size =  new V2Int(sizes[sizeIdx], sizes[sizeIdx]),
                Path = path,
                MazeItems = mazeProtItems
                    .SelectMany(_Item =>
                    {
                        var dirs = _Item.Directions.Clone();
                        dirs.Add(V2Int.zero);
                        return dirs.Select(_Dir =>
                                new MazeItem
                                {
                                    Direction = _Dir,
                                    Pair = _Item.Pair,
                                    Path = _Item.Path,
                                    Type = _Item.Type,
                                    Position = _Item.Position
                                });
                    }).ToList()
            };
        }
    }
}