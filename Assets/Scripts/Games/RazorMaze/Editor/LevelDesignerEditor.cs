using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeItems;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Editor;

namespace Games.RazorMaze.Editor
{
    public class LevelDesignerEditor : EditorWindow
    {
        public static int GameId;
        private List<MazeInfo> levels;
        
        private LevelDesigner m_Des;
        private int m_LevelGroup = 1;
        private int m_LevelIndex;
        private Vector2 m_HeapScroll;

        [MenuItem("Tools/Level Designer", false)]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelDesignerEditor>("Level Designer");
            window.minSize = new Vector2(300, 200);
        }
        
        private void OnEnable()
        {
            GameId = 1;
            m_Des = LevelDesigner.Instance;
            levels = MazeLevelUtils.LoadHeapLevels(GameId).Levels;
        }

        public void OnGUI()
        {
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Release view", GUILayout.Width(80));
                int gameMode = GameClientUtils.GameMode;
                bool releaseView = gameMode == (int) EGameMode.Release;
                bool newReleaseView = EditorGUILayout.Toggle(releaseView, GUILayout.Width(35));
                if (releaseView != newReleaseView)
                    GameClientUtils.GameMode = newReleaseView ? (int) EGameMode.Release : (int) EGameMode.Prototyping;
                EditorUtilsEx.GUIEnabledZone(m_Des.size != default, () =>
                {
                    EditorUtilsEx.GuiButtonAction("Focus", FocusCamera, m_Des.size, GUILayout.Width(50));
                });
            });

            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Size:", GUILayout.Width(35));
                m_Des.sizeIdx = EditorGUILayout.Popup(m_Des.sizeIdx, 
                    LevelDesigner.Sizes.Select(_S => $"{LevelDesigner.MazeWidth}x{_S}").ToArray(),
                    GUILayout.Width(100));
                GUILayout.Label("Fullness:", GUILayout.Width(50));
                m_Des.aParam = EditorGUILayout.Slider(m_Des.aParam, 0, 1, GUILayout.Width(150));
                
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Path lengths:", GUILayout.Width(80));
                m_Des.pathLengths = EditorGUILayout.TextField(m_Des.pathLengths, GUILayout.Width(100));
                EditorUtilsEx.GuiButtonAction("Default", () => m_Des.pathLengths = "1,2,3", GUILayout.Width(60));
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Create", CreateLevel, GUILayout.Width(50));
                EditorUtilsEx.GuiButtonAction("Create Default", CreateDefault, GUILayout.Width(100));
                EditorUtilsEx.GuiButtonAction("Check for validity", CheckForValidity, GUILayout.Width(120));
                
                EditorUtilsEx.GUIColorZone(m_Des.valid ? Color.green : Color.red, 
                    () => GUILayout.Label($"Level is {(m_Des.valid ? "" : "not ")}valid"));
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Group:", GUILayout.Width(45));
                m_LevelGroup = EditorGUILayout.IntField( m_LevelGroup, GUILayout.Width(50));
                GUILayout.Label("Index:", GUILayout.Width(45));
                m_LevelIndex = EditorGUILayout.Popup(m_LevelIndex, 
                    Enumerable.Range(1, MazeLevelUtils.LevelsInGroup)
                        .Select(_Idx => _Idx.ToString()).ToArray(), GUILayout.Width(50));
                if (m_Des.group != 0 && m_Des.index >= 0)
                    GUILayout.Label($"Current: Group: {m_Des.group}, Index: {m_Des.index}");
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction(LoadLevel, m_LevelGroup, m_LevelIndex + 1, GUILayout.Width(100));
                EditorUtilsEx.GuiButtonAction(SaveLevel, m_LevelGroup, m_LevelIndex + 1, false, 1, GUILayout.Width(100));
            });

            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Add to heap", SaveLevel, m_LevelGroup, m_LevelIndex + 1, true, -1, GUILayout.Width(100));
                GUILayout.Label("Heap:");
            });

            ShowHeap();
        }

        private void CreateLevel()
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                EditorUtilsEx.ClearConsole();
                ClearLevel();
                var size = new V2Int(LevelDesigner.MazeWidth, LevelDesigner.Sizes[m_Des.sizeIdx]);
                var parms = new MazeGenerationParams(
                    size,
                    m_Des.aParam,
                    m_Des.pathLengths.Split(',').Select(_S => int.Parse(_S)).ToArray());
                var info = LevelGenerator.CreateRandomLevelInfo(parms, out m_Des.valid);
                LoadLevel(info);
            });
        }

        private void CreateDefault()
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                EditorUtilsEx.ClearConsole();
                ClearLevel();
                var size = new V2Int(LevelDesigner.MazeWidth, LevelDesigner.Sizes[m_Des.sizeIdx]);
                var info = LevelGenerator.CreateDefaultLevelInfo(size, true);
                LoadLevel(info);
            });
        }

        private void CreateObjects(MazeInfo _Info)
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                var container = CommonUtils.FindOrCreateGameObject("Maze", out _).transform;
                container.gameObject.DestroyChildrenSafe();
                var converter = new CoordinateConverter();
                converter.Init(_Info.Size);
                var contGetter = new ContainersGetter(converter);
                var mazeItemsCreator = new MazeItemsCreator(contGetter, converter, null);
                m_Des.maze = mazeItemsCreator.CreateMazeItems(_Info)
                    .Cast<ViewMazeItemProt>()
                    .ToList();
                m_Des.group = _Info.LevelGroup;
                m_Des.index = _Info.LevelIndex;
                m_Des.size = _Info.Size;
            });
        }

        private static void FocusCamera(V2Int _Size)
        {
            var converter = new CoordinateConverter();
            converter.Init(_Size);
            var bounds = new Bounds(converter.GetCenter(), GameUtils.GetVisibleBounds().size * 0.7f);
            EditorUtilsEx.FocusSceneCamera(bounds);
        }

        private void ClearLevel()
        {
            var items = m_Des.maze;
            if (items == null)
                return;
            foreach (var item in items.Where(_Item => _Item != null))
                item.gameObject.DestroySafe();
            items.Clear();
        }

        private void CheckForValidity()
        {
            var info = m_Des.GetLevelInfoFromScene();
            m_Des.valid = LevelAnalizator.IsValid(info, false);
        }

        private void LoadLevel(int _Group, int _Index)
        {
            var info = MazeLevelUtils.LoadLevel(GameId, _Group, _Index, false, false);
            CreateObjects(info);
            FocusCamera(info.Size);
        }

        private void SaveLevel(int _Group, int _Index, bool _ToHeap, int _HeapIndex)
        {
            var info = m_Des.GetLevelInfoFromScene(false);
            info.LevelGroup = _Group;
            info.LevelIndex = _Index;
            m_Des.group = _Group;
            m_Des.index = _Index;
            if (!_ToHeap)
                MazeLevelUtils.SaveLevel(GameId, info);
            else
            {
                MazeLevelUtils.SaveLevelToHeap(GameId, info, _HeapIndex == -1 ? null : (int?)_HeapIndex);
                levels = MazeLevelUtils.LoadHeapLevels(GameId).Levels;
            }
        }

        private void ShowHeap()
        {
            int k = 0;
            EditorUtilsEx.ScrollViewZone(ref m_HeapScroll, () =>
            {
                foreach (var level in levels)
                {
                    EditorUtilsEx.HorizontalZone(() =>
                    {
                        GUILayout.Label($"{++k}", GUILayout.Width(30));
                        EditorUtilsEx.GuiButtonAction("Load", LoadLevel, level, GUILayout.Width(50));
                        EditorUtilsEx.GuiButtonAction("Save to this", SaveLevel, 0, 0, true, k - 1, GUILayout.Width(100));
                        EditorUtilsEx.GuiButtonAction("Delete", DeleteLevelFromHeap, k - 1, GUILayout.Width(50));
                    });
                }
            }, false, false);
        }

        private void LoadLevel(MazeInfo _Info)
        {
            CreateObjects(_Info);
            FocusCamera(_Info.Size);
        }

        private void DeleteLevelFromHeap(int _Index)
        {
            MazeLevelUtils.DeleteLevelFromHeap(GameId, _Index);
            levels = MazeLevelUtils.LoadHeapLevels(GameId).Levels;
        }
    }
}