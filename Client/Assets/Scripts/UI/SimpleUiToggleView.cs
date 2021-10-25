using Constants;
using DI.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class SimpleUiToggleView : SimpleUiItemBase, INormalPressedDisabled
    {
        [SerializeField] private Image checkMark;

        protected override void OnEnable()
        {
            if (checkMark.IsNotNull())
                checkMark.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiToggleCheckMark);
            base.OnEnable();
        }

        public void SetNormal()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiToggleNormal));
        }

        public void SetPressed()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiTogglePressed));
        }

        public void SetSelected()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiTogglePressed));
        }

        public void SetDisabled()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiToggleDisabled));
        }
    }
}