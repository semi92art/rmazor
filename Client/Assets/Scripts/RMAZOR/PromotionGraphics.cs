using System;
using Common.Constants;
using Common.Extensions;
using Common.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace RMAZOR
{
    public class PromotionGraphics : MonoBehaviour
    {
        [SerializeField] private bool   show;
        [SerializeField] private Sprite image;
        [SerializeField] private float  bottomImageOffset;

        private static PromotionGraphics _instance;

        private ViewSettings m_ViewSettings;

        private float m_BottomScreenOffsetDefault;
        private float m_TopScreenOffsetDefault;
        
        [Inject]
        private void Inject(ViewSettings _ViewSettings)
        {
            m_ViewSettings = _ViewSettings;
        }

        private void Start()
        {
            _instance = this;
            if (!show)
                return;
            (m_BottomScreenOffsetDefault, m_ViewSettings.bottomScreenOffset) = (m_ViewSettings.bottomScreenOffset, 25f);
            (m_TopScreenOffsetDefault, m_ViewSettings.topScreenOffset) = (m_ViewSettings.topScreenOffset, 0f);
            DontDestroyOnLoad(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
        {
            if (_Scene.name != SceneNames.Level)
                return;
            if (_instance.IsNotNull())
                _instance.Show();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Show()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            var rend = gameObject.AddComponentOnNewChild<SpriteRenderer>("Renderer", out _);
            rend.sprite = image;
            rend.transform.SetPosXY(bounds.center.x, bounds.min.y + bottomImageOffset);
        }

        private void OnDestroy()
        {
            m_ViewSettings.bottomScreenOffset = m_BottomScreenOffsetDefault;
            m_ViewSettings.topScreenOffset = m_TopScreenOffsetDefault;
        }
    }
}