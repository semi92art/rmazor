using Extensions;
using Helpers;
using Managers;
using UI;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace DialogViewers
{
    public interface IMenuDialogViewer : IDialogViewer
    {
        void Show(IMenuDialogPanel _ItemTo, bool _HidePrevious = true);
        void AddNotDialogItem(RectTransform _Item, MenuUiCategory _Categories);
    }
    
    public class MainMenuDialogViewer : DialogViewerBase, IMenuDialogViewer
    {
        #region nonpublic members
        
        private Sprite m_IconBack;
        private Sprite m_IconClose;
        private Button m_CloseButton;
        private Image m_CloseButtonIcon;

        #endregion
    
        #region engine methods

        private void Start()
        {
            GameObject go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    gameObject.RTransform(),
                    UiAnchor.Create(0.5f, 0.5f, 0.5f, 0.5f),
                    new Vector2(0, -418f),
                    Vector2.one * 0.5f,
                    Vector2.one * 60f),
                "main_menu_buttons", "dialog_close_button");
            m_CloseButton = go.GetCompItem<Button>("button");
            m_CloseButtonIcon = go.GetCompItem<Image>("close_icon");
            m_CloseButton.SetOnClick(() =>
            {
                SoundManager.Instance.PlayMenuButtonClick();
                Back();
            });
            GraphicsAlphas.Add(m_CloseButton.RTransform().GetInstanceID(), new GraphicAlphas(m_CloseButton.RTransform()));
            m_CloseButton.RTransform().gameObject.SetActive(false);

            m_IconBack = PrefabInitializer.GetObject<Sprite>("icons", "icon_back");
            m_IconClose = PrefabInitializer.GetObject<Sprite>("icons", "icon_close");
        }
        
        #endregion

        #region api

        public static IMenuDialogViewer Create(RectTransform _Parent)
        {
            var dialogPanelObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "main_menu_viewer");
            return dialogPanelObj.GetComponent<MainMenuDialogViewer>();
        }

        public void Show(IMenuDialogPanel _ItemTo, bool _HidePrevious = true)
        {
            var to = new Panel(_ItemTo);
            ShowCore(to, _HidePrevious, false, MenuUiCategoryType);
        }

        public void AddNotDialogItem(RectTransform _Item, MenuUiCategory _Categories)
        {
            if (NotDialogs.ContainsKey(_Item))
                return;
            NotDialogs.Add(_Item, new VisibleInCategories(_Categories, _Item.gameObject.activeSelf));
            GraphicsAlphas.Add(_Item.GetInstanceID(), new GraphicAlphas(_Item));
        }

        #endregion

        #region private methods
        protected override void FinishShowing(
            Panel _ItemFrom,
            Panel _ItemTo,
            bool _GoBack,
            RectTransform _PanelTo,
            int _UiCategoryType)
        {
            if (m_CloseButton != null && (_ItemFrom == null || _PanelTo == null))
            {
                StartCoroutine(Coroutines.DoTransparentTransition(m_CloseButton.RTransform(),
                    GraphicsAlphas[m_CloseButton.RTransform().GetInstanceID()].Alphas, TransitionTime, _GoBack));
            }
            
            base.FinishShowing(_ItemFrom, _ItemTo, _GoBack, _PanelTo, _UiCategoryType);
            
            SetCloseButtonIcon();
        }

        private void SetCloseButtonIcon()
        {
            if (m_CloseButton == null)
                return;
            m_CloseButtonIcon.sprite = PanelStack.GetItem(1) == null ? 
                m_IconClose : m_IconBack;
        }

        #endregion
    }
}