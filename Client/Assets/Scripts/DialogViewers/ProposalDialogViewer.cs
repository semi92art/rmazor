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
        
        private static int AkShow => AnimKeys.Anim;
        private static int AkHide => AnimKeys.Stop;

        private RectTransform              m_DialogContainer;
        private Animator                   m_Animator;
        private TranslucentImage           m_Background;
        private Dictionary<Graphic, float> m_Alphas;
        
        #endregion

        #region inject

        private ViewSettings      ViewSettings     { get; }
        private IUITicker         Ticker           { get; }
        private ICameraProvider   CameraProvider   { get; }
        private IPrefabSetManager PrefabSetManager { get; }

        public ProposalDialogViewer(
            ViewSettings _ViewSettings,
            IUITicker _Ticker,
            ICameraProvider _CameraProvider,
            IPrefabSetManager _PrefabSetManager)
        {
            ViewSettings = _ViewSettings;
            Ticker = _Ticker;
            CameraProvider = _CameraProvider;
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api

        public override RectTransform Container => m_DialogContainer;
        
        public override void Init(RectTransform _Parent)
        {
            var go = PrefabSetManager.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "notification_viewer");
            
            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
                m_Animator = go.GetCompItem<Animator>("animator");
                m_Background = go.GetCompItem<TranslucentImage>("background");
                m_Animator.speed = ViewSettings.ProposalDialogAnimSpeed;
                m_Animator.SetTrigger(AkHide);
            }));
        }

        public void Show(IDialogPanel _Item)
        {
            if (_Item == null)
                return;
            if (_Item is ICharacterDiedDialogPanel)
                Dbg.Log("Show CharacterDiedDialogPanel");
            
            CameraProvider.EnableTranslucentSource(true);
            CurrentPanel = _Item;
            var panel = CurrentPanel.PanelObject;
            m_Alphas = panel.GetComponentsInChildrenEx<Graphic>()
                .Distinct()
                .ToDictionary(_El => _El, _El => _El.color.a);
            Coroutines.Run(DoTransparentTransition(
                panel, 
                m_Alphas,
                0.2f,
                Ticker,
                false,
                () =>
                {
                    CurrentPanel.AppearingState = EAppearingState.Appeared;
                }));
            CurrentPanel.OnDialogShow();
            m_Animator.SetTrigger(AkShow);
            CurrentPanel.AppearingState = EAppearingState.Appearing;
        }
        
        public void Back(UnityAction _OnFinish = null)
        {
            var panel = CurrentPanel;
            m_Animator.SetTrigger(AkHide);
            Coroutines.Run(DoTransparentTransition(
                panel.PanelObject,
                m_Alphas,
                0.2f,
                Ticker,
                true,
                () =>
                {
                    panel.AppearingState = EAppearingState.Dissapeared;
                    if (IsOtherDialogViewersShowing == null || !IsOtherDialogViewersShowing())
                        CameraProvider.EnableTranslucentSource(false);
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