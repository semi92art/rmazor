using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers;
using UnityEngine.Analytics;
using Utils;

namespace Managers
{
    public class AnalyticsManager : MonoBehaviour, ISingleton
    {
       #region singleton
        
            private static AnalyticsManager _instance;
            private static GameObject _analyticsObject;
            public static AnalyticsManager Instance
            {
                get
                {
                    if (_instance is AnalyticsManager ptm && !ptm.IsNull())
                        return _instance;
                    _analyticsObject = new GameObject("Analytics Manager");
                    _instance = _analyticsObject.AddComponent<AnalyticsManager>();
                    DontDestroyOnLoad(_analyticsObject);
                    return _instance;
                }
            }
        
            #endregion
    
            #region private members
        
           
            #endregion
    
            #region api

            public void Init()
            {
               // AnalyticsEventTracker analyticsEventTracker = _analyticsObject.AddComponent<AnalyticsEventTracker>();
               // analyticsEventTracker.m_Trigger.lifecycleEvent.
               //  
               //  Debug.Log("Analytics enabled");
            }
            #endregion
    }
}
