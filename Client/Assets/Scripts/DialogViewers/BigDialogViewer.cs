using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.InputConfigurators;
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
    public interface IBigDialogViewer : IDialogViewerBase
    {
        void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
        void CloseAll();
    }
    
    public class BigDialogViewer : IBigDialogViewer, IAction, IUpdateTick
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
        private Animator m_CloseButtonAnim;
        private Image m_Background;
        private RectTransform m_DialogContainer;
        private readonly Stack<IDialogPanel> PanelStack = new Stack<IDialogPanel>();
        private readonly Dictionary<int, GraphicAlphas> GraphicsAlphas = 
            new Dictionary<int, GraphicAlphas>();

        #endregion

        #region inject

        private IManagersGetter        Managers          { get; }
        private IUITicker              Ticker            { get; }
        private IViewInput Input { get; }
        private IColorProvider         ColorProvider     { get; }

        public BigDialogViewer(
            IManagersGetter _Managers,
            IUITicker _Ticker,
            IViewInput _Input,
            IColorProvider _ColorProvider)
        {
            Managers = _Managers;
            Ticker = _Ticker;
            Input = _Input;
            ColorProvider = _ColorProvider;
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
            m_CloseButtonAnim = go.GetCompItem<Animator>("buttons_animator");

            var borderColor = ColorProvider.GetColor(ColorIds.UiBorderDefault);
            m_CloseButton.GetCompItem<Image>("border").color = borderColor;
            m_CloseButton.GetCompItem<Image>("icon").color = borderColor;
            
            m_CloseButton.SetOnClick(() =>
            {
                Managers.SoundManager.PlayClip(AudioClipNames.UIButtonClick);
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
            Input.RaiseCommand(InputCommands.UnPauseLevel, null, true);
        }
        
        public virtual void UpdateTick()
        {
            if (!ProposalDialogViewer.IsShowing && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
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
                        // ReSharper disable once SuspiciousTypeConversion.Global
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
                // ReSharper disable once SuspiciousTypeConversion.Global
                select item as MonoBehaviour)
            {
                Object.Destroy(monobeh);
            }
        }
        
        private void SetCloseButtonsState(bool _CloseAll)
        {
            if (m_CloseButtonAnim.IsNull())
                return;
            m_CloseButtonAnim.SetTrigger(_CloseAll ? AkDisableCloseButton : AkEnableCloseButton);
            m_CloseButtonAnim.SetInteger(AkState, _CloseAll ? AkStateCloseButtonDisabled : AkStateCloseButtonEnabled);
        }

        #endregion
    }
}