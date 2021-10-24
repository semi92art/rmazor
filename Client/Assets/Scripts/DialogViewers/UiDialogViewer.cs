using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Ticker;
using UI;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace DialogViewers
{
    public interface IDialogViewer : IDialogViewerBase
    {
        void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
        void CloseAll();
    }
    
    public class UiDialogViewer : IDialogViewer, IAction, IUpdateTick
    {
        #region types
        
        protected class GraphicAlphas
        {
            public Dictionary<Graphic, float> Alphas { get; }

            public GraphicAlphas(RectTransform _Item)
            {
                Alphas = _Item.GetComponentsInChildrenEx<Graphic>()
                    .Distinct()
                    .ToDictionary(_El => _El, _El => _El.color.a);
            }
        }

        #endregion

        #region constants

        private const int AkStateCloseButtonDisabled = 0;
        private const int AkStateCloseButtonEnabled = 1;
        private const float TransitionTime = 0.2f;

        #endregion
        

        #region nonpublic members

        private static int AkEnableCloseButton => AnimKeys.Anim2;
        private static int AkDisableCloseButton => AnimKeys.Stop2;
        private static int AkState => AnimKeys.State;
        
        private Button m_CloseButton;
        private Animator m_ButtonsAnim;
        private Image m_Background;
        private RectTransform m_DialogContainer;
        private readonly Stack<IDialogPanel> PanelStack = new Stack<IDialogPanel>();
        private readonly Dictionary<int, GraphicAlphas> GraphicsAlphas = 
            new Dictionary<int, GraphicAlphas>();

        #endregion

        #region inject

        private IManagersGetter Managers { get; }
        private IUITicker Ticker { get; }
        
        public UiDialogViewer(IManagersGetter _Managers, IUITicker _Ticker)
        {
            Managers = _Managers;
            Ticker = _Ticker;
            _Ticker.Register(this);
        }
        
        #endregion

        #region api
        
        public RectTransform Container => m_DialogContainer;
        public UnityAction Action { get; set; }

        public void Init(RectTransform _Parent)
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "dialog_viewer");
            
            m_Background = go.GetCompItem<Image>("background");
            m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
            m_CloseButton = go.GetCompItem<Button>("close_button");
            m_ButtonsAnim = go.GetCompItem<Animator>("buttons_animator");

            var borderColor = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiBorderDefault);
            var backgroundColor = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiDialogBackground);
            m_CloseButton.GetCompItem<Image>("border").color = borderColor;
            m_CloseButton.GetCompItem<Image>("background").color = backgroundColor;
            m_CloseButton.GetCompItem<Image>("icon").color = borderColor;
            
            m_CloseButton.SetOnClick(() =>
            {
                Managers.Notify(_SM => _SM.PlayClip(AudioClipNames.UIButtonClick));
                CloseAll();
            });
        }

        public void Show(IDialogPanel _ItemTo, bool _HidePrevious = true)
        {
            ShowCore(_ItemTo, _HidePrevious, false);
            m_CloseButton.transform.SetAsLastSibling();
        }
        
        public void CloseAll()
        {
            if (!PanelStack.Any())
                return;
            var lastPanel = PanelStack.Pop();
            var panelsToDestroy = new List<IDialogPanel>();
            while (PanelStack.Count > 0)
                panelsToDestroy.Add(PanelStack.Pop());
            
            foreach (var pan in panelsToDestroy
                .Where(_Panel => _Panel != null))
            {
                Object.Destroy(pan.Panel.gameObject);
            }
            
            PanelStack.Push(lastPanel);
            ShowCore(null, true, true);
        }
        
        public virtual void UpdateTick()
        {
            if (!NotificationViewer.IsShowing && Input.GetKeyDown(KeyCode.Escape))
                CloseAll();
        }
        
        #endregion

        #region nonpublic methods

        private void ShowCore(
            IDialogPanel _ItemTo,
            bool _HidePrevious,
            bool _GoBack)
        {
            var itemFrom = !PanelStack.Any() ? null : PanelStack.Peek();
            if (itemFrom == null && _ItemTo == null)
                return;

            var fromPanel = itemFrom?.Panel;
            var toPanel = _ItemTo?.Panel;
            EUiCategory menuCat = _ItemTo?.Category ?? EUiCategory.MainMenu;
            
            if (itemFrom != null && fromPanel != null && _HidePrevious)
            {
                int instId = fromPanel.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(fromPanel));
                Coroutines.Run(Coroutines.DoTransparentTransition(
                    fromPanel, GraphicsAlphas[instId].Alphas, TransitionTime,
                    Ticker,
                    true,
                    () =>
                    {
                        if (!_GoBack)
                            return;
                        Object.Destroy(fromPanel.gameObject);
                        var monobeh = itemFrom as MonoBehaviour;
                        if (monobeh != null)
                            Object.Destroy(monobeh.gameObject);
                    }));
            }

            if (toPanel != null)
            {
                int instId = toPanel.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(toPanel));
                Coroutines.Run(Coroutines.DoTransparentTransition(
                    toPanel, GraphicsAlphas[instId].Alphas, TransitionTime,
                    Ticker,
                    false, 
                    () => m_Background.enabled = true));
                _ItemTo.OnDialogEnable();
            }

            FinishShowing(itemFrom, _ItemTo, _GoBack, toPanel);
        }

        private void FinishShowing(
            IDialogPanel _ItemFrom,
            IDialogPanel _ItemTo,
            bool _GoBack,
            RectTransform _PanelTo)
        {
            m_Background.enabled = m_Background.raycastTarget = !(_PanelTo == null && _GoBack);
            ClearGraphicsAlphas();
        
            if (_PanelTo == null)
                ClearPanelStack();
            else
            {
                if (!PanelStack.Any())
                    PanelStack.Push(_ItemFrom);
                if (PanelStack.Any() && _GoBack)
                    PanelStack.Pop();
                if (!_GoBack)
                    PanelStack.Push(_ItemTo);
            }
            
            SetCloseButtonsState(_PanelTo == null);
        }
        
        private void ClearGraphicsAlphas()
        {
            foreach (var item in GraphicsAlphas.ToArray())
            {
                if (item.Value.Alphas.All(_A => _A.Key.IsNull()))
                    GraphicsAlphas.Remove(item.Key);
            }
        }
        
        private void ClearPanelStack()
        {
            var list = new List<IDialogPanel>();
            while(PanelStack.Any())
                list.Add(PanelStack.Pop());
            foreach (var monobeh in from item in list
                where item != null
                select item as MonoBehaviour)
            {
                Object.Destroy(monobeh);
            }
        }
        
        private void SetCloseButtonsState(bool _CloseAll)
        {
            if ( m_CloseButton == null)
                return;
            int state = m_ButtonsAnim.GetInteger(AkState);
            switch (state)
            {
                case AkStateCloseButtonDisabled:
                    if (_CloseAll)
                        return;
                    m_ButtonsAnim.SetTrigger(AkEnableCloseButton);
                    m_ButtonsAnim.SetInteger(AkState, AkStateCloseButtonEnabled);
                    break;
                case AkStateCloseButtonEnabled:
                    m_ButtonsAnim.SetTrigger(AkDisableCloseButton);
                    m_ButtonsAnim.SetInteger(AkState, AkStateCloseButtonDisabled);
                    break;
            }
        }

        #endregion
    }
}