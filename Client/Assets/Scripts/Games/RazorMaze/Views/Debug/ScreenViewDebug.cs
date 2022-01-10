#if UNITY_EDITOR
using DI.Extensions;
using Entities;
using GameHelpers;
using Managers;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Debug
{
    public class ScreenViewDebug : MonoBehaviour
    {
        private static ScreenViewDebug _instance;
        public static ScreenViewDebug Instance =>
            _instance.IsNotNull() ? _instance : _instance = FindObjectOfType<ScreenViewDebug>(); 
        
        private                  MazeCoordinateConverter m_Converter;
        [SerializeField] private Vector2                 mazeSize;
        
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
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ViewSettings>(
                "model_settings", "view_settings");
            m_Converter = new MazeCoordinateConverter(settings, null);
            m_Converter.Init();
            m_Converter.MazeSize = (V2Int)mazeSize;
        }

        private void OnDrawGizmos()
        {
            if (m_Converter == null)
                return;
            if (!m_Converter.InitializedAndMazeSizeSet())
                return;
            var mazeBounds = m_Converter.GetMazeBounds();
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(mazeBounds.center, mazeBounds.size);
            var bds = m_Converter.GetMazeBounds();
            var offsets = new Vector4(bds.min.x, bds.max.x, bds.min.y, bds.max.y);
            var scrBds = GraphicUtils.GetVisibleBounds();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector2(offsets.x, scrBds.min.y), new Vector2(offsets.x, scrBds.max.y));
            Gizmos.DrawLine(new Vector2(offsets.y, scrBds.min.y), new Vector2(offsets.y, scrBds.max.y));
            Gizmos.DrawLine(new Vector2(scrBds.min.x, offsets.z), new Vector2(scrBds.max.x, offsets.z));
            Gizmos.DrawLine(new Vector2(scrBds.min.x, offsets.w), new Vector2(scrBds.max.x, offsets.w));
        }
    }

    [CustomEditor(typeof(ScreenViewDebug))]
    public class ScreenViewDebugEditor : Editor
    {
        private ScreenViewDebug t;
        private V2Int           size;

        private void OnEnable()
        {
            t = target as ScreenViewDebug;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Set size"))
                t.SetMazeSize();
        }
    }

}
#endif
