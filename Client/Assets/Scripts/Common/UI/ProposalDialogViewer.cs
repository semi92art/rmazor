// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Entities.UI;
using Common.Enums;
using Common.Extensions;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Common.UI
{
    public interface IProposalDialogViewer : IDialogViewerBase
    {
        float AnimationSpeed { get; }
        void  Back(UnityAction  _OnFinish           = null);
        void  Show(IDialogPanel _Item, float _Speed = 1f);
    }
    
    public class ProposalDialogViewer : DialogViewerBase, IProposalDialogViewer
    {
        #region nonpublic members
        
        private RectTransform              m_DialogContainer;
        private Dictionary<Graphic, float> m_Alphas;
        
        #endregion

        #region inject
        
        private ProposalDialogViewer(
            IUITicker         _Ticker,
            ICameraProvider   _CameraProvider,
            IPrefabSetManager _PrefabSetManager)
            : base(_CameraProvider, _Ticker, _PrefabSetManager) { }

        #endregion

        #region api

        public override RectTransform Container      => m_DialogContainer;
        public          float         AnimationSpeed { get; private set; } = 1f;
        
        public override void Init(RectTransform _Parent)
        {
            var go = PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Parent,
                    RectTransformLite.FullFill),
                "dialog_viewers",
                "notification_viewer");
            
            Cor.Run(Cor.WaitEndOfFrame(() =>
            {
                m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
            }));
        }

        public void Show(IDialogPanel _Item, float _Speed = 1f)
        {
            if (_Item == null)
                return;
            if (!_Item.AllowMultiple 
                && CurrentPanel != null 
                && CurrentPanel.GetType() == _Item.GetType()
                && CurrentPanel.AppearingState == EAppearingState.Appeared)
                return;
            AnimationSpeed = _Speed;
            CameraProvider.EnableEffect(ECameraEffect.DepthOfField, true);
            CurrentPanel = _Item;
            var panel = CurrentPanel.PanelObject;
            m_Alphas = panel.GetComponentsInChildrenEx<Graphic>()
                .Distinct()
                .ToDictionary(_El => _El, _El => _El.color.a);
            Cor.Run(DoTransparentTransition(
                panel, 
                m_Alphas,
                0.2f,
                false,
                () =>
                {
                    CurrentPanel.AppearingState = EAppearingState.Appeared;
                }));
            CurrentPanel.OnDialogShow();
            CurrentPanel.AppearingState = EAppearingState.Appearing;
        }

        public void Back(UnityAction _OnFinish = null)
        {
            var panel = CurrentPanel;
            Cor.Run(DoTransparentTransition(
                panel.PanelObject,
                m_Alphas,
                0.2f,
                true,
                () =>
                {
                    panel.AppearingState = EAppearingState.Dissapeared;
                    if (IsOtherDialogViewersShowing == null || !IsOtherDialogViewersShowing())
                        CameraProvider.EnableEffect(ECameraEffect.DepthOfField, false);
                    panel.OnDialogHide();
                    Object.Destroy(panel.PanelObject.gameObject);
                    _OnFinish?.Invoke();
                }));
            panel.AppearingState = EAppearingState.Dissapearing;
        }

        #endregion
    }

    public class ProposalDialogViewerFake : IProposalDialogViewer
    {
        public IDialogPanel    CurrentPanel                => null;
        public RectTransform   Container                   => null;
        public Func<bool>      IsOtherDialogViewersShowing { get; set; }

        public void  Init(RectTransform _Parent)                   { }
        public float AnimationSpeed                                => 1f;
        public void  Back(UnityAction  _OnFinish           = null) { }
        public void  Show(IDialogPanel _Item, float _Speed = 1f)   { }
    }
}