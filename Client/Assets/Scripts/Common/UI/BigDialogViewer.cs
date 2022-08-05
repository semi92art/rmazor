using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using Lean.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Common.UI
{
    public interface IFullscreenDialogViewer : IDialogViewer
    {
        UnityAction OnClosed { get; set; }
    }

    public class FullscreenDialogViewerFake : InitBase, IFullscreenDialogViewer
    {
        public IDialogPanel      CurrentPanel              => null;
        public RectTransform     Container                 => null;
        public System.Func<bool> OtherDialogViewersShowing { get; set; }
        public UnityAction       OnClosed                  { get; set; }

        public void Back(UnityAction   _OnFinish = null) { }

        public void Show(IDialogPanel _PanelTo, float _AnimationSpeed = 1, bool _HidePrevious = true) { }
    }
    
    public class FullscreenDialogViewer : DialogViewerBase, IFullscreenDialogViewer, IAction, IUpdateTick
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
        private Image         m_CloseButtonBorder;
        private Image         m_CloseButtonIcon;
        private Animator      m_CloseButtonAnim;
        private RectTransform m_DialogContainer;
        
        private readonly Stack<IDialogPanel> m_PanelStack = new Stack<IDialogPanel>();
        private readonly Dictionary<int, GraphicAlphas> m_GraphicsAlphas = 
            new Dictionary<int, GraphicAlphas>();

        #endregion

        #region inject

        private IAudioManager       AudioManager  { get; }
        private IColorProvider      ColorProvider { get; }

        private FullscreenDialogViewer(
            IViewUICanvasGetter         _CanvasGetter,
            IAudioManager               _AudioManager,
            IUITicker                   _Ticker,
            IColorProvider              _ColorProvider,
            ICameraProvider             _CameraProvider,
            IPrefabSetManager           _PrefabSetManager)
            : base(
                _CanvasGetter,
                _CameraProvider,
                _Ticker, 
                _PrefabSetManager)
        {
            AudioManager = _AudioManager;
            ColorProvider = _ColorProvider;
            _Ticker.Register(this);
        }
        
        #endregion

        #region api

        public override RectTransform Container => m_DialogContainer;
        public          UnityAction   Action    { get; set; }
        public          UnityAction   OnClosed    { get; set; }

        public override void Init()
        {
            var parent = CanvasGetter.GetCanvas().RTransform();
            var go = PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    parent,
                    RectTransformLite.FullFill),
                "dialog_viewers",
                "dialog_viewer");
            m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
            m_CloseButton = go.GetCompItem<Button>("close_button");
            m_CloseButtonAnim = go.GetCompItem<Animator>("buttons_animator");
            m_CloseButton.RTransform().anchoredPosition = new Vector2(0f, 100f);
            m_CloseButtonBorder = m_CloseButton.GetCompItem<Image>("border");
            m_CloseButtonIcon = m_CloseButton.GetCompItem<Image>("icon");
            var borderColor = ColorProvider.GetColor(ColorIds.UiBorder);
            m_CloseButtonBorder.color = borderColor;
            m_CloseButtonIcon.color = borderColor;
            m_CloseButton.SetOnClick(() =>
            {
                AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
                CloseAll();
            });
            ColorProvider.ColorChanged += OnColorChanged;
            base.Init();
        }
        
        public override void Show(IDialogPanel _PanelTo, float _AnimationSpeed = 1f, bool _HidePrevious = true)
        {
            CameraProvider.EnableEffect(ECameraEffect.DepthOfField, true);
            CurrentPanel = _PanelTo;
            m_CloseButton.transform.SetAsLastSibling();
            ShowCore(_PanelTo, _AnimationSpeed, _HidePrevious);
        }
        
        public override void Back(UnityAction _OnFinish = null)
        {
            throw new System.NotSupportedException();
        }
        
        public virtual void UpdateTick()
        {
            if (!Initialized)
                return;
            if (OtherDialogViewersShowing())
                return;
            if (LeanInput.GetDown(KeyCode.Space))
                CloseAll();
        }
        
        #endregion

        #region nonpublic methods

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.UiBorder)
                return;
            m_CloseButtonBorder.color = _Color;
            m_CloseButtonIcon.color = _Color;
        }
        
        private void ShowCore(
            IDialogPanel _PanelTo,
            float        _AnimationSpeed = 1f,
            bool         _HidePrevious = false,
            bool         _GoBack = false)
        {
            var panelFrom = !m_PanelStack.Any() ? null : m_PanelStack.Peek();
            if (panelFrom == null && _PanelTo == FakePanel)
                return;
            var panelFromObj = panelFrom?.PanelObject;
            var panelToObj = _PanelTo.PanelObject;
            if (panelFrom != null && panelFromObj.IsNotNull() && _HidePrevious)
            {
                panelFrom.AppearingState = EAppearingState.Dissapearing;
                int instId = panelFromObj.GetInstanceID(); //-V3105
                if (!m_GraphicsAlphas.ContainsKey(instId))
                    m_GraphicsAlphas.Add(instId, new GraphicAlphas(panelFromObj));
                Cor.Run(DoTransparentTransition(
                    panelFromObj,
                    m_GraphicsAlphas[instId].Alphas,
                    TransitionTime / _AnimationSpeed,
                    true,
                    () =>
                    {
                        if (_PanelTo == FakePanel && OtherDialogViewersShowing())
                            CameraProvider.EnableEffect(ECameraEffect.DepthOfField, false);
                        panelFrom.AppearingState = EAppearingState.Dissapeared;
                        if (!_GoBack)
                            return;
                        Object.Destroy(panelFromObj.gameObject);
                        var canvas = CanvasGetter.GetCanvas();
                        if (canvas.enabled && !OtherDialogViewersShowing() && panelToObj.IsNull())
                            canvas.enabled = false; 
                    }));
            }
            if (panelToObj.IsNotNull())
            {
                CurrentPanel = _PanelTo;
                var canvas = CanvasGetter.GetCanvas();
                if (!canvas.enabled)
                    canvas.enabled = true;
                _PanelTo.AppearingState = EAppearingState.Appearing;
                int instId = panelToObj!.GetInstanceID();
                if (!m_GraphicsAlphas.ContainsKey(instId))
                    m_GraphicsAlphas.Add(instId, new GraphicAlphas(panelToObj));
                Cor.Run(DoTransparentTransition(
                    panelToObj,
                    m_GraphicsAlphas[instId].Alphas,
                    TransitionTime / _AnimationSpeed,
                    false, 
                    () =>
                    {
                        _PanelTo.AppearingState = EAppearingState.Appeared;
                    }));
            }
            FinishShowing(panelFrom, _PanelTo, _GoBack, panelToObj);
        }

        private void FinishShowing(
            IDialogPanel _ItemFrom,
            IDialogPanel _ItemTo,
            bool _GoBack,
            RectTransform _PanelTo)
        {
            ClearGraphicsAlphas();
            if (_PanelTo.IsNull())
                m_PanelStack.Clear();
            else
            {
                if (!m_PanelStack.Any())
                    m_PanelStack.Push(_ItemFrom);
                if (m_PanelStack.Any() && _GoBack)
                    m_PanelStack.Pop();
                if (!_GoBack)
                    m_PanelStack.Push(_ItemTo);
            }
            
            SetCloseButtonsState(_PanelTo == null);
        }
        
        private void CloseAll()
        {
            if (!m_PanelStack.Any())
                return;
            var lastPanel = m_PanelStack.Pop();
            var panelsToDestroy = new List<IDialogPanel>();
            while (m_PanelStack.Count > 0)
                panelsToDestroy.Add(m_PanelStack.Pop());
            foreach (var pan in panelsToDestroy
                .Where(_Panel => _Panel != null))
            {
                Object.Destroy(pan.PanelObject.gameObject);
            }
            m_PanelStack.Push(lastPanel);
            ShowCore(FakePanel, _HidePrevious: true, _GoBack: true);
            OnClosed?.Invoke();
        }
        
        private void ClearGraphicsAlphas()
        {
            foreach ((int key, var value) in m_GraphicsAlphas.ToArray())
            {
                if (value.Alphas.All(_A => _A.Key.IsNull()))
                    m_GraphicsAlphas.Remove(key);
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