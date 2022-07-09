#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using RMAZOR.Controllers;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Zenject;
using ObjectExtensions = StansAssets.Foundation.Extensions.ObjectExtensions;

namespace RMAZOR
{
    public class LevelDesigner : MonoBehaviour
    {
        private static LevelDesigner _instance;
        public static LevelDesigner Instance => _instance.IsNotNull() ?
            _instance : _instance = FindObjectOfType<LevelDesigner>();

        [HideInInspector] public int                    width;
        [HideInInspector] public int                    height;
        [HideInInspector] public HeapReorderableList    levelsList;
        [HideInInspector] public string                 pathLengths;
        [HideInInspector] public float                  aParam;
        [HideInInspector] public bool                   valid;
        [SerializeField]  public List<ViewMazeItemProt> maze;
        [HideInInspector] public V2Int                  size;
        [HideInInspector] public int                    loadedLevelIndex     = -1;
        [HideInInspector] public int                    loadedLevelHeapIndex = -1;
        [HideInInspector] public GameObject             mazeObject;
        public static            UnityAction            SceneUnloaded;

        private static MazeInfo MazeInfo
        {
            get => SaveUtilsInEditor.GetValue(SaveKeysInEditor.DesignerMazeInfo);
            set => SaveUtilsInEditor.PutValue(SaveKeysInEditor.DesignerMazeInfo, value);
        }
        
        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            _instance = null;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static CommonGameSettings CommonGameSettings { get; set; }

        [Inject]
        private void Inject(CommonGameSettings _CommonGameSettings)
        {
            CommonGameSettings = _CommonGameSettings;
        }
        
        public MazeInfo GetLevelInfoFromScene()
        {
            mazeObject = GameObject.Find(ContainerNames.MazeItems);
            maze = new List<ViewMazeItemProt>();
            foreach (Transform mazeObj in mazeObject.transform)
            {
                var mazeItem = mazeObj.gameObject.GetComponent<ViewMazeItemProt>();
                if (mazeItem.IsNotNull()) 
                    maze.Add(mazeItem);
            }
            var protItemStart = maze.FirstOrDefault(_Item => _Item.Props.IsStartNode);
            if (protItemStart == null)
                Dbg.LogWarning("Maze must contain start item");
            var pathItems = maze
                .Where(_Item => _Item.Props.IsNode && !_Item.Props.IsStartNode)
                .Select(_Item => new PathItem{Blank = _Item.Props.Blank, Position = _Item.Props.Position})
                .ToList();
            if (protItemStart.IsNotNull())
            {
                pathItems.Insert(
                    0, 
                    new PathItem
                    {
                        Blank = protItemStart!.Props.Blank,
                        Position = protItemStart!.Props.Position
                    });
            }
            var mazeProtItems = maze
                .Where(_Item => !_Item.Props.IsNode)
                .Select(_Item => _Item.Props).ToList();
            int maxX = mazeProtItems.Any() ? mazeProtItems.Max(_Item => _Item.Position.X + 1) : 0;
            maxX = Math.Max(maxX, pathItems.Max(_Item => _Item.Position.X + 1));
            int maxY = mazeProtItems.Any() ? mazeProtItems.Max(_Item => _Item.Position.Y + 1) : 0;
            maxY = Math.Max(maxY, pathItems.Max(_Item => _Item.Position.Y + 1));
            var mazeSize = new V2Int(maxX, maxY);
            return new MazeInfo{
                Size =  mazeSize,
                PathItems = pathItems,
                MazeItems = mazeProtItems
                    .SelectMany(_Item =>
                    {
                        var dirs = _Item.Directions.Clone();
                        if (!dirs.Any())
                            dirs.Add(V2Int.Zero);
                        return dirs.Select(_Dir =>
                            new MazeItem
                            {
                                Directions = new List<V2Int> {_Dir},
                                Pair = _Item.Pair,
                                Path = _Item.Path,
                                Type = _Item.Type,
                                Position = _Item.Position,
                                Blank = _Item.Blank,
                                Args = _Item.Args
                            });
                    }).ToList()
            };
        }

        public V2Int GetSizeByIndex()
        {
            return new V2Int(width, height);
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange _Change)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.Contains(SceneNames.Prototyping))
                return;

            switch (_Change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    // SceneManager.sceneLoaded -= OnSceneLoaded;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    MazeInfo = Instance.GetLevelInfoFromScene();
                    CommonGameSettings.gameId = 1;
                    CommonData.Release = false;
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    Cor.Run(LoadSceneLevel());
                    EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Change);
            }
        }
        
        private static IEnumerator LoadSceneLevel()
        {
            var @params = new LoadSceneParameters(LoadSceneMode.Single);
            var op =  SceneManager.LoadSceneAsync(SceneNames.Level, @params);
            while (!op.isDone)
                yield return null;
        }

        private static void OnSceneUnloaded(PlayModeStateChange _State)
        {
#if UNITY_EDITOR
            SceneUnloaded?.Invoke();
            EditorApplication.playModeStateChanged -= OnSceneUnloaded;
#endif
        }

        private static void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
        {
            if (_Scene.name != SceneNames.Level)
                return;
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnSceneUnloaded;
            #endif
            Application.targetFrameRate = GraphicUtils.GetTargetFps();
            var controller = GameControllerMVC.CreateInstance();
            controller.Initialize += () =>
            {
                int selectedLevel = SaveUtilsInEditor.GetValue(SaveKeysInEditor.DesignerSelectedLevel);
                controller.Model.LevelStaging.LoadLevel(MazeInfo, selectedLevel);
                CommonData.LoadNextLevelAutomatically = false;
            };
            controller.Init();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}

#endif