using System;
using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Enums;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using GameHelpers;
using RMAZOR;
using UI;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DialogViewers
{
    public interface IProposalDialogViewer : IDialogViewerBase
    {
        void Back(UnityAction _OnFinish = null);
        void Show(IDialogPanel _Item, float _Speed = 1f);
    }
    
    public class ProposalDialogViewer : DialogViewerBase, IProposalDialogViewer
    {
        #region nonpublic members
        
        private RectTransform              m_DialogContainer;
        private Dictionary<Graphic, float> m_Alphas;
        
        #endregion

        #region inject

        private ViewSettings      ViewSettings     { get; }

        public ProposalDialogViewer(
            ViewSettings      _ViewSettings,
            IUITicker         _Ticker,
            ICameraProvider   _CameraProvider,
            IPrefabSetManager _PrefabSetManager)
            : base(_CameraProvider, _Ticker, _PrefabSetManager)
        {
            ViewSettings     = _ViewSettings;
        }

        #endregion

        #region api

        public override RectTransform Container => m_DialogContainer;
        
        public override void Init(RectTransform _Parent)
        {
            var go = PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
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
            ViewSettings.ProposalDialogAnimSpeed = _Speed;
            CameraProvider.DofEnabled = true;
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
                        CameraProvider.DofEnabled = false;
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

        public void Init(RectTransform _Parent)                 { }
        public void Back(UnityAction _OnFinish = null)          { }
        public void Show(IDialogPanel _Item, float _Speed = 1f) { }
    }
}