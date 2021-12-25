using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItems;
using Lean.Common;
using Ticker;
using UI;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using Object = UnityEngine.Object;

namespace DialogViewers
{
    public interface IBigDialogViewer : IDialogViewerBase
    {
        void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
        void CloseAll();
    }
    
    public class BigDialogViewer : DialogViewerBase, IBigDialogViewer, IAction, IUpdateTick
    {
        #region types

        private class GraphicAlphas
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

        private const int   AkStateCloseButtonDisabled = 0;
        private const int   AkStateCloseButtonEnabled  = 1;
        private const float TransitionTime             = 0.2f;

        #endregion
        
        #region nonpublic members

        private static int AkEnableCloseButton  => AnimKeys.Anim2;
        private static int AkDisableCloseButton => AnimKeys.Stop2;
        private static int AkState              => AnimKeys.State;
        
        private Button        m_CloseButton;
        private Animator      m_CloseButtonAnim;
        private Image         m_Background;
        private RectTransform m_DialogContainer;
        
        private readonly Stack<IDialogPanel> PanelStack = new Stack<IDialogPanel>();
        private readonly Dictionary<int, GraphicAlphas> GraphicsAlphas = 
            new Dictionary<int, GraphicAlphas>();

        #endregion

        #region inject

        private IManagersGetter             Managers          { get; }
        private IUITicker                   Ticker            { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IColorProvider              ColorProvider     { get; }
        private ICameraProvider             CameraProvider    { get; }
        private IPrefabSetManager           PrefabSetManager  { get; }

        public BigDialogViewer(
            IManagersGetter _Managers,
            IUITicker _Ticker,
            IViewInputCommandsProceeder _CommandsProceeder,
            IColorProvider _ColorProvider,
            ICameraProvider _CameraProvider,
            IPrefabSetManager _PrefabSetManager)
        {
            Managers = _Managers;
            Ticker = _Ticker;
            CommandsProceeder = _CommandsProceeder;
            ColorProvider = _ColorProvider;
            CameraProvider = _CameraProvider;
            PrefabSetManager = _PrefabSetManager;
            _Ticker.Register(this);
        }
        
        #endregion

        #region api

        public override RectTransform Container                   => m_DialogContainer;
        public          UnityAction   Action                      { get; set; }

        public override void Init(RectTransform _Parent)
        {
            var go = PrefabSetManager.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "dialog_viewer");
            
            m_Background = go.GetCompItem<Image>("background");
            m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
            m_CloseButton = go.GetCompItem<Button>("close_button");
            m_CloseButtonAnim = go.GetCompItem<Animator>("buttons_animator");
            
            m_CloseButton.RTransform().anchoredPosition = new Vector2(0f, 100f);

            var borderColor = ColorProvider.GetColor(ColorIds.UiBorder);
            m_CloseButton.GetCompItem<Image>("border").color = borderColor;
            m_CloseButton.GetCompItem<Image>("icon").color = borderColor;
            
            m_CloseButton.SetOnClick(() =>
            {
                Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
                CloseAll();
            });
        }

        public void Show(IDialogPanel _ItemTo, bool _HidePrevious = true)
        {
            CameraProvider.EnableTranslucentSource(true);
            CurrentPanel = _ItemTo;
            m_CloseButton.transform.SetAsLastSibling();
            ShowCore(_ItemTo, _HidePrevious, false);
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
                Object.Destroy(pan.PanelObject.gameObject);
            }
            
            PanelStack.Push(lastPanel);
            ShowCore(null, true, true);
            CommandsProceeder.RaiseCommand(EInputCommand.UnPauseLevel, null, true);
        }

        public virtual void UpdateTick()
        {
            if (IsOtherDialogViewersShowing != null && IsOtherDialogViewersShowing())
                return;
            if (LeanInput.GetDown(KeyCode.Space))
                CloseAll();
        }
        
        #endregion

        #region nonpublic methods

        private void ShowCore(
            IDialogPanel _PanelTo,
            bool _HidePrevious,
            bool _GoBack)
        {
            var panelFrom = !PanelStack.Any() ? null : PanelStack.Peek();
            if (panelFrom == null && _PanelTo == null)
                return;
            var panelFromObj = panelFrom?.PanelObject;
            var panelToObj = _PanelTo?.PanelObject;
            if (panelFrom != null && panelFromObj != null && _HidePrevious)
            {
                panelFrom.AppearingState = EAppearingState.Dissapearing;
                int instId = panelFromObj.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(panelFromObj));
                Coroutines.Run(DoTransparentTransition(
                    panelFromObj, GraphicsAlphas[instId].Alphas, TransitionTime,
                    Ticker,
                    true,
                    () =>
                    {
                        if (_PanelTo == null && (IsOtherDialogViewersShowing == null || !IsOtherDialogViewersShowing()))
                            CameraProvider.EnableTranslucentSource(false);
                        panelFrom.AppearingState = EAppearingState.Dissapeared;
                        if (!_GoBack)
                            return;
                        Object.Destroy(panelFromObj.gameObject);
                    }));
            }
            if (panelToObj != null)
            {
                CurrentPanel = _PanelTo;
                _PanelTo.AppearingState = EAppearingState.Appearing;
                int instId = panelToObj.GetInstanceID();
                if (!GraphicsAlphas.ContainsKey(instId))
                    GraphicsAlphas.Add(instId, new GraphicAlphas(panelToObj));
                Coroutines.Run(DoTransparentTransition(
                    panelToObj, GraphicsAlphas[instId].Alphas, TransitionTime,
                    Ticker,
                    false, 
                    () =>
                    {
                        _PanelTo.AppearingState = EAppearingState.Appeared;
                        m_Background.enabled = true;
                    }));
                _PanelTo.OnDialogEnable();
            }
            FinishShowing(panelFrom, _PanelTo, _GoBack, panelToObj);
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
                PanelStack.Clear();
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