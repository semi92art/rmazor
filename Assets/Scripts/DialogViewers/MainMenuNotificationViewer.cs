using System.Collections.Generic;
using System.Linq;
using Constants;
using Extensions;
using GameHelpers;
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
        void Show(IDialogPanel _Item);
    }
    
    public class MainMenuNotificationViewer : MonoBehaviour, INotificationViewer
    {

        #region serialized fields
        
        [SerializeField] private RectTransform container;
        [SerializeField] private Animator animator;
        
        #endregion

        #region nonpublic members

        private static int AkShow => AnimKeys.Anim;
        private static int AkHide => AnimKeys.Stop;
        private IDialogPanel m_Panel;
        private Dictionary<Graphic, float> m_Alphas;

        #endregion

        #region api

        public static bool IsShowing { get; private set; }
        public RectTransform Container => container;
        
        public static INotificationViewer Create(RectTransform _Parent)
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "main_menu_notification_viewer");
            var result = go.GetComponent<MainMenuNotificationViewer>();
            return result;
        }
        
        public void Show(IDialogPanel _Item)
        {
            m_Panel = _Item;
            var panel = m_Panel.Panel;
            m_Alphas = panel.GetComponentsInChildrenEx<Graphic>()
                .Distinct()
                .ToDictionary(_El => _El, _El => _El.color.a);
            Coroutines.Run(Coroutines.DoTransparentTransition(panel, m_Alphas, 0.2f));
            m_Panel.OnDialogShow();
            animator.SetTrigger(AkShow);
            IsShowing = true;
        }
        public void Back()
        {
            animator.SetTrigger(AkHide);
            
            Coroutines.Run(Coroutines.DoTransparentTransition(
                m_Panel.Panel,
                m_Alphas,
                0.2f,
                true,
                () =>
                {
                    m_Panel.OnDialogHide();
                    Destroy(m_Panel.Panel.gameObject);
                    m_Panel = null;
                }));
            IsShowing = false;
        }
        
        #endregion

        #region engine methods

        private void Update()
        {
            if (!IsShowing)
                return;
            if (Input.GetKeyDown(KeyCode.Escape))
                Back();
        }

        #endregion
    }
}