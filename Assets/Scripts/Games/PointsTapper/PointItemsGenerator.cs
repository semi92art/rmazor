using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.PointsTapper
{
    public interface IPointItemsGenerator
    {
        Dictionary<PointType, PointsPool> Pools { get; }
        void ActivateItem(PointType _PointType, float _Radius);
        bool DoInstantiate { get; set; }
        void ClearAllPoints();
    }

    public interface IOnLevelStartedFinished
    {
        void OnLevelStarted(LevelStateChangedArgs _Args);
        void OnLevelFinished(LevelStateChangedArgs _Args);
    }

    public class PointItemsGenerator : DI.DiObject, IPointItemsGenerator, IOnLevelStartedFinished
    {
        #region public properties

        public Dictionary<PointType, PointsPool> Pools { get; }
        public bool DoInstantiate { get; set; }
        
        #endregion
        
        #region nonpublic members
        
        private readonly RandomPositionGenerator m_PositionGenerator;
        private readonly Vector4 m_Margin;
        private float m_CurrentTime;
        private float m_CurrentLevel;
        private bool m_IsLevelInProcess;
        
        #endregion
        
        #region api
        
        public PointItemsGenerator(
            Dictionary<PointType, UnityAction> _TapActions = null,
            Dictionary<PointType, UnityAction> _NotTappedActions = null)
        {
            Pools = new Dictionary<PointType, PointsPool>
            {
                {PointType.Default, new PointsPool(PointType.Default, 30)},
                {PointType.Bad, new PointsPool(PointType.Bad, 30)},
                {PointType.BonusGold, new PointsPool(PointType.BonusGold, 30)}
            };

            foreach (var kvp in Pools)
            {
                if (kvp.Value == null)
                    continue;
                foreach (var item in kvp.Value)
                {
                    item.SetActions(
                        () => _TapActions?[kvp.Key]?.Invoke(),
                        () => _NotTappedActions?[kvp.Key]?.Invoke());
                }
            }
            
            
            m_Margin = new Vector4(2f, 2f, 4f, 2f);
            m_PositionGenerator = new RandomPositionGenerator(Pools.Values.ToList(), m_Margin);
        }
        
        [DI.Update]
        private void GenerateItemsOnUpdate()
        {
            if (!DoInstantiate)
                return;
            
            if (!m_IsLevelInProcess)
                return;
            
            if (m_CurrentLevel <= 3)
                GenerateOnLevels1_3();
            else if (m_CurrentLevel <= 10)
                GenerateOnLevels4_10();
        }

        public void ActivateItem(PointType _PointType, float _Radius)
        {
            var pool = Pools
                .First(_Kvp => _Kvp.Key == _PointType)
                .Value;
            var item = pool.FirstInactive;
            item.Radius = _Radius;
            pool.Activate(item, m_PositionGenerator.Next(_Radius + 1f));
        }

        public void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            m_CurrentLevel = _Args.Level;
            m_IsLevelInProcess = true;
        }

        public void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            m_IsLevelInProcess = false;
            foreach (var pool in Pools.Values)
            foreach (var item in pool)
                item.ForceActivate(false);
        }
        
        public void ClearAllPoints()
        {
            foreach (var pool in Pools.Values.ToList())
            {
                pool.Clear();
            }
        }

#if UNITY_EDITOR
        
        [DI.DrawGizmos]
        public void DrawGizmos()
        {
            var bounds = GameUtils.GetVisibleBounds();
            var topLeft = new Vector3(
                bounds.center.x - bounds.size.x + m_Margin.x,
                bounds.center.y + bounds.size.y - m_Margin.z);
            var topRight = new Vector3(
                bounds.center.x + bounds.size.x - m_Margin.y,
                bounds.center.y + bounds.size.y - m_Margin.z);
            var bottomLeft = new Vector3(
                bounds.center.x - bounds.size.x + m_Margin.x,
                bounds.center.y - bounds.size.y + m_Margin.w);
            var bottomRight = new Vector3(
                bounds.center.x + bounds.size.x - m_Margin.y,
                bounds.center.y - bounds.size.y + m_Margin.w);

            GameUtils.DrawGizmosRect(
                topLeft, 
                topRight, 
                bottomLeft, 
                bottomRight,
                Color.white);
        }
        
#endif

        #endregion
        
        #region nonpublic methods

        private void GenerateOnLevels1_3()
        {
            float minTime = 1f;
            float maxTime = 1.5f;
            
            float dt = minTime + CommonUtils.RandomGen.NextFloat() * (maxTime - minTime);
            if (GameTimeProvider.Instance.Time < m_CurrentTime + dt)
                return;
            m_CurrentTime = GameTimeProvider.Instance.Time;
            PointType pt = CommonUtils.RandomGen.NextFloat() < 0.8f ? PointType.Default : PointType.BonusGold;
            ActivateItem(pt, 3f);
        }

        private void GenerateOnLevels4_10()
        {
            float minTime = 0.5f;
            float maxTime = 3f;
            
            float dt = minTime + CommonUtils.RandomGen.NextFloat() * (maxTime - minTime);
            if (GameTimeProvider.Instance.Time < m_CurrentTime + dt)
                return;
            m_CurrentTime = GameTimeProvider.Instance.Time;

            float rand = CommonUtils.RandomGen.NextFloat();

            PointType pt;

            if (rand < 0.6f) pt = PointType.Default;
            else if (rand < 0.8f) pt = PointType.Bad;
            else pt = PointType.BonusGold;

            ActivateItem(pt, 3f);
        }

        #endregion
        
    }
}