using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Utils.Editor;

namespace Games.RazorMaze.Editor
{
    public class LevelDesignerEditor : EditorWindow
    {
        #region static members
        
        public static HeapReorderableList LevelsList;
        private static int _gameId = 1;

        private static int HeapIndex
        {
            get => SaveUtils.GetValue<int>(SaveKey.DesignerHeapIndex);
            set => SaveUtils.PutValue(SaveKey.DesignerHeapIndex, value);
        }
        private static int _heapIndexCheck;
        
        #endregion
        
        #region nonpublic members

        private LevelDesigner m_Des;
        private Vector2 m_HeapScroll;
        private int m_LevelIndex;
        private int m_LoadedLevelHeapIndex = -1;
        private int m_LoadedLevelIndex = -1;
        private int m_TabPage;
        private GUIStyle headerStyle;

        #endregion

        #region engine methods
        
        [MenuItem("Tools/Level Designer", false)]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelDesignerEditor>("Level Designer");
            window.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            m_Des = LevelDesigner.Instance;
        }

        private void OnBecameVisible()
        {
            headerStyle = new GUIStyle
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = GUI.contentColor}
            };
        }
        
        private void OnFocus()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            m_Des = LevelDesigner.Instance;
        }
        
        public void OnGUI()
        {
            m_TabPage = GUILayout.Toolbar (m_TabPage, new [] {"Levels", "Fix Utils"});
            switch (m_TabPage)
            {
                case 0: ShowLevelsTabPage(); break;
                case 1: ShowFixUtilsTabPage(); break;
            }
        }
        
        #endregion
        
        #region load/reload
        
        private void LoadLevel()
        {
            if (LevelsList.SelectedIndex == -1)
                return;
            var info = LevelsList.Levels[LevelsList.SelectedIndex];
            CreateObjects(info);
            FocusCamera(info.Size);
            m_LoadedLevelIndex = LevelsList.SelectedIndex;
            m_LoadedLevelHeapIndex = HeapIndex;
            LevelsList.SetupLoadedLevel(m_LoadedLevelIndex, m_LoadedLevelHeapIndex);
        }
        
        private void LoadLevel(MazeInfo _Info)
        {
            CreateObjects(_Info);
            FocusCamera(_Info.Size);
        }

        private void ReloadReorderableLevels(bool _Forced = false)
        {
            if (!_Forced && LevelsList != null)
                return;
            
            if (LevelsList == null)
                LevelsList = new HeapReorderableList(_gameId, HeapIndex, _SelectedIndex =>
                {
                    SaveUtils.PutValue(SaveKey.DesignerSelectedLevel, _SelectedIndex);
                });
            else LevelsList.Reload(HeapIndex);
            
            if (m_LoadedLevelIndex != -1)
                LevelsList.SetupLoadedLevel(m_LoadedLevelIndex, m_LoadedLevelHeapIndex);
        }
        
        #endregion
        
        #region save
        
        private void SaveLevel(int _HeapIndex, int _LevelIndex)
        {
            if (_LevelIndex == -1)
                return;
            var info = m_Des.GetLevelInfoFromScene();
            LevelsList.Save(info, _LevelIndex);
        }

        #endregion
        
        #region create
        
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
                    m_Des.pathLengths.Split(',').Select(int.Parse).ToArray());
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
        
        private void AddEmptyLevel()
        {
            var idx = LevelsList.SelectedIndex;
            var size = new V2Int(LevelDesigner.MazeWidth, LevelDesigner.Sizes[m_Des.sizeIdx]);
            var info = LevelGenerator.CreateDefaultLevelInfo(size, true);
            if (idx < LevelsList.Count - 1 && idx != -1)
                LevelsList.Insert(idx + 1, info);
            else 
                LevelsList.Add(info);
            LevelsList.Save();
        }
        
        #endregion
        
        #region draw
        
        private void ShowLevelsTabPage()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            ShowRandomMazeGeneratorZone();
            EditorUtilsEx.HorizontalLine(Color.gray);
            ShowHeapZone();
        }

        private void ShowFixUtilsTabPage()
        {
            EditorUtilsEx.GuiButtonAction("Fix Paths", LevelDesignerFixUtils.FixPaths);
        }

        private void ShowRandomMazeGeneratorZone()
        {
            GUILayout.Label("Random Maze Generator", headerStyle);
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
                m_Des.pathLengths = EditorGUILayout.TextField(m_Des.pathLengths);
                EditorUtilsEx.GuiButtonAction("Default", () => m_Des.pathLengths = "1,2,3", GUILayout.Width(60));
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Create", CreateLevel);
                EditorUtilsEx.GuiButtonAction("Create Default", CreateDefault);

            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Check validity", () =>
                {
                    var info = m_Des.GetLevelInfoFromScene();
                    m_Des.valid = LevelAnalizator.IsValid(info, false);
                }, GUILayout.Width(120));
                EditorUtilsEx.GUIColorZone(m_Des.valid ?
                        new Color(0.37f, 1f, 0.4f) : new Color(1f, 0.32f, 0.31f), 
                    () => GUILayout.Label($"Level is {(m_Des.valid ? "" : "not ")}valid"));
            });
        }
        
        private void ShowHeapZone()
        {
            GUILayout.Label("Heap", headerStyle);
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Heap:");
                HeapIndex = 1 + EditorGUILayout.Popup(HeapIndex - 1,
                                Enumerable.Range(1, 10)
                                    .Select(_Idx =>
                                    {
                                        string str = $"{_Idx}";
                                        if (_Idx == 1)
                                            return str + " (Release)";
                                        return str;
                                    }).ToArray());
                EditorUtilsEx.GuiButtonAction("Reload", () => ReloadReorderableLevels(true));
            });
            if (HeapIndex != _heapIndexCheck)
                ReloadReorderableLevels(true);
            _heapIndexCheck = HeapIndex;
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label($"Current level: heap {m_LoadedLevelHeapIndex}, index {m_LoadedLevelIndex + 1}", EditorStyles.boldLabel);
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Load", LoadLevel);
                EditorUtilsEx.GuiButtonAction("Save", SaveLevel, HeapIndex, LevelsList.SelectedIndex);
                EditorUtilsEx.GuiButtonAction("Delete", LevelsList.Delete, LevelsList.SelectedIndex);
                EditorUtilsEx.GuiButtonAction("Add Empty", AddEmptyLevel);
            });
            EditorUtilsEx.ScrollViewZone(ref m_HeapScroll, () => LevelsList.DoLayoutList());
        }
        
        #endregion
        
        #region other
        
        private void CreateObjects(MazeInfo _Info)
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                var container = CommonUtils.FindOrCreateGameObject("Maze", out _).transform;
                container.gameObject.DestroyChildrenSafe();
                var converter = new CoordinateConverter();
                converter.MazeSize = _Info.Size;
                var contGetter = new ContainersGetter(converter);
                var mazeItemsCreator = new MazeItemsCreatorInEditor(contGetter, converter);
                m_Des.maze = mazeItemsCreator.CreateMazeItems(_Info)
                    .Cast<ViewMazeItemProt>()
                    .ToList();
                m_Des.size = _Info.Size;
            });
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

        public static void FocusCamera(V2Int _Size)
        {
            var converter = new CoordinateConverter();
            converter.MazeSize = _Size;
            var bounds = new Bounds(converter.GetMazeCenter(), GameUtils.GetVisibleBounds().size * 0.7f);
            EditorUtilsEx.FocusSceneCamera(bounds);
        }
        
        #endregion
    }
}