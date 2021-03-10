using System.Linq;
using Extensions;
using Games.RazorMaze.Nodes;
using Games.RazorMaze.Prot;
using Games.RazorMaze.WallBlocks;
using UnityEditor;
using UnityEngine;
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
            EditorUtilsEx.GuiButtonAction("Create Default", CreateDefault);
            EditorUtilsEx.GuiButtonAction("Check for validity", CheckLevelOnSceneForValidity);
            EditorUtilsEx.GUIColorZone(m_Des.valid ? Color.green : Color.red, 
                () => GUILayout.Label($"Level is {(m_Des.valid ? "" : "not")} valid"));
        }

        private void CreateLevel()
        {
            ClearLevel();
            int size = m_Des.sizes[m_Des.sizeIdx];
            var parms = new MazeGenerationParams(
                size,
                size,
                m_Des.aParam,
                m_Des.wallLengths.ToArray());
            var info = LevelGenerator.CreateRandomLevelInfo(parms, out m_Des.valid);
            CreateObjectsAndFocusCamera(info);
            EditorUtilsEx.ClearConsole();
        }

        private void CreateDefault()
        {
            ClearLevel();
            var info = LevelGenerator.CreateDefaultLevelInfo(m_Des.sizes[m_Des.sizeIdx], true);
            CreateObjectsAndFocusCamera(info);
            EditorUtilsEx.ClearConsole();
        }

        private void CreateObjectsAndFocusCamera(LevelInfo _Info)
        {
            m_Des.prototype = LevelProt.Create(
                _Info,
                GetParent("WallsAndNodes"));
            var bounds = new Bounds(
                Vector3.zero.SetXY(new Vector2((_Info.Width - 1) * 0.5f, (_Info.Height - 1) * 0.5f)),
                new Vector3(_Info.Width * 0.7f, _Info.Height * 0.7f, 10));
            EditorUtilsEx.FocusSceneCamera(bounds);
        }

        private void ClearLevel()
        {
            var prototype = m_Des.prototype;
            if (prototype == null)
                return;
            foreach (var node in prototype.items)
                node.gameObject.DestroySafe();
            prototype.items.Clear();
        }

        private void CheckLevelOnSceneForValidity()
        {
            var info = GetLevelInfoFromScene();
            m_Des.valid = LevelAnalizator.IsValid(info, false);
        }

        private LevelInfo GetLevelInfoFromScene()
        {
            var prot = m_Des.prototype;
            var nodes = prot.items
                .Where(_Item => _Item.type == PrototypingItemType.Node)
                .Select(_Item => _Item.transform.position.XY().ToVector2Int())
                .Select(_PosInt => new Node(_PosInt) as INode)
                .ToList();
            var nodeStart = prot.items.Where(_Item => _Item.type == PrototypingItemType.NodeStart)
                .Select(_Item => new NodeStart(_Item.transform.position.XY().ToVector2Int())).First();
            nodes.Insert(0, nodeStart);
            var wallBlocks  = prot.items
                .Where(_Item => _Item.type == PrototypingItemType.WallBlockSimple)
                .Select(_Item => _Item.transform.position.XY().ToVector2Int())
                .Select(_PosInt => new WallBlockSimple(_PosInt) as IWallBlock)
                .ToList();
            return new LevelInfo(prot.Width, prot.Height, nodes, wallBlocks);
        }

        private static Transform GetParent(string _Name)
        {
            var go = GameObject.Find(_Name) ?? new GameObject(_Name);
            return go.transform;
        }
    }
}