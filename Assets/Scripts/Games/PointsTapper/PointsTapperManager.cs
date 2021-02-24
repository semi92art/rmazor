using System.Collections.Generic;
using Constants;
using Lean.Touch;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace Games.PointsTapper
{
    public sealed class PointsTapperManager : GameManagerBase
    {
        #region nonpublic members

        private IPointItemsGenerator m_PointItemsGenerator;

        #endregion

        #region singleton

        public static IGameManager Instance => GetInstance<PointsTapperManager>();

        #endregion

        #region engine methods

        protected override void OnDestroy()
        {
            m_PointItemsGenerator.ClearAllPoints();
            base.OnDestroy();
        }

        #endregion

        #region api

        public bool DoInstantiate
        {
            set => m_PointItemsGenerator.DoInstantiate = value;
        }

        public override void Init(int _Level)
        {
            m_PointItemsGenerator = new PointItemsGenerator(
                new Dictionary<PointType, UnityAction>
                {
                    {PointType.Default, () => MainScoreController.Score++},
                    {PointType.Bad, () => LifesController.MinusOneLife()},
                    {PointType.BonusGold, () => RevenueController.AddRevenue(BankItemType.FirstCurrency, 300)}
                });
            GameMenuUi = new PointsTapperGameMenuUi();
            base.Init(_Level);
        }

#if UNITY_EDITOR
        public void GenerateItem(PointType _PointType, float _Radius)
        {
            m_PointItemsGenerator.ActivateItem(_PointType, _Radius);
        }
#endif

        #endregion

        #region nonpublic methods

        protected override void InitTouchSystem()
        {
            var go = new GameObject("Lean Select");
            var ls = go.AddComponent<LeanSelect>();
            ls.SelectUsing = LeanSelectBase.SelectType.Overlap2D;
            ls.Search = LeanSelectBase.SearchType.GetComponent;
            ls.Camera = Camera.main;
            ls.LayerMask = LayerMask.GetMask(LayerNames.Touchable);
            ls.MaxSelectables = 1;
            ls.Reselect = LeanSelect.ReselectType.SelectAgain;
            ls.AutoDeselect = false;
            ls.SuppressMultipleSelectWarning = false;

            var lfd = go.AddComponent<LeanFingerDown>();
            lfd.IgnoreStartedOverGui = true;
            lfd.OnFinger.AddListener(ls.SelectScreenPosition);
            base.InitTouchSystem();
        }

        protected override void OnBeforeLevelStarted(LevelStateChangedArgs _Args)
        {
            DoInstantiate = false;
            base.OnBeforeLevelStarted(_Args);
        }

        protected override void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            DoInstantiate = true;
            ((IOnLevelStartedFinished) m_PointItemsGenerator).OnLevelStarted(_Args);
            base.OnLevelStarted(_Args);
        }

        protected override void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            DoInstantiate = false;
            ((IOnLevelStartedFinished) m_PointItemsGenerator).OnLevelFinished(_Args);
            base.OnLevelFinished(_Args);
        }

        protected override void OnTimeEnded()
        {
            DoInstantiate = false;
            ((IOnLevelStartedFinished) m_PointItemsGenerator).OnLevelFinished(null);
            base.OnTimeEnded();
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
    }
}