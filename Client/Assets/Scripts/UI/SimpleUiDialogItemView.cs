using Constants;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class SimpleUiDialogItemView : SimpleUiItemBase
    {
        #region serialized

        [SerializeField] protected Image dialogBackground;  

        #endregion
        
        #region nonpublic members

        protected IManagersGetter Managers { get; private set; }

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

            if (dialogBackground.IsNotNull())
                dialogBackground.color = _ColorProvider.GetColor(ColorIds.UiDialogBackground);
        }
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiDialogBackground)
            {
                if (dialogBackground.IsNotNull())
                    dialogBackground.color = _Color;
            }
        }
        
        protected void SoundOnClick()
        {
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }

        #endregion
    }
}