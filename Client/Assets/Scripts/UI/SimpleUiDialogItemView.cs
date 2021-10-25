using Constants;
using DI.Extensions;
using Entities;
using Ticker;
using Utils;

namespace UI
{
    public interface INormalPressedDisabled
    {
        void SetNormal();
        void SetPressed();
        void SetSelected();
        void SetDisabled();
    }
    
    public abstract class SimpleUiDialogItemView : SimpleUiItemBase, INormalPressedDisabled
    {
        #region nonpublic members

        protected IManagersGetter Managers { get; private set; }

        #endregion

        #region api

        public void SetNormal()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiDialogItemNormal));
        }

        public void SetPressed()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiDialogItemPressed));
        }

        public void SetSelected()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiDialogItemSelected));
        }

        public void SetDisabled()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiDialogItemDisabled));
        }

        #endregion

        #region nonpublic methods

        protected override void OnEnable()
        {
            if (!background.IsNull())
                background.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiDialogItemNormal);
            base.OnEnable();
        }
        
        protected void InitCore(IManagersGetter _Managers, IUITicker _UITicker)
        {
            Managers = _Managers;
            InitCore(_UITicker);
        }
        
        protected void SoundOnClick()
        {
            Managers.Notify(_SM => 
                _SM.PlayClip(AudioClipNames.UIButtonClick));
        }

        #endregion
    }
}