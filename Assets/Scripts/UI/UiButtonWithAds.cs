using Constants;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class UiButtonWithAds : SimpleUiButtonView
    {
        [SerializeField] private Image watchAd;

        protected override void OnEnable()
        {
            if (!watchAd.IsNull())
                watchAd.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiBorderDefault);
            base.OnEnable();
        }
    }
}