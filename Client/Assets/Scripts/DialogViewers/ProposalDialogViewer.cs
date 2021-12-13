using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze;
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
    
    public class ProposalDialogViewer : IProposalDialogViewer
    {
        #region nonpublic members
        
        private static int AkShow => AnimKeys.Anim;
        private static int AkHide => AnimKeys.Stop;

        private RectTransform              m_DialogContainer;
        private Animator                   m_Animator;
        private TranslucentImage           m_Background;
        private IDialogPanel               m_Panel;
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

        public        RectTransform Container                   => m_DialogContainer;
        public        Func<bool>    IsOtherDialogViewersShowing { get; set; }
        public        bool          IsShowing                   { get; private set; }
        
        public void Init(RectTransform _Parent)
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
            CameraProvider.EnableTranslucentSource(true);
            m_Panel = _Item;
            var panel = m_Panel.Panel;
            m_Alphas = panel.GetComponentsInChildrenEx<Graphic>()
                .Distinct()
                .ToDictionary(_El => _El, _El => _El.color.a);
            Coroutines.Run(Coroutines.DoTransparentTransition(
                panel, 
                m_Alphas,
                0.2f,
                Ticker));
            m_Panel.OnDialogShow();
            m_Animator.SetTrigger(AkShow);
            IsShowing = true;
        }
        
        public void Back(UnityAction _OnFinish = null)
        {
            m_Animator.SetTrigger(AkHide);
            Coroutines.Run(Coroutines.DoTransparentTransition(
                m_Panel.Panel,
                m_Alphas,
                0.2f,
                Ticker,
                true,
                () =>
                {
                    IsShowing = false;
                    if (IsOtherDialogViewersShowing == null || !IsOtherDialogViewersShowing())
                        CameraProvider.EnableTranslucentSource(false);
                    m_Panel.OnDialogHide();
                    Object.Destroy(m_Panel.Panel.gameObject);
                    m_Panel = null;
                    _OnFinish?.Invoke();
                }));
        }

        #endregion
    }

    public class ProposalDialogViewerFake : IProposalDialogViewer
    {
        public RectTransform Container                   { get; }
        public bool          IsShowing                   { get; }
        public Func<bool>    IsOtherDialogViewersShowing { get; set; }

        public void Init(RectTransform _Parent)
        {
            throw new NotImplementedException();
        }


        public void Back(UnityAction _OnFinish = null)
        {
            throw new NotImplementedException();
        }

        public void Show(IDialogPanel _Item)
        {
            throw new NotImplementedException();
        }
    }
}