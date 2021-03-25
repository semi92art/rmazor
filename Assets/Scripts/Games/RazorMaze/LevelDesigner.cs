using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Exceptions;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
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
        [HideInInspector] public List<MazeProtItem> maze;

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
                 RazorMazeGameManager.Instance.SetMazeInfo(MazeInfo);
                 RazorMazeGameManager.Instance.Init();
            };
            SceneManager.LoadScene(SceneNames.Level);
        }
        
        public MazeInfo GetLevelInfoFromScene()
        {
            var protItemStart = maze.FirstOrDefault(_Item => _Item.props.IsStartNode);
            if (protItemStart == null)
            {
                Dbg.LogError("Maze must contain start item");
                return null;
            }
            var path = maze
                .Where(_Item => _Item.props.IsNode && !_Item.props.IsStartNode)
                .Select(_Item => _Item.props.Position)
                .ToList();
            path.Insert(0, protItemStart.props.Position);
            var mazeProtItems = maze
                .Where(_Item => !_Item.props.IsNode)
                .Select(_Item => _Item.props).ToList();
            foreach (var item in mazeProtItems.ToList())
            {
                switch (item.Type)
                {
                    case EMazeItemType.Block: 
                    case EMazeItemType.Turret:
                    case EMazeItemType.Portal:
                    case EMazeItemType.TrapIncreasing:
                    case EMazeItemType.TrapReact:
                    case EMazeItemType.BlockTransformingToNode:
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
                Width = sizes[sizeIdx],
                Height = sizes[sizeIdx],
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