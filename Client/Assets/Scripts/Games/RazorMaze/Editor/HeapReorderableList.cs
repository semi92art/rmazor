using System;
using System.Collections.Generic;
using System.Linq;
using GameHelpers.Editor;
using Games.RazorMaze.Models;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace Games.RazorMaze.Editor
{
    public class HeapReorderableList
    {
        #region nonpublic members

        private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly Color ContentColor = Color.white;
        private static readonly Color BackgroundSelectedLevelColor = new Color(0.16f, 0.27f, 0.58f);
        private static readonly Color BackgroundLoadedLevelColor = new Color(0f, 1f, 0.19f, 0.43f);

        private readonly ReorderableList m_List;
        private readonly LevelsSaverEditor m_LevelsSaver = new LevelsSaverEditor();
        private readonly int m_GameId;
        private readonly Dictionary<EMazeItemType, bool> m_Filters = new Dictionary<EMazeItemType, bool>();

        private int m_HeapIndex;
        private int m_SelectedIndexCheck = -1;
        private int m_LoadedIndex;

        #endregion

        #region api

        public List<MazeInfo> Levels => m_List.list.Cast<MazeInfo>().ToList();
        public int SelectedIndex => m_List.index;
        public int Count => m_List.count;

        public HeapReorderableList(int _GameId, int _HeapIndex, UnityAction<int> _OnSelect)
        {
            m_GameId = _GameId;
            m_HeapIndex = _HeapIndex;
            var levels = m_LevelsSaver.LoadHeapLevels(_GameId, _HeapIndex).Levels;
            var filters = Enum.GetValues(typeof(EMazeItemType))
                .Cast<EMazeItemType>()
                .Except(new[] {EMazeItemType.Block, EMazeItemType.Attenuator})
                .ToList();
            foreach (var filter in filters)
                m_Filters.Add(filter, true);
            m_List = new ReorderableList(
                levels,
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
                    m_SelectedIndexCheck = _List.index;
                    _OnSelect?.Invoke(_List.index);
                },
                onChangedCallback = _List => Save(),
                drawElementCallback = OnDrawElementCallback
            };
        }

        public void DoLayoutList()
        {
            m_List.DoLayoutList();
        }

        public void Insert(int _Index, MazeInfo _Info)
        {
            m_List.list.Insert(_Index, _Info);
        }

        public void Add(MazeInfo _Info)
        {
            m_List.list.Add(_Info);
        }

        public void Reload(int _HeapIndex)
        {
            m_HeapIndex = _HeapIndex;
            m_List.list = m_LevelsSaver.LoadHeapLevels(m_GameId, _HeapIndex).Levels;
        }

        public void Delete(int _Index)
        {
            if (_Index < 0 || _Index >= m_List.list.Count)
                return;
            m_List.list.RemoveAt(_Index);
            Save();
        }

        public void Save()
        {
            m_LevelsSaver.SaveLevelsToHeap(m_GameId, m_HeapIndex, Levels);
        }

        public void Save(MazeInfo _Info, int _Index)
        {
            _Info.Comment = (m_List.list[_Index] as MazeInfo)?.Comment;
            m_List.list[_Index] = _Info;
            Save();
        }

        #endregion

        #region nonpublic methods

        private void OnDrawHeaderCallback(Rect _Rect)
        {
            var rect = new Rect(_Rect.x, _Rect.y, _Rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, $"Levels in heap: {m_List.list.Count}");
            rect = new Rect(_Rect.x, _Rect.y + LineHeight, _Rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, "Filters:");
            int k = 0;
            foreach (var filter in m_Filters
                .ToList()
                .Select(_Kvp => _Kvp.Key))
            {
                float yPos = _Rect.y + 2f * LineHeight + LineHeight * k;
                float xPos = _Rect.x;
                float width = 20f;
                UnityAction updateRect = () => rect = new Rect(xPos, yPos, width, EditorGUIUtility.singleLineHeight);
                updateRect();
                EditorGUI.DrawRect(rect, FilterColors[k++]);
                xPos += width;
                width = 20f;
                updateRect();
                m_Filters[filter] = EditorGUI.Toggle(rect, m_Filters[filter]);
                xPos += width;
                width = 100f;
                updateRect();
                EditorGUI.LabelField(rect, filter.ToString());
                xPos += width;
                width = 100f;
                updateRect();
                int levelsWithThisFilterCount = Levels
                    .Count(_Level => _Level.MazeItems
                        .Any(_Item => _Item.Type == filter));
                EditorGUI.LabelField(rect, $"Count: {levelsWithThisFilterCount}");
            }
        }

        private void OnDrawElementBackgroundCallback(Rect _Rect, int _Index, bool _IsActive, bool _IsFocused)
        {
            GUI.contentColor = ContentColor;
        }

        public void SetupLoadedLevel(int _Index)
        {
            m_LoadedIndex = _Index;
        }

        private void OnDrawElementCallback(Rect _Rect, int _Index, bool _IsActive, bool _IsFocused)
        {
            var elementColor = _IsFocused || m_SelectedIndexCheck == _Index
                ? BackgroundSelectedLevelColor
                : GetContentColor(_Index);
            if (m_LoadedIndex == _Index)
                elementColor = BackgroundLoadedLevelColor;
            EditorGUI.DrawRect(new Rect(
                _Rect.x,
                _Rect.y,
                _Rect.width,
                LineHeight), elementColor);
            var element = m_List.list[_Index] as MazeInfo;
            EditorGUI.LabelField(new Rect(_Rect.x, _Rect.y, 40, LineHeight), $"{_Index + 1}");
            if (element == null)
                return;
            element.Comment = EditorGUI.TextField(
                new Rect(
                    _Rect.x + 40,
                    _Rect.y,
                    _Rect.width - 3f * 40f,
                    LineHeight),
                element.Comment);
            var selectedFilters = m_Filters
                .Where(_Kvp => _Kvp.Value)
                .Select(_Kvp => _Kvp.Key)
                .ToList();
            int k = 0;
            foreach (var filter in m_Filters
                .ToList()
                .Select(_Kvp => _Kvp.Key))
            {
                if (selectedFilters.Contains(filter)
                    && element.MazeItems.Any(_Item => _Item.Type == filter))
                {
                    float w = 2f * 40f;
                    EditorGUI.DrawRect(new Rect(
                        _Rect.x + 40f + _Rect.width - 3f * 40f + w / m_Filters.Count * k,
                        _Rect.y,
                        w / m_Filters.Count,
                        LineHeight), FilterColors[k]);
                }
                k++;
            }
        }

        private static Color GetContentColor(int _Index)
        {
            int a = _Index % (RazorMazeUtils.LevelsInGroup * 2);
            return a < RazorMazeUtils.LevelsInGroup ?
                new Color(0.2f, 0.2f, 0.2f) : new Color(0.32f, 0.32f, 0.32f);
        }
        
        private static readonly Color[] FilterColors =
        {
            new Color(1f, 0f, 0.01f),
            new Color(1f, 0.66f, 0f),
            new Color(0.8f, 1f, 0f),
            new Color(0.25f, 1f, 0f),
            new Color(0f, 0.53f, 1f),
            new Color(0.09f, 0f, 1f),
            new Color(0.7f, 0f, 1f),
            new Color(1f, 0f, 0.64f),
            new Color(0.94f, 1f, 0.96f),
            Color.black
        };

        #endregion
    }
}