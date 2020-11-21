using System;
using Constants;
using Helpers;
using Lean.Touch;
using Managers;
using UnityEngine;
using Utils;
using ColorUtils = Utils.ColorUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PointsTapper
{
    public sealed class PointsTapperManager : GameManagerBase
    {
        #region singleton
        
        private static IGameManager _instance;

        public static IGameManager Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;
                var go = new GameObject("Game Manager");
                _instance = go.AddComponent<PointsTapperManager>();
                return _instance;
            }
        }
        
        #endregion
        
        #region private members
        
        private IPointItemsGenerator m_PointItemsGenerator;
        
        #endregion
        
        #region api

        public bool DoInstantiate { get; set; }

        public override void Init(int _Level, long _LifesOnStart)
        {
            LevelController = new PointsTapperLevelControllerBasedOnTime();
            m_PointItemsGenerator = new PointItemsGenerator();
            SpriteRenderer background = new GameObject().AddComponent<SpriteRenderer>();
            background.sprite = PrefabInitializer.GetObject<Sprite>("points_tapper", "background");
            GameUtils.FillByCameraRect(background);
            background.color = ColorUtils.GetColorFromPalette("Points Tapper", "Background");
            background.sortingOrder = SortingOrders.Background;

            var args = new LevelStateChangedArgs {Level = _Level, LifesLeft = _LifesOnStart};
            GameMenuUi = new PointsTapperGameMenuUi();
            GameMenuUi.OnGameStarted(args, () => OnLevelStarted(args));
            base.Init(_Level, _LifesOnStart);
        }

#if UNITY_EDITOR
        public void GenerateItem(PointType _PointType, float _Radius)
        {
            m_PointItemsGenerator.ActivateItem(_PointType, _Radius);
        }
#endif

        protected override void OnLifesChanged(LifeEventArgs _Args)
        {
            GameMenuUi.OnLifesChanged(_Args);
        }

        protected override void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            DoInstantiate = true;
            if (m_PointItemsGenerator is IOnLevelStartedFinished olc)
                olc.OnLevelStarted(_Args);
            GameMenuUi.OnLevelStarted(_Args);
        }
        
        protected override void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            DoInstantiate = false;
            if (m_PointItemsGenerator is IOnLevelStartedFinished olc)
                olc.OnLevelFinished(_Args);
        }
        
        #endregion

        #region nonpublic methods

        protected override void InitTouchSystem()
        {
            var go = new GameObject("Lean Select");
            LeanSelect ls = go.AddComponent<LeanSelect>();
            ls.SelectUsing = LeanSelectBase.SelectType.Overlap2D;
            ls.Search = LeanSelectBase.SearchType.GetComponent;
            ls.Camera = Camera.main;
            ls.LayerMask = LayerMask.GetMask(LayerNames.Touchable);
            ls.MaxSelectables = 1;
            ls.Reselect = LeanSelect.ReselectType.SelectAgain;
            ls.AutoDeselect = false;
            ls.SuppressMultipleSelectWarning = false;

            LeanFingerDown lfd = go.AddComponent<LeanFingerDown>();
            lfd.IgnoreStartedOverGui = true;
            lfd.OnFinger.AddListener(ls.SelectScreenPosition);
            base.InitTouchSystem();
        }

        #endregion

        #region engine methods

        protected override void Start()
        {
            base.Start();
            LevelController.StartLevel(() => true);
        }

        private void Update()
        {
            if (DoInstantiate)
                m_PointItemsGenerator.GenerateItemsOnUpdate();
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            
            if (m_PointItemsGenerator is IOnDrawGizmos odg)
                odg.OnDrawGizmos();
#endif
        }

        #endregion
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(PointsTapperManager))]
    public class PointsTapperManagerEditor : Editor
    {
        private PointsTapperManager m_Manager;
        private float m_Radius = 2f;

        private void OnEnable()
        {
            m_Manager = (PointsTapperManager) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Normal"))
                m_Manager.GenerateItem(PointType.Normal, m_Radius);
            if (GUILayout.Button("Bad"))
                m_Manager.GenerateItem(PointType.Bad, m_Radius);
            GUILayout.Label("Radius: ");
            m_Radius = EditorGUILayout.FloatField(m_Radius);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Do Instantiate"))
                m_Manager.DoInstantiate = true;
            if (GUILayout.Button("Do Not Instantiate"))
                m_Manager.DoInstantiate = false;
            GUILayout.EndHorizontal();
        }
    }
#endif
}