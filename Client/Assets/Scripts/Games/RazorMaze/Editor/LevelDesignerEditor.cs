using System;
using System.Linq;
using System.Reflection;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Debug;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using Managers;
using ModestTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Utils.Editor;

namespace Games.RazorMaze.Editor
{
    public partial class LevelDesignerEditor : EditorWindow
    {
        #region static members
        
        private static          HeapReorderableList      _levelsList;
        private static          LevelDesigner            _des    => LevelDesigner.Instance;
        private static          int                      _gameId = 1;
        private static          IPrefabSetManager        _prefabSetManager;
        private static          IAssetBundleManager      _assetBundleManager;
        private static          ViewSettings             _viewSettings;
        private static          IMazeCoordinateConverter _coordinateConverter;
        private static          IContainersGetter        _containersGetter;
        private static          IMazeItemsCreator        _mazeItemsCreator;
        

        private static int HeapIndex
        {
            get => SaveUtilsInEditor.GetValue(SaveKeysInEditor.DesignerHeapIndex);
            set => SaveUtilsInEditor.PutValue(SaveKeysInEditor.DesignerHeapIndex, value);
        }
        private static int _heapIndexCheck;
        
        #endregion
        
        #region nonpublic members

        private Vector2  m_HeapScroll;
        private int      m_LevelIndex;
        private int      m_LoadedLevelHeapIndex = -1;
        private int      m_LoadedLevelIndex     = -1;
        private int      m_TabPage;
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
            InitEditor();
        }

        private void OnFocus()
        {
            InitEditor();
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

        public void OnGUI()
        {
            if (SceneManager.GetActiveScene().name == SceneNames.Prototyping)
            {
                m_TabPage = GUILayout.Toolbar (m_TabPage, new [] {"Levels", "Fix Utils"});
                switch (m_TabPage)
                {
                    case 0: ShowLevelsTabPage(); break;
                    case 1: ShowFixUtilsTabPage(); break;
                }
                return;
            }
            GUILayout.Label($"Level designer available only on scene {SceneNames.Prototyping}.unity");
            EditorUtilsEx.GuiButtonAction("Load scene", () =>
            {
                string sceneName = AssetDatabase
                    .FindAssets("l:Scene t:Scene", new[] {"Assets\\Scenes"})
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault(_SceneName => _SceneName.Contains(SceneNames.Prototyping));
                EditorHelper.LoadScene(sceneName);
            });
        }
        
        #endregion
        
        #region load/reload
        
        private void LoadLevel()
        {
            if (_levelsList.SelectedIndex == -1)
                return;
            var info = _levelsList.Levels[_levelsList.SelectedIndex];
            CreateObjects(info);
            FocusCamera(info.Size);
            m_LoadedLevelIndex = _levelsList.SelectedIndex;
            m_LoadedLevelHeapIndex = HeapIndex;
            _levelsList.SetupLoadedLevel(m_LoadedLevelIndex, m_LoadedLevelHeapIndex);
        }

        private void LoadLevel(MazeInfo _Info)
        {
            CreateObjects(_Info);
            FocusCamera(_Info.Size);
        }

        private void ReloadReorderableLevels(bool _Forced = false)
        {
            if (!_Forced && _levelsList != null)
                return;
            
            if (_levelsList == null)
                _levelsList = new HeapReorderableList(_gameId, HeapIndex, _SelectedIndex =>
                {
                    SaveUtilsInEditor.PutValue(SaveKeysInEditor.DesignerSelectedLevel, _SelectedIndex);
                });
            else _levelsList.Reload(HeapIndex);
            
            if (m_LoadedLevelIndex != -1)
                _levelsList.SetupLoadedLevel(m_LoadedLevelIndex, m_LoadedLevelHeapIndex);
        }
        
        #endregion
        
        #region save
        
        private void SaveLevel(int _HeapIndex, int _LevelIndex)
        {
            if (_LevelIndex == -1)
                return;
            var info = _des.GetLevelInfoFromScene();
            _levelsList.Save(info, _LevelIndex);
        }

        #endregion
        
        #region create
        
        private void CreateLevel()
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                EditorUtilsEx.ClearConsole();
                ClearLevel();
                var size = new V2Int(LevelDesigner.MazeWidth, LevelDesigner.Heights[_des.sizeIdx]);
                var parms = new MazeGenerationParams(
                    size,
                    _des.aParam,
                    _des.pathLengths.Split(',').Select(int.Parse).ToArray());
                var info = LevelGenerator.CreateRandomLevelInfo(parms, out _des.valid);
                LoadLevel(info);
            });
        }
        
        private void CreateDefault()
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                EditorUtilsEx.ClearConsole();
                ClearLevel();
                var size = new V2Int(LevelDesigner.MazeWidth, LevelDesigner.Heights[_des.sizeIdx]);
                var info = LevelGenerator.CreateDefaultLevelInfo(size, true);
                LoadLevel(info);
            });
        }
        
        private void AddEmptyLevel()
        {
            var idx = _levelsList.SelectedIndex;
            var size = new V2Int(LevelDesigner.MazeWidth, LevelDesigner.Heights[_des.sizeIdx]);
            var info = LevelGenerator.CreateDefaultLevelInfo(size, true);
            if (idx < _levelsList.Count - 1 && idx != -1)
                _levelsList.Insert(idx + 1, info);
            else 
                _levelsList.Add(info);
            _levelsList.Save();
        }
        
        #endregion
        
        #region draw
        
        private void ShowLevelsTabPage()
        {
            ShowMazeGeneratorZone();
            EditorUtilsEx.HorizontalLine(Color.gray);
            ShowHeapZone();
        }

        private void ShowFixUtilsTabPage()
        {
            var t = typeof(LevelDesignerEditor);
            var methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods.Where(_M => _M.HasAttribute(typeof(FixUtilAttribute))))
                EditorUtilsEx.GuiButtonAction(
                    method.Name.WithSpaces(), 
                    () => method.Invoke(null, null));
        }

        private void ShowMazeGeneratorZone()
        {
            if (_des.IsNull())
                return;
            
            GUILayout.Label("Maze Generator", headerStyle);
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Size:", GUILayout.Width(35));
                _des.sizeIdx = EditorGUILayout.Popup(_des.sizeIdx, 
                    LevelDesigner.Heights.Select(_S => $"{LevelDesigner.MazeWidth}x{_S}").ToArray(),
                    GUILayout.Width(100));
                GUILayout.Label("Fullness:", GUILayout.Width(50));
                _des.aParam = EditorGUILayout.Slider(_des.aParam, 0, 1, GUILayout.Width(150));
                
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Path lengths:", GUILayout.Width(80));
                _des.pathLengths = EditorGUILayout.TextField(_des.pathLengths);
                EditorUtilsEx.GuiButtonAction("Default", () => _des.pathLengths = "1,2,3", GUILayout.Width(60));
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
                    var info = _des.GetLevelInfoFromScene();
                    _des.valid = LevelAnalizator.IsValid(info, false);
                }, GUILayout.Width(120));
                EditorUtilsEx.GUIColorZone(_des.valid ?
                        new Color(0.37f, 1f, 0.4f) : new Color(1f, 0.32f, 0.31f), 
                    () => GUILayout.Label($"Level is {(_des.valid ? "" : "not ")}valid"));
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
            if (m_LoadedLevelIndex >= 0 && m_LoadedLevelHeapIndex >= 0)
            {
                EditorUtilsEx.HorizontalZone(() =>
                {
                    GUILayout.Label("Current level: " +
                                    $"heap {m_LoadedLevelHeapIndex}, " +
                                    $"index {m_LoadedLevelIndex + 1}", EditorStyles.boldLabel);
                });    
            }
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Load", LoadLevel);
                EditorUtilsEx.GuiButtonAction("Save", SaveLevel, HeapIndex, _levelsList.SelectedIndex);
                EditorUtilsEx.GuiButtonAction("Delete", _levelsList.Delete, _levelsList.SelectedIndex);
                EditorUtilsEx.GuiButtonAction("Add Empty", AddEmptyLevel);
            });
            EditorUtilsEx.ScrollViewZone(ref m_HeapScroll, () => _levelsList.DoLayoutList());
        }
        
        #endregion
        
        #region other

        private void InitEditor()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            _assetBundleManager = new AssetBundleManagerFake();
            _prefabSetManager = new PrefabSetManager(_assetBundleManager);
            _viewSettings = _prefabSetManager.GetObject<ViewSettings>(
                "model_settings", "view_settings");
            _coordinateConverter = new MazeCoordinateConverter(_viewSettings, null);
            _containersGetter = new ContainersGetter(null, _coordinateConverter);
            _coordinateConverter.GetContainer = _containersGetter.GetContainer;
            _coordinateConverter.Init();
            _mazeItemsCreator = new MazeItemsCreatorInEditor(_containersGetter, _coordinateConverter);
        }
        
        private void CreateObjects(MazeInfo _Info)
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                var container = CommonUtils.FindOrCreateGameObject(ContainerNames.MazeHolder, out _).transform;
                container.gameObject.DestroyChildrenSafe();
                _coordinateConverter.MazeSize = _Info.Size;
                _des.maze = _mazeItemsCreator.CreateMazeItems(_Info)
                    .Cast<ViewMazeItemProt>()
                    .ToList();
                _des.size = _Info.Size;
                if (ScreenViewDebug.Instance.IsNotNull())
                    ScreenViewDebug.Instance.MazeSize = _Info.Size;
            });
        }
        
        private static void ClearLevel()
        {
            var items = _des.maze;
            if (items == null)
                return;
            foreach (var item in items.Where(_Item => _Item != null))
                item.gameObject.DestroySafe();
            items.Clear();
        }

        public static void FocusCamera(V2Int _Size)
        {
            _coordinateConverter.MazeSize = _Size;
            var bounds = new Bounds(
                _coordinateConverter.GetMazeCenter(), 
                _coordinateConverter.GetMazeBounds().size);
            EditorUtilsEx.FocusSceneCamera(bounds);
        }
        
        #endregion
    }
}