using Constants;
using Helpers;
using Lean.Touch;
using UnityEngine;

namespace PointsTapper
{
    public sealed class PointsTapperManager : GameManagerBase, ISingleton
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
        
        #region api

        public override void Init()
        {
            m_PointPrefab = PrefabInitializer.GetPrefab("points_clicker", "point");
            m_PointPrefab.SetActive(false);
        }
        
        #endregion
        
        #region private members

        private GameObject m_PointPrefab;
        private LeanSelect m_LeanSelect;
        private LeanFingerDown m_LeanFingerDown;

        #endregion

        #region engine methods

        protected override void Start_()
        {
            
        }

        protected override void Update_()
        {
            
        }

        #endregion
        
        #region nonpublic methods

        protected override void InitTouchSystem()
        {
            var go = new GameObject("Lean Select");
            m_LeanSelect = go.AddComponent<LeanSelect>();
            var ls = m_LeanSelect;
            ls.SelectUsing = LeanSelectBase.SelectType.Overlap2D;
            ls.Camera = Camera.main;
            ls.LayerMask = LayerMask.GetMask(LayerNames.Touchable);
            ls.MaxSelectables = 1;
            ls.Reselect = LeanSelect.ReselectType.SelectAgain;
            ls.AutoDeselect = false;
            ls.SuppressMultipleSelectWarning = false;

            m_LeanFingerDown = go.AddComponent<LeanFingerDown>();
            m_LeanFingerDown.IgnoreStartedOverGui = true;
            m_LeanFingerDown.OnFinger.AddListener(ls.SelectScreenPosition);
            base.InitTouchSystem();
        }

        #endregion
    }
}