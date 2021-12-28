using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze;
using Games.RazorMaze.Views.MazeItems;
using LeTai.Asset.TranslucentImage;
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
    public interface IProposalDialogViewer : IDialogViewerBase
    {
        void Back(UnityAction _OnFinish = null);
        void Show(IDialogPanel _Item);
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
            
            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
                Background = go.GetCompItem<TranslucentImage>("background");
                Background.source = CameraProvider.TranslucentSource;
                Background.enabled = false;
            }));
        }

        public void Show(IDialogPanel _Item)
        {
            if (_Item == null)
                return;
            if (!_Item.AllowMultiple && CurrentPanel != null && CurrentPanel.GetType() == _Item.GetType())
                return;
            CameraProvider.TranslucentSource.enabled = true;
            CurrentPanel = _Item;
            var panel = CurrentPanel.PanelObject;
            m_Alphas = panel.GetComponentsInChildrenEx<Graphic>()
                .Distinct()
                .ToDictionary(_El => _El, _El => _El.color.a);
            Coroutines.Run(DoTransparentTransition(
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
            Coroutines.Run(DoTransparentTransition(
                panel.PanelObject,
                m_Alphas,
                0.2f,
                true,
                () =>
                {
                    panel.AppearingState = EAppearingState.Dissapeared;
                    if (IsOtherDialogViewersShowing == null || !IsOtherDialogViewersShowing())
                        CameraProvider.TranslucentSource.enabled = false;
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

        public void Init(RectTransform _Parent)        { }
        public void Back(UnityAction _OnFinish = null) { }
        public void Show(IDialogPanel _Item)           { }
    }
}