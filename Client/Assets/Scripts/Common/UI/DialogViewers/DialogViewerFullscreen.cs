using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Entities.UI;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using Lean.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Common.UI.DialogViewers
{
    public interface IDialogViewerFullscreen : IDialogViewer { }

    public class DialogViewerFullscreenFake : InitBase, IDialogViewerFullscreen
    {
        public IDialogPanel      CurrentPanel              => null;
        public RectTransform     Container                 => null;
        public System.Func<bool> OtherDialogViewersShowing { get; set; }

        public void Back(UnityAction   _OnFinish = null) { }

        public void Show(IDialogPanel _Panel, float _AnimationSpeed = 1, bool _HidePrevious = true) { }
    }
    
    public class DialogViewerFullscreen : DialogViewerBase, IDialogViewerFullscreen, IUpdateTick
    {
        #region types

        private class GraphicAlphas
        {
            public Dictionary<Graphic, float> Alphas { get; }

            public GraphicAlphas(RectTransform _Item)
            {
                Alphas = _Item.GetComponentsInChildrenEx<Graphic>()
                    .Distinct()
                    .ToDictionary(
                        _El => _El, 
                        _El => _El.color.a);
            }
        }

        #endregion

        #region constants

        private const float TransitionTime = 0.2f;

        #endregion
        
        #region nonpublic members
        
        private RectTransform m_DialogContainer;
        
        private readonly Stack<IDialogPanel> m_PanelStack = new Stack<IDialogPanel>();
        private readonly Dictionary<int, GraphicAlphas> m_GraphicsAlphas = 
            new Dictionary<int, GraphicAlphas>();

        #endregion

        #region inject
        
        private DialogViewerFullscreen(
            IViewUICanvasGetter _CanvasGetter,
            IUITicker           _Ticker,
            ICameraProvider     _CameraProvider,
            IPrefabSetManager   _PrefabSetManager)
            : base(
                _CanvasGetter,
                _CameraProvider,
                _Ticker, 
                _PrefabSetManager) { }
        
        #endregion

        #region api

        public override RectTransform Container => m_DialogContainer;

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
            base.Init();
        }
        
        public override void Show(IDialogPanel _Panel, float _AnimationSpeed = 1f, bool _HidePrevious = true)
        {
            CameraProvider.EnableEffect(ECameraEffect.DepthOfField, true);
            base.Show(_Panel, _AnimationSpeed, _HidePrevious);
            ShowCore(_Panel, _AnimationSpeed, _HidePrevious);
        }
        
        public override void Back(UnityAction _OnFinish = null)
        {
            CloseAll();
            _OnFinish?.Invoke();
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

        private void ShowCore(
            IDialogPanel _PanelTo,
            float        _AnimationSpeed = 1f,
            bool         _HidePrevious = false,
            bool         _GoBack = false)
        {
            var panelFrom = !m_PanelStack.Any() ? null : m_PanelStack.Peek();
            if (panelFrom == null && _PanelTo == FakePanel)
                return;
            var panelFromObj = panelFrom?.PanelRectTransform;
            var panelToObj = _PanelTo.PanelRectTransform;
            if (panelFrom != null && _HidePrevious)
            {
                panelFrom.AppearingState = EAppearingState.Dissapearing;
                int instId = panelFromObj.GetInstanceID(); //-V3105
                if (!m_GraphicsAlphas.ContainsKey(instId))
                    m_GraphicsAlphas.Add(instId, new GraphicAlphas(panelFromObj));
                Cor.Run(DoTransparentTransition(
                    panelFrom,
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
                // CurrentPanel = _PanelTo;
                var canvas = CanvasGetter.GetCanvas();
                if (!canvas.enabled)
                    canvas.enabled = true;
                _PanelTo.AppearingState = EAppearingState.Appearing;
                int instId = panelToObj!.GetInstanceID();
                if (!m_GraphicsAlphas.ContainsKey(instId))
                    m_GraphicsAlphas.Add(instId, new GraphicAlphas(panelToObj));
                Cor.Run(DoTransparentTransition(
                    _PanelTo,
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
        }
        
        private void CloseAll()
        {
            if (!m_PanelStack.Any())
                return;
            var lastPanel = m_PanelStack.Pop();
            var panelsToDeactivate = new List<IDialogPanel>();
            while (m_PanelStack.Count > 0)
                panelsToDeactivate.Add(m_PanelStack.Pop());
            foreach (var pan in panelsToDeactivate
                .Where(_Panel => _Panel != null))
            {
                Object.Destroy(pan.PanelRectTransform.SetGoActive(false));
            }
            m_PanelStack.Push(lastPanel);
            ShowCore(FakePanel, _HidePrevious: true, _GoBack: true);
        }
        
        private void ClearGraphicsAlphas()
        {
            foreach ((int key, var value) in m_GraphicsAlphas.ToArray())
            {
                if (value.Alphas.All(_A => _A.Key.IsNull()))
                    m_GraphicsAlphas.Remove(key);
            }
        }

        #endregion
    }
}