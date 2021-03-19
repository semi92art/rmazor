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
        
        [Serializable] public class ObstacleLengthList : ReorderableArray<int> { }
        
        private const int MinSize = 5;
        private const int MaxSize = 20;

        [Header("Path lengths"), Reorderable(paginate = true, pageSize = 5)] public ObstacleLengthList obstacleLengths;
        
        [HideInInspector] public List<int> sizes = Enumerable.Range(MinSize, MaxSize).ToList();
        [HideInInspector] public int sizeIdx;
        [HideInInspector] public float aParam;
        [HideInInspector] public bool valid;
        [HideInInspector] public List<MazeProtItem> mazeItems;

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
            //Dbg.Log($"PlayModeStateChange: {_Change}");
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
            var nodes = mazeItems
                .Where(_Item => _Item.Type == MazeItemType.Node)
                .Select(_Item => _Item.start)
                .Select(_PosInt => new Node{Position = _PosInt})
                .ToList();
            var nodeStart = mazeItems.Where(_Item => _Item.Type == MazeItemType.NodeStart)
                .Select(_Item => new Node{Position = _Item.start}).First();
            nodes.Insert(0, nodeStart);
            var obstacles  = mazeItems
                .Where(_Item => _Item.Type == MazeItemType.Obstacle 
                                || _Item.Type == MazeItemType.ObstacleMoving 
                                || _Item.Type == MazeItemType.ObstacleTrap
                                || _Item.Type == MazeItemType.ObstacleTrapMoving)
                .Select(_Item =>
                {
                    var type = RazorMazePrototypingUtils.GetObstacleType(_Item.Type);
                    return new Obstacle {Position = _Item.start, Path = _Item.path, Type = type};
                }).ToList();
            foreach (var obs in obstacles)
            {
                switch (obs.Type)
                {
                    case EObstacleType.Obstacle: break; // do nothing
                    case EObstacleType.Trap:
                        obstacles.Add(new Obstacle{Position = obs.Position, Path = obs.Path, Type = EObstacleType.Obstacle});
                        break;
                    case EObstacleType.ObstacleMoving:
                    case EObstacleType.TrapMoving:
                        nodes.Add(new Node{Position = obs.Position});
                        break;
                    default: throw new SwitchCaseNotImplementedException(obs.Type);
                }
            }
            return new MazeInfo{
                Width     = sizes[sizeIdx],
                Height    = sizes[sizeIdx],
                Nodes     = nodes,
                Obstacles = obstacles
            };
        }
    }
}