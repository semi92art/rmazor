using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Games.RazorMaze.Prot;
using Malee.List;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Games.RazorMaze
{
    [InitializeOnLoad]
    public class LevelDesigner : MonoBehaviour
    {
        [Serializable] public class WallLengthList : ReorderableArray<int> { }
        
        private const int MinSize = 5;
        private const int MaxSize = 20;

        [Header("Wall lengths"), Reorderable(paginate = true, pageSize = 5)] public WallLengthList wallLengths;
        
        [HideInInspector] public List<int> sizes = Enumerable.Range(MinSize, MaxSize).ToList();
        [HideInInspector] public int sizeIdx;
        [HideInInspector] public float aParam;
        [HideInInspector] public bool valid;
        [HideInInspector] public MazeProtItems prototype;

        private static MazeInfo MazeInfo
        {
            get => SaveUtils.GetValue<MazeInfo>(SaveKey.DesignerMazeInfo);
            set => SaveUtils.PutValue(SaveKey.DesignerMazeInfo, value);
        }
        
        static LevelDesigner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        public static void Play(MazeInfo _Info)
        {
            MazeInfo = _Info;
            GameClientUtils.GameId = 1;
            GameClientUtils.GameMode = 0;
            EditorApplication.isPlaying = true;
            Dbg.Log(EditorApplication.isPlaying);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _Change)
        {
            //Dbg.Log($"PlayModeStateChange: {_Change}");
            if (_Change != PlayModeStateChange.EnteredPlayMode)
                return;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
             var sceneName = SceneManager.GetActiveScene().name;
             if (!sceneName.Contains("prot"))
                 return;
            SceneManager.sceneLoaded += (_Scene, _Mode) =>
            {
                 RazorMazeGameManager.Instance.SetMazeInfo(MazeInfo);
                 RazorMazeGameManager.Instance.Init();
            };
            SceneManager.LoadScene(SceneNames.Level);
        }
    }
}