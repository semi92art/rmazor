using Constants;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Views.Common;
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

        protected IManagersGetter Managers      { get; private set; }
        private   IColorProvider  ColorProvider { get; set; }

        #endregion

        #region api

        public void SetNormal()
        {
            MakeTransition(ColorProvider.GetColor(ColorIds.UiDialogItemNormal));
        }

        public void SetPressed()
        {
            MakeTransition(ColorProvider.GetColor(ColorIds.UiDialogItemPressed));
        }

        public void SetSelected()
        {
            MakeTransition(ColorProvider.GetColor(ColorIds.UiDialogItemSelected));
        }

        public void SetDisabled()
        {
            MakeTransition(ColorProvider.GetColor(ColorIds.UiDialogItemDisabled));
        }

        #endregion

        #region nonpublic methods
        
        protected void InitCore(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider)
        {
            Managers = _Managers;
            InitCore(_UITicker, _ColorProvider);

            if (!background.IsNull())
                background.color = ColorProvider.GetColor(ColorIds.UiDialogItemNormal);
        }
        
        protected void SoundOnClick()
        {
            Managers.SoundManager.PlayClip(AudioClipNames.UIButtonClick);
        }

        #endregion
    }
}