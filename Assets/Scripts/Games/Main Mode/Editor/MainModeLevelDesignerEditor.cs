using System.Collections.Generic;
using System.Linq;
using Extensions;
using GameHelpers;
using Games.Editor;
using Games.Main_Mode.LevelStages;
using Games.Main_Mode.StageBlocks;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Games.Main_Mode.Editor
{
    [CustomEditor(typeof(MainModeLevelDesigner))]
    public class MainModeLevelDesignerEditor : LevelDesignerEditorBase
    {
        protected override GameMode GMode => GameMode.MainMode;
        private MainModeLevelDesigner m_Designer;
        

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Designer = Target as MainModeLevelDesigner;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var level = Target.LevelObject.GetComponent<Level>();
            
        }

        protected override void NewLevel()
        {
            if (!CreateLevelDialog)
                return;
            var go = new GameObject("Level");
            go.AddComponent<Level>();
            Target.LevelObject = go;
        }

        protected override void CreateStage()
        {
            
        }

        protected override void SelectStage(int _Idx)
        {
            var level = Target.LevelObject.GetComponent<Level>();
            if (_Idx >= level.stages.Count)
            {
                Debug.LogError("Index is more than stages count");
                return;
            }

            m_Designer.levelStage = level.stages[_Idx];
        }

        protected override void DeleteStage()
        {
            throw new System.NotImplementedException();
        }
    }
}