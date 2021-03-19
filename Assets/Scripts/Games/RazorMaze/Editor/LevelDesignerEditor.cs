using System.Linq;
using Extensions;
using Games.RazorMaze.Prot;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Editor;

namespace Games.RazorMaze.Editor
{
    [CustomEditor(typeof(LevelDesigner))]
    public class LevelDesignerEditor : UnityEditor.Editor
    {
        private LevelDesigner m_Des;

        private void OnEnable()
        {
            m_Des = (LevelDesigner) target;
        }

        public override void OnInspectorGUI()
        {
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Size");
                m_Des.sizeIdx = EditorGUILayout.Popup(m_Des.sizeIdx, 
                    m_Des.sizes.Select(_S => $"{_S}x{_S}").ToArray());
            });

            m_Des.aParam = EditorGUILayout.Slider(
                "Walls/nodes ratio", m_Des.aParam, 0, 1);

            base.OnInspectorGUI();
            
            EditorUtilsEx.GuiButtonAction("Create", CreateLevel);
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Create Default", CreateDefault);
                EditorUtilsEx.GuiButtonAction("Check for validity", CheckLevelOnSceneForValidity);
            });
            
            EditorUtilsEx.GUIColorZone(m_Des.valid ? Color.green : Color.red, 
                () => GUILayout.Label($"Level is {(m_Des.valid ? "" : "not")} valid"));
            EditorUtilsEx.DrawUiLine(Color.gray);
        }

        private void CreateLevel()
        {
            EditorUtilsEx.ClearConsole();
            ClearLevel();
            int size = m_Des.sizes[m_Des.sizeIdx];
            var parms = new MazeGenerationParams(
                size,
                size,
                m_Des.aParam,
                m_Des.obstacleLengths.ToArray());
            var info = LevelGenerator.CreateRandomLevelInfo(parms, out m_Des.valid);
            CreateObjectsAndFocusCamera(info);
            //m_Des.valid = LevelAnalizator.IsValid(info, false);
        }

        private void CreateDefault()
        {
            EditorUtilsEx.ClearConsole();
            ClearLevel();
            var info = LevelGenerator.CreateDefaultLevelInfo(m_Des.sizes[m_Des.sizeIdx], true);
            CreateObjectsAndFocusCamera(info);
            //m_Des.valid = LevelAnalizator.IsValid(info, false);
        }

        private void CreateObjectsAndFocusCamera(MazeInfo _Info)
        {
            var container = CommonUtils.FindOrCreateGameObject("Walls and Nodes", out _).transform;
            m_Des.mazeItems = RazorMazePrototypingUtils.CreateMazeItems(_Info, container);
            var converter = new CoordinateConverter();
            converter.Init(_Info.Width);
            var bounds = new Bounds(converter.GetCenter(), GameUtils.GetVisibleBounds().size * 0.7f);
            EditorUtilsEx.FocusSceneCamera(bounds);
        }

        private void ClearLevel()
        {
            var items = m_Des.mazeItems;
            if (items == null)
                return;
            foreach (var node in items.Where(_Item => _Item != null))
                node.gameObject.DestroySafe();
            items.Clear();
        }

        private void CheckLevelOnSceneForValidity()
        {
            var info = m_Des.GetLevelInfoFromScene();
            m_Des.valid = LevelAnalizator.IsValid(info, false);
        }
    }
}