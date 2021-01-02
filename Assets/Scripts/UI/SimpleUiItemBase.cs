using System.Collections.Generic;
using System.Linq;
using Constants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using Utils;

namespace UI
{
    public interface INormalPressedDisabled
    {
        void SetNormal();
        void SetPressed();
        void SetDisabled();
    }
    
    public abstract class SimpleUiItemBase : MonoBehaviour
    {
        [SerializeField] protected ProceduralImage background;
        [SerializeField] protected ProceduralImage border;
        [SerializeField] protected Image shadow;
        [SerializeField] protected List<TextMeshProUGUI> texts;
        private const float TransitionTime = 0.2f;
        private int m_TransitionCount;

        protected virtual void OnEnable()
        {
            if (!border.IsNull())
                border.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiBorderDefault);
            foreach (var text in texts.Where(_Text => !_Text.IsNull()))
                text.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiTextDefault);
        }

        private void OnDestroy()
        {
            m_TransitionCount = 0;
        }
        
        protected void MakeTransition(Color _Background)
        {
            if (background.IsNull())
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
                UiTimeProvider.Instance));
        }
    }
}