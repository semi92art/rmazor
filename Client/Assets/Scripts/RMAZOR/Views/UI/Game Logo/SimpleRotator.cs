using Common.Ticker;
using UnityEngine;

namespace RMAZOR.Views.UI.Game_Logo
{
    public class SimpleRotator : MonoBehaviour, IFixedUpdateTick
    {
        public Transform[] rotationObjects;

        private IViewGameTicker m_Ticker;
        private float           m_RotationSpeed;
        private bool            m_Initialized;
        
        public void Init(IViewGameTicker _Ticker, float _Speed)
        {
            _Ticker.Register(this);
            m_Ticker = _Ticker;
            m_RotationSpeed = _Speed;
            m_Initialized = true;
        }

        public void FixedUpdateTick()
        {
            if (!m_Initialized)
                return;
            for (int i = 0; i < rotationObjects.Length; i++)
            {
                var tr = rotationObjects[i];
                tr.Rotate(0f, 0f, m_RotationSpeed * m_Ticker.FixedDeltaTime * 100f);
            }
        }
    }
}