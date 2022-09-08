using System;
using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities.UI;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable ClassNeverInstantiated.Global
namespace Common.UI
{
    public interface IDialogViewerMedium : IDialogViewer { }
    
    public class DialogViewerMediumFake : InitBase, IDialogViewerMedium
    {
        public IDialogPanel  CurrentPanel                => null;
        public RectTransform Container                   => null;
        public Func<bool>    OtherDialogViewersShowing   { get; set; }
        
        public void Back(UnityAction  _OnFinish                       = null)                         { }
        public void Show(IDialogPanel _PanelTo, float _AnimationSpeed = 1, bool _HidePrevious = true) { }
    }
    
    
    public class DialogViewerMedium : DialogViewerBase, IDialogViewerMedium
    {
        #region nonpublic members
        
        private RectTransform              m_DialogContainer;
        private Dictionary<Graphic, float> m_Alphas;

        #endregion

        #region inject
        
        private DialogViewerMedium(
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
                "notification_viewer");
            Cor.Run(Cor.WaitNextFrame(() =>
            {
                m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
                base.Init();
            }));
        }

        public override void Show(IDialogPanel _PanelTo, float _Speed = 1f, bool _HidePrevious = true)
        {
            if (_PanelTo == null)
                return;
            if (!_PanelTo.AllowMultiple 
                && CurrentPanel != null 
                && CurrentPanel.GetType() == _PanelTo.GetType()
                && CurrentPanel.AppearingState == EAppearingState.Appeared)
                return;
            CameraProvider.EnableEffect(ECameraEffect.DepthOfField, true);
            base.Show(_PanelTo, _Speed, _HidePrevious);
            var panel = CurrentPanel.PanelRectTransform;
            m_Alphas = panel.GetComponentsInChildrenEx<Graphic>()
                .Distinct()
                .ToDictionary(_El => _El, _El => _El.color.a);
            var canvas = CanvasGetter.GetCanvas();
            if (!canvas.enabled)
                canvas.enabled = true;
            Cor.Run(DoTransparentTransition(
                panel, 
                m_Alphas,
                0.2f,
                false,
                () =>
                {
                    CurrentPanel.AppearingState = EAppearingState.Appeared;
                }));
            CurrentPanel.AppearingState = EAppearingState.Appearing;
            if (_PanelTo.Animator.IsNull())
                return;
            _PanelTo.Animator.speed = _Speed;
            _PanelTo.Animator.SetTrigger(AnimKeys.Anim);
        }

        public override void Back(UnityAction _OnFinish = null)
        {
            var panel = PanelsStack.Pop();
            Cor.Run(DoTransparentTransition(
                panel.PanelRectTransform,
                m_Alphas,
                0.2f,
                true,
                () =>
                {
                    panel.AppearingState = EAppearingState.Dissapeared;
                    if (OtherDialogViewersShowing())
                        CameraProvider.EnableEffect(ECameraEffect.DepthOfField, false);
                    Object.Destroy(panel.PanelRectTransform.gameObject);
                    _OnFinish?.Invoke();
                    var canvas = CanvasGetter.GetCanvas();
                    if (CurrentPanel == null 
                        && canvas.enabled 
                        && !OtherDialogViewersShowing())
                    {
                        canvas.enabled = false;
                    }
                }));
            panel.AppearingState = EAppearingState.Dissapearing;
        }

        #endregion
    }
}