using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Games.RazorMaze.Views.Common;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using Utils;

namespace UI
{
    public abstract class SimpleUiItemBase : MonoBehaviour
    {
        [SerializeField] protected Image                 background;
        [SerializeField] protected Image                 border;
        [SerializeField] protected List<TextMeshProUGUI> texts;
        private const              float                 TransitionTime = 0.2f;
        private                    int                   m_TransitionCount;
        private                    ITicker               m_Ticker;
        protected                  IColorProvider        m_ColorProvider;
        private                    bool                  m_Initialized;

        protected void InitCore(IUITicker _Ticker, IColorProvider _ColorProvider)
        {
            m_Ticker = _Ticker;
            m_ColorProvider = _ColorProvider;
            m_Initialized = true;
        }

        protected virtual void OnEnable()
        {
            if (!border.IsNull())
                border.color = m_ColorProvider.GetColor(ColorIds.UiBorderDefault);
            foreach (var text in texts.Where(_Text => !_Text.IsNull()))
                text.color = m_ColorProvider.GetColor(ColorIds.UiTextDefault);
        }

        private void OnDestroy()
        {
            m_TransitionCount = 0;
        }
        
        protected void MakeTransition(Color _Background)
        {
            if (background.IsNull() || !m_Initialized)
                return;
            m_TransitionCount++;
            Color from = background.color;
            Color to = _Background;
            int transCount = m_TransitionCount;
            Coroutines.Run(Coroutines.Lerp(
                from,
                to,
                TransitionTime,
                _Color =>
                {
                    if (transCount != m_TransitionCount) return;
                    background.color = _Color;
                },
                m_Ticker));
        }
    }
}