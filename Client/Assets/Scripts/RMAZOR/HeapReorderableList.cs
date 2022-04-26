#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using RMAZOR.Helpers;
using RMAZOR.Models.MazeInfos;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR
{
    [Serializable]
    public class HeapReorderableList
    {
        #region constants

        private const int LevelsOnPage = 60;

        #endregion
        
        #region nonpublic members

        private static readonly float LineHeight                   = EditorGUIUtility.singleLineHeight;
        private static readonly Color ContentColor                 = Color.white;
        private static readonly Color BackgroundSelectedLevelColor = new Color(0.16f, 0.27f, 0.58f);
        private static readonly Color BackgroundLoadedLevelColor   = new Color(0f, 1f, 0.19f, 0.43f);

        private ReorderableList List { get; set; }

        #endregion
        
        #region serialized fields

        [SerializeField] private SerializableDictionary<EMazeItemType, bool> filters;
        
        public                   int            gameId;
        [SerializeField] public  int            heapIndex;
        [SerializeField] private int            selectedIndexCheck   = -1;
        [SerializeField] private int            loadedLevelIndex     = -1;
        [SerializeField] private int            loadedLevelHeapIndex = -1;
        [SerializeField] private bool           fastMode;
        [SerializeField] public  List<MazeInfo> levels;
        [SerializeField] private int            page = 1;

        #endregion

        #region api

        public        UnityAction<int> OnSelect;
        public static List<MazeInfo>   LevelsCached;
        
        public List<MazeInfo> Levels        => (levels ?? LevelsCached).ToList();
        public int            SelectedIndex => (page - 1) * LevelsOnPage + List.index;
        public int            Count         => (levels ?? LevelsCached).Count;

        public HeapReorderableList(
            int              _GameId,
            int              _HeapIndex,
            UnityAction<int> _OnSelect,
            List<MazeInfo>   _Levels)
        {
            gameId = _GameId;
            heapIndex = _HeapIndex;
            LevelsCached = levels = _Levels ?? GetLevelsSaver().LoadHeapLevels(_GameId, _HeapIndex).Levels;
            OnSelect = _OnSelect;
            InitFilters();
            ReloadList();
        }

        public void ReloadList()
        {
            if (filters.NullOrEmpty())
                InitFilters();
            var lvls = levels ?? LevelsCached;
            var levelsOnPage = Enumerable.Range(
                    (page - 1) * LevelsOnPage,
                    Math.Min(LevelsOnPage, lvls.Count - (page - 1) * LevelsOnPage))
                .Select(_I => lvls[_I])
                .ToList();
            List = new ReorderableList(
                levelsOnPage,
                typeof(MazeInfo),
                true,
                true,
                false,
                false)
            {
                headerHeight = LineHeight * 2f + LineHeight * filters.Count,
                drawElementBackgroundCallback = OnDrawElementBackgroundCallback,
                drawHeaderCallback = OnDrawHeaderCallback,
                onSelectCallback = _List =>
                {
                    selectedIndexCheck = _List.index;
                    OnSelect?.Invoke(_List.index);
                },
                onChangedCallback = _List => Save(),
                drawElementCallback = OnDrawElementCallback
            };
        }
        
        public bool NeedToReload(bool _Log = false)
        {
            bool needToReload = List == null || filters == null;
            if (_Log)
                Dbg.Log($"{List == null}; {filters == null}");
            if (needToReload) 
                return true;
            try
            {
                _ = List.count; //-V3125
            }
            catch
            {
                return true;
            }
            return false;
        }

        public void DoLayoutList()
        {
            List.DoLayoutList();
        }

        public void Insert(int _Index, MazeInfo _Info)
        {
            var lvls = levels ?? LevelsCached;
            lvls?.Insert(_Index, _Info);
            ReloadList();
        }

        public void Add(MazeInfo _Info)
        {
            var lvls = levels ?? LevelsCached;
            lvls?.Add(_Info);
            ReloadList();
        }

        public void Reload(int _HeapIndex, List<MazeInfo> _Levels = null)
        {
            if (heapIndex != _HeapIndex)
            {
                levels = LevelsCached = _Levels ?? GetLevelsSaver().LoadHeapLevels(gameId, _HeapIndex).Levels;
                page = 1;
                ReloadList();
            }
            else
            {
                if (_Levels != null)
                    levels = LevelsCached = _Levels;
                ReloadList();
            }
            heapIndex = _HeapIndex;
        }

        public void Delete(int _Index)
        {
            var lvls = levels ?? LevelsCached;
            if (lvls == null)
            {
                Dbg.LogError("Failed to delete level");
                return;
            }
            if (_Index < 0 || _Index >= lvls.Count)
                return;
            lvls.RemoveAt(_Index);
            ReloadList();
            Save();
        }

        public void Save()
        {
            var lvls = levels ?? LevelsCached;
            GetLevelsSaver().SaveLevelsToHeap(gameId, heapIndex, lvls);
        }

        public void Save(MazeInfo _Info, int _Index)
        {
            var lvls = levels ?? LevelsCached;
            _Info.AdditionalInfo.Comment1 = lvls[_Index]?.AdditionalInfo.Comment1;
            _Info.AdditionalInfo.Comment2 = lvls[_Index]?.AdditionalInfo.Comment2;
            lvls[_Index] = _Info;
            ReloadList();
            Save();
        }
        
        public void PreviousPage()
        {
            page = Math.Max(1, page - 1);
            ReloadList();
        }

        public void NextPage()
        {
            var lvls = levels ?? LevelsCached;
            page = Math.Min(page + 1, lvls.Count / LevelsOnPage + 1);
            ReloadList();
        }

        #endregion

        #region nonpublic methods

        private void InitFilters()
        {
            var fltrs = Enum.GetValues(typeof(EMazeItemType))
                .Cast<EMazeItemType>()
                .Except(new[] {EMazeItemType.Block})
                .ToList();
            filters = new SerializableDictionary<EMazeItemType, bool>();
            foreach (var filter in fltrs)
                filters.Add(filter, true);
        }
        
        private void OnDrawHeaderCallback(Rect _Rect)
        {
            float x, y, w;
            Rect UpdateRect() { return new Rect(x, y, w, LineHeight); }
            (x, y, w) = (_Rect.x, _Rect.y, _Rect.width * 0.5f);
            var lvls = levels ?? LevelsCached;
            EditorGUI.LabelField(UpdateRect(), $"Levels in heap: {lvls.Count}");
            x = _Rect.width * 0.5f;
            fastMode = EditorGUI.Toggle(UpdateRect(), "Fast mode", fastMode);
            foreach (var filter in filters
                .ToList()
                .Select(_Kvp => _Kvp.Key))
            {
                (x, y, w) = (_Rect.x, y + LineHeight, 20f);
                EditorGUI.DrawRect(UpdateRect(), FilterColors[filter]);
                (x, w) = (x + w, 20f);
                filters[filter] = EditorGUI.Toggle(UpdateRect(), filters[filter]);
                (x, w) = (x + w, 100f);
                EditorGUI.LabelField(UpdateRect(), filter.ToString());
                (x, w) = (x + w, 100f);
                int levelsWithThisFilterCount = Levels
                    .Count(_Level => _Level.MazeItems
                        .Any(_Item => _Item.Type == filter));
                EditorGUI.LabelField(UpdateRect(), $"Count: {levelsWithThisFilterCount}");
            }
        }
        
        private static void OnDrawElementBackgroundCallback(Rect _Rect, int _Index, bool _IsActive, bool _IsFocused)
        {
            GUI.contentColor = ContentColor;
        }

        public void SetupLoadedLevel(int _LevelIndex, int _HeapIndex)
        {
            loadedLevelIndex = _LevelIndex;
            loadedLevelHeapIndex = _HeapIndex;
        }

        private void OnDrawElementCallback(Rect _Rect, int _Index, bool _IsActive, bool _IsFocused)
        {
            var elementColor = _IsFocused || selectedIndexCheck == _Index
                ? BackgroundSelectedLevelColor
                : GetContentColor(_Index);
            if (loadedLevelIndex == _Index && loadedLevelHeapIndex == heapIndex)
                elementColor = BackgroundLoadedLevelColor;
            float x = _Rect.x;
            float y = _Rect.y;
            float w = 25f;
            Rect GetRect() { return new Rect(x, y, w, LineHeight); }
            EditorGUI.DrawRect(new Rect(
                _Rect.x,
                _Rect.y,
                _Rect.width,
                LineHeight), elementColor);
            EditorGUI.LabelField(GetRect(), $"{(page - 1) * LevelsOnPage + _Index + 1}");
            (x, w) = (x + w, 50f);
            var info = (MazeInfo)List.list[_Index];
            EditorGUI.LabelField(GetRect(), $"S: {info.Size.X}x{info.Size.Y}");
            if (fastMode)
                return;
            (x, w) = (x + w, _Rect.width * 0.5f - 2f * 40f);
            info.AdditionalInfo.Comment1 = EditorGUI.TextField(GetRect(), info.AdditionalInfo.Comment1);
            (x, w) = (x + w + 10f, _Rect.width * 0.5f - 2f * 40f - 10f);
            info.AdditionalInfo.Comment2 = EditorGUI.TextField(GetRect(), info.AdditionalInfo.Comment2);
            var selectedFilters = filters
                .Where(_Kvp => _Kvp.Value)
                .Select(_Kvp => _Kvp.Key)
                .ToList();
            int k = 0;
            foreach (var filter in filters
                .ToList()
                .Select(_Kvp => _Kvp.Key))
            {
                if (selectedFilters.Contains(filter)
                    && info.MazeItems.Any(_Item => _Item.Type == filter))
                {
                    const float w1 = 2f * 40f;
                    EditorGUI.DrawRect(new Rect(
                        _Rect.x + 40f + _Rect.width - 3f * 40f + w1 / filters.Count * k,
                        _Rect.y,
                        w1 / filters.Count,
                        LineHeight), FilterColors[filter]);
                }
                k++;
            }
        }

        private static Color GetContentColor(int _Index)
        {
            int groupIndex = RmazorUtils.GetGroupIndex(_Index);
            int idx = (groupIndex - 1) % RmazorUtils.LevelsInGroupList.Length;
            return GroupColors[idx];
        }

        private static readonly Color[] GroupColors =
        {
            new Color(0.2f, 0.2f, 0.2f),
            new Color(0.4f, 0.34f, 0.34f),
            new Color(0.32f, 0.34f, 0.4f),
        };
        
        private static readonly Dictionary<EMazeItemType, Color> FilterColors = new Dictionary<EMazeItemType, Color>
        {
            {EMazeItemType.GravityBlock,     new Color(1f, 0f, 0.01f)},
            {EMazeItemType.ShredingerBlock,  new Color(1f, 0.66f, 0f)},
            {EMazeItemType.Portal,           new Color(0.8f, 1f, 0f)},
            {EMazeItemType.TrapReact,        new Color(0.25f, 1f, 0f)},
            {EMazeItemType.TrapIncreasing,   new Color(0f, 0.53f, 1f)},
            {EMazeItemType.TrapMoving,       new Color(0.09f, 0f, 1f)},
            {EMazeItemType.GravityTrap,      new Color(0.7f, 0f, 1f)},
            {EMazeItemType.Turret,           new Color(1f, 0f, 0.64f)},
            {EMazeItemType.GravityBlockFree, new Color(0.94f, 1f, 0.96f)},
            {EMazeItemType.Springboard,      Color.black},
            {EMazeItemType.Hammer,           new Color(1f, 0.54f, 0.55f)},
            {EMazeItemType.Bazooka,          new Color(0.05f, 0.05f, 0.37f)},
        };
        
        private static LevelsSaver GetLevelsSaver()
        {
            var assetBundleManager = new AssetBundleManagerFake();
            var prefabSetManager = new PrefabSetManager(assetBundleManager);
            var mazeInfoValidator = new MazeInfoValidator();
            return new LevelsSaver(prefabSetManager, mazeInfoValidator);
        }

        #endregion
    }
}
#endif