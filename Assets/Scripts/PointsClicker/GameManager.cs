using Helpers;
using UnityEngine;

namespace PointsClicker
{
    public class GameManager : MonoBehaviour, ISingleton
    {
        #region singleton
        
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;
                var go = new GameObject("Game Manager");
                _instance = go.AddComponent<GameManager>();
                return _instance;
            }
        }
        
        #endregion
        
        #region private members

        private GameObject m_PointPrefab;

        #endregion

        #region engine methods

        private void Start()
        {
            m_PointPrefab = PrefabInitializer.GetPrefab("points_clicker", "point");
        }

        private void Update()
        {
            
        }

        #endregion
    }
}