using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Ticker;
using UI;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace DialogViewers
{
    public interface INotificationViewer : IDialogViewerBase
    {
        void Back();
        void Show(IDialogPanel _Item);
    }
    
    public class NotificationViewer : INotificationViewer, IUpdateTick
    {
        #region nonpublic members
        
        private RectTransform m_DialogContainer;
        private Animator m_Animator;

        private static int AkShow => AnimKeys.Anim;
        private static int AkHide => AnimKeys.Stop;
        private IDialogPanel m_Panel;
        private Dictionary<Graphic, float> m_Alphas;

        #endregion

        #region inject
        
        private IUITicker Ticker { get; }

        public NotificationViewer(IUITicker _Ticker)
        {
            Ticker = _Ticker;
            _Ticker.Register(this);
        }

        #endregion

        #region api

        public RectTransform Container => m_DialogContainer;
        public static bool IsShowing { get; private set; }
        
        public void Init(RectTransform _Parent)
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "notification_viewer");
            
            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                m_DialogContainer = go.GetCompItem<RectTransform>("dialog_container");
                m_Animator = go.GetCompItem<Animator>("animator");
            }));
        }

        public void Show(IDialogPanel _Item)
        {
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
        public void Back()
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
                    m_Panel.OnDialogHide();
                    Object.Destroy(m_Panel.Panel.gameObject);
                    m_Panel = null;
                }));
            IsShowing = false;
        }
        
        public void UpdateTick()
        {
            if (!IsShowing)
                return;
            if (Input.GetKeyDown(KeyCode.Escape))
                Back();
        }
        
        #endregion
    }
}