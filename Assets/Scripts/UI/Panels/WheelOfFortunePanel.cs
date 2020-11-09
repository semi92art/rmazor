using Extensions;
using Helpers;
using UI.Managers;
using UnityEngine;
using WheelController = MkeyFW.WheelController;

namespace UI.Panels
{
    public class WheelOfFortunePanel : IDialogPanel
    {
        #region private members

        private readonly IDialogViewer m_DialogViewer;
        private GameObject m_Wheel;
        private WheelController m_WheelCollider;
        
        #endregion
        
        #region api

        public UiCategory Category => UiCategory.WheelOfFortune;
        public RectTransform Panel { get; private set; }

        public WheelOfFortunePanel(IDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
            UiManager.Instance.OnCurrentCategoryChanged += (_Prev, _New) =>
            {
                if (_New != Category)
                    OnPanelClose();
            };
        }
        
        public void Show()
        {
            InstantiateWheel();
            Panel = Create();
            m_DialogViewer.Show(this);
        }

        private RectTransform Create()
        {
            var go = new GameObject("Wheel Of Fortune Fake Panel");
            var rTr = go.AddComponent<RectTransform>();
            rTr.SetParent(m_DialogViewer.DialogContainer);
            rTr.Set(RtrLites.FullFill);
            return rTr;
        }

        private void InstantiateWheel()
        {
            m_Wheel = PrefabInitializer.InitPrefab(
                null,
                "wheel_of_fortune",
                "wheel_main");
            m_WheelCollider = m_Wheel.GetCompItem<WheelController>("wheel_controller");
            m_WheelCollider.Init(m_DialogViewer);
        }

        private void OnPanelClose()
        {
            IActionExecuter actionExecuter = (IActionExecuter) m_DialogViewer;
            actionExecuter.Action = () =>
            {
                if (m_Wheel != null && UiManager.Instance.CurrentCategory == UiCategory.MainMenu)
                    Object.Destroy(m_Wheel);    
            };
        }

        #endregion
        
        
    }
}