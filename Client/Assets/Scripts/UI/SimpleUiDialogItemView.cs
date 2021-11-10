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

        #endregion

        #region api

        public void SetNormal()
        {
            MakeTransition(m_ColorProvider.GetColor(ColorIds.UiDialogItemNormal));
        }

        public void SetPressed()
        {
            MakeTransition(m_ColorProvider.GetColor(ColorIds.UiDialogItemPressed));
        }

        public void SetSelected()
        {
            MakeTransition(m_ColorProvider.GetColor(ColorIds.UiDialogItemSelected));
        }

        public void SetDisabled()
        {
            MakeTransition(m_ColorProvider.GetColor(ColorIds.UiDialogItemDisabled));
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
                background.color = _ColorProvider.GetColor(ColorIds.UiDialogItemNormal);
        }
        
        protected void SoundOnClick()
        {
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }

        #endregion
    }
}