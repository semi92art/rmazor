﻿using UnityEngine.SceneManagement;
using Utils;

namespace UnityGameLoopDI
{
    public interface ITicker
    {
        void Register(object _Object);
        void Unregister(object _Object);
        void Clear();
    }
    
    public class Ticker : ITicker
    {
        private static TickerManager _instance;
        private static TickerManager Instance => 
            CommonUtils.MonoBehSingleton(ref _instance, "Ticker Manager");
        
        public void Register(object _Object)
        {
            Instance.RegisterObject(_Object);
        }

        public void Unregister(object _Object)
        {
            Instance.UnregisterObject(_Object);
        }

        public void Clear()
        {
            Instance.Clear();
        }
    }
}