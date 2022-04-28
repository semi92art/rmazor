using System;
using Common.Constants;
using Common.Extensions;
using Common.Utils;
using Lean.Common;
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

        private float          m_BottomScreenOffsetDefault;
        private float          m_TopScreenOffsetDefault;
        private SpriteRenderer m_Renderer;
        
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
            // if (_instance.IsNotNull())
            //     _instance.Show();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Show()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            m_Renderer = gameObject.AddComponentOnNewChild<SpriteRenderer>("Renderer", out _);
            m_Renderer.sprite = image;
            m_Renderer.color = Color.white.SetA(0f);
            m_Renderer.transform.SetPosXY(bounds.center.x, bounds.min.y + bottomImageOffset);
            Cor.Run(Cor.Lerp(
                null,
                0.5f,
                _OnProgress: _P => m_Renderer.color = Color.white.SetA(_P)));
        }

        private void Hide()
        {
            Cor.Run(Cor.Lerp(
                null,
                0.5f,
                _OnProgress: _P => m_Renderer.color = Color.white.SetA(1f - _P)));
        }

        private void OnDestroy()
        {
            if (!show)
                return;
            m_ViewSettings.bottomScreenOffset = m_BottomScreenOffsetDefault;
            m_ViewSettings.topScreenOffset = m_TopScreenOffsetDefault;
        }

        private void Update()
        {
            if (LeanInput.GetDown(KeyCode.P))
                Show();
            if (LeanInput.GetDown(KeyCode.O))
                Hide();
        }
    }
}