using System.Collections.Generic;
using System.Linq;
using GameHelpers;
using UnityEditor;
using Utils.Editor;

namespace Games.Editor
{
    public abstract class LevelDesignerEditorBase : UnityEditor.Editor
    {
        protected abstract GameMode GMode { get; }
        protected ILevelDesigner Target { get; private set; }
        
        private List<int> m_LevelIndexes;
        private int m_SelectedLevIndexToLoad;
        private int m_SelectedLevelToSave;
        
        protected virtual void OnEnable()
        {
            Target = (ILevelDesigner) target;
            m_LevelIndexes = LevelUtils.GetLevelIndexes(GMode);
        }

        public override void OnInspectorGUI()
        {
            var levelIndexeStrs = m_LevelIndexes.Select(_Idx => _Idx.ToString()).ToArray();

            if (m_LevelIndexes.Any())
            {
                EditorGUILayout.BeginHorizontal();
                m_SelectedLevIndexToLoad = EditorGUILayout.Popup(m_SelectedLevIndexToLoad, levelIndexeStrs);
                EditorUtilsEx.GuiButtonAction("Load Level", LoadLevel, m_LevelIndexes[m_SelectedLevIndexToLoad]);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorUtilsEx.GuiButtonAction("Save Level", SaveLevel, m_SelectedLevelToSave);
            m_SelectedLevelToSave = EditorGUILayout.IntField(m_SelectedLevelToSave);
            EditorGUILayout.EndHorizontal();
            
            EditorUtilsEx.GuiButtonAction("New Level", NewLevel);
            EditorUtilsEx.GuiButtonAction("Create Stage", CreateStage);
            EditorUtilsEx.GuiButtonAction("Select Stage", SelectStage, Target.StageIndex);
            EditorUtilsEx.GuiButtonAction("Delete Stage", DeleteStage);
        }

        protected virtual void LoadLevel(int _Idx)
        {
            var go = LevelUtils.LoadLevel(GMode, _Idx);
            Target.LevelObject = go;
        }

        protected virtual void SaveLevel(int _Idx)
        {
            LevelUtils.SaveLevel(GMode, _Idx, Target.LevelObject);
            m_LevelIndexes = LevelUtils.GetLevelIndexes(GMode);
        }

        protected abstract void NewLevel();
        protected abstract void CreateStage();
        protected abstract void SelectStage(int _Idx);
        protected abstract void DeleteStage();

        protected bool CreateLevelDialog => EditorUtility.DisplayDialog(
            "New Level",
            "Are you sure to create new level?\nCurrent changes will be lost.",
            "Ok",
            "Cancel");
    }
}