using System.Collections.Generic;
using Constants;
using GameHelpers;
using Lean.Touch;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using ColorUtils = Utils.ColorUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PathFinder
{
    public sealed class PathFinderManager : GameManagerBase
    {
        #region singleton
        
        private static IGameManager _instance;

        public static IGameManager Instance
        {
            get
            {
                if (_instance is PathFinderManager ptm && !ptm.IsNull()) 
                    return _instance;
                var go = new GameObject("Game Manager");
                _instance = go.AddComponent<PathFinderManager>();
                return _instance;
            }
        }
        
        #endregion
        
        #region nonpublic members
        
        private IGridItemsGenerator m_GridItemsGenerator;

        #endregion
        
        #region api

        public bool DoInstantiate { set => m_GridItemsGenerator.DoInstantiate = value; }

        public override void Init(int _Level)
        {
            m_GridItemsGenerator = new GridItemsGenerator();
            m_GridItemsGenerator.GenerateItems();
            // m_PointItemsGenerator = new PointItemsGenerator(
            //     new Dictionary<PointType, UnityAction>
            // {
            //     {PointType.Default, () => MainScoreController.Score++},
            //     {PointType.Bad, () => LifesController.MinusOneLife()},
            //     {PointType.BonusGold, () => RevenueController.AddRevenue(MoneyType.Gold, 300)}
            // });
            // SpriteRenderer background = new GameObject().AddComponent<SpriteRenderer>();
            // background.sprite = PrefabInitializer.GetObject<Sprite>("points_tapper", "background");
            // GameUtils.FillByCameraRect(background);
            // background.color = ColorUtils.GetColorFromPalette("Points Tapper", "Background");
            // background.sortingOrder = SortingOrders.Background;
            GameMenuUi = new PathFinderGameMenuUi();
            base.Init(_Level);
        }
       
        #endregion

        #region nonpublic methods

        protected override void InitTouchSystem()
        {
            // var go = new GameObject("Lean Select");
            // LeanSelect ls = go.AddComponent<LeanSelect>();
            // ls.SelectUsing = LeanSelectBase.SelectType.Overlap2D;
            // ls.Search = LeanSelectBase.SearchType.GetComponent;
            // ls.Camera = Camera.main;
            // ls.LayerMask = LayerMask.GetMask(LayerNames.Touchable);
            // ls.MaxSelectables = 1;
            // ls.Reselect = LeanSelect.ReselectType.SelectAgain;
            // ls.AutoDeselect = false;
            // ls.SuppressMultipleSelectWarning = false;
            //
            // LeanFingerDown lfd = go.AddComponent<LeanFingerDown>();
            // lfd.IgnoreStartedOverGui = true;
            // lfd.OnFinger.AddListener(ls.SelectScreenPosition);
            // base.InitTouchSystem();
        }
        
        protected override void OnBeforeLevelStarted(LevelStateChangedArgs _Args)
        {
            DoInstantiate = false;
            base.OnBeforeLevelStarted(_Args);
        }
        
        protected override void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            DoInstantiate = true;
            ((IOnLevelStartedFinished)m_GridItemsGenerator).OnLevelStarted(_Args);
            base.OnLevelStarted(_Args);
        }
        
        protected override void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            DoInstantiate = false;
            ((IOnLevelStartedFinished)m_GridItemsGenerator).OnLevelFinished(_Args);
            base.OnLevelFinished(_Args);
        }
        
        protected override void OnTimeEnded()
        {
            DoInstantiate = false;
            ((IOnLevelStartedFinished)m_GridItemsGenerator).OnLevelFinished(null);
            base.OnTimeEnded();
        }

        protected override void OnLifesEnded()
        {
            DoInstantiate = false;
            ((IOnLevelStartedFinished)m_GridItemsGenerator).OnLevelFinished(null);
            base.OnLifesEnded();
        }
        
        protected override float LevelDuration(int _Level)
        {
            if (_Level < 5)
                return 20f;
            if (_Level < 10)
                return 25f;
            if (_Level < 20)
                return 30f;
            if (_Level < 50)
                return 40f;
            return 60f;
        }
        
        protected override int NecessaryScore(int _Level)
        {
            if (_Level < 5)
                return 10;
            if (_Level < 10)
                return 15;
            if (_Level < 20)
                return 20;
            if (_Level < 50)
                return 30;
            return 40;
        }

        #endregion

        #region engine methods

        protected override void OnDestroy()
        {
            m_GridItemsGenerator.ClearGrid();
            base.OnDestroy();
        }
        
        #endregion
    }
   
}
