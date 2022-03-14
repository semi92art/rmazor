#if UNITY_EDITOR
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using Common.Utils;
using UnityEditor;
using UnityEngine;

namespace RMAZOR.Views.Debug
{
    public class ScreenViewDebug : MonoBehaviour
    {
        private static ScreenViewDebug _instance;
        public static ScreenViewDebug Instance =>
            _instance.IsNotNull() ? _instance : _instance = FindObjectOfType<ScreenViewDebug>(); 
        
        private                  MazeCoordinateConverter m_Converter;
        private                  ViewSettings            m_Settings;
        [SerializeField] private Vector2                 mazeSize;
        public                   bool                    drawMazeBounds;
        public                   bool                    drawScreenOffsets;
        
        public V2Int MazeSize
        {
            set
            {
                mazeSize = value;
                SetMazeSize();
            }
        }

        public void SetMazeSize()
        {
            if (mazeSize.x <= 0 || mazeSize.y <= 0)
            {
                Dbg.LogError("Maze size incorrect.");
                return;
            }
            m_Settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ViewSettings>(
                "configs", "view_settings");
            m_Converter = new MazeCoordinateConverter(m_Settings, null, true);
            m_Converter.Init();
            m_Converter.SetMazeSize((V2Int)mazeSize);
        }

        private void OnDrawGizmos()
        {
            if (m_Converter == null)
                return;
            if (!m_Converter.InitializedAndMazeSizeSet())
                return;
            Gizmos.color = Color.red;
            var mazeBds = m_Converter.GetMazeBounds();
            var scrBds = GraphicUtils.GetVisibleBounds();
            
            if (drawMazeBounds)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(mazeBds.center, mazeBds.size);
                // Gizmos.DrawLine(new Vector2(mazeBds.min.x, scrBds.min.y), new Vector2(mazeBds.min.x, scrBds.max.y));
                // Gizmos.DrawLine(new Vector2(mazeBds.max.x, scrBds.min.y), new Vector2(mazeBds.max.x, scrBds.max.y));
                // Gizmos.DrawLine(new Vector2(scrBds.min.x, mazeBds.min.y), new Vector2(scrBds.max.x, mazeBds.min.y));
                // Gizmos.DrawLine(new Vector2(scrBds.min.x, mazeBds.min.y), new Vector2(scrBds.max.x, mazeBds.min.y));
                Gizmos.DrawCube(mazeBds.center, Vector3.one);
            }
            if (drawScreenOffsets)
            {
                Gizmos.color = Color.green;
                float a = scrBds.min.x + m_Settings.LeftScreenOffset;
                Gizmos.DrawLine(new Vector2(a, scrBds.min.y), new Vector2(a, scrBds.max.y));
                a = scrBds.max.x - m_Settings.RightScreenOffset;
                Gizmos.DrawLine(new Vector2(a, scrBds.min.y), new Vector2(a, scrBds.max.y));
                a = scrBds.min.y + m_Settings.BottomScreenOffset;
                Gizmos.DrawLine(new Vector2(scrBds.min.x, a), new Vector2(scrBds.max.x, a));
                a = scrBds.max.y - m_Settings.TopScreenOffset;
                Gizmos.DrawLine(new Vector2(scrBds.min.x, a), new Vector2(scrBds.max.x, a));
            }
        }
    }

    [CustomEditor(typeof(ScreenViewDebug))]
    public class ScreenViewDebugEditor : Editor
    {
        private ScreenViewDebug m_T;

        private void OnEnable()
        {
            m_T = target as ScreenViewDebug;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Set size"))
                m_T.SetMazeSize();
        }
    }

}
#endif
