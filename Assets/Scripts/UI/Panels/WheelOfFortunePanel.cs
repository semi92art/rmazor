using Extensions;
using Helpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using WheelController = MkeyFW.WheelController;

namespace UI.Panels
{
    public class WheelOfFortunePanel : IDialogPanel
    {
        #region private members

        private readonly IDialogViewer m_DialogViewer;
        private GameObject m_Wheel;
        private WheelController m_WheelController;
        private Button m_SpinButton;
        
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
            Panel = Create();
            InstantiateWheel();
            m_DialogViewer.Show(this);
        }

        private RectTransform Create()
        {
            var go = new GameObject("Wheel Of Fortune Panel");

            var rTr = go.AddComponent<RectTransform>();
            rTr.SetParent(m_DialogViewer.DialogContainer);
            rTr.Set(RtrLites.FullFill);
            rTr.localScale = Vector3.one;
            
            var button = PrefabInitializer.InitUiPrefab(UiFactory.UiRectTransform(
                    rTr,
                    UiAnchor.Create(0, 0, 1, 0),
                    Vector2.up  * 35f,
                    Vector2.right * 0.5f,
                    new Vector2(-80f, 70f)),
                "wheel_of_fortune", "spin_button");
            m_SpinButton = button.GetComponent<Button>();
            
            return rTr;
        }

        private void InstantiateWheel()
        {
            m_Wheel = PrefabInitializer.InitPrefab(
                null,
                "wheel_of_fortune",
                "wheel_main");
            m_WheelController = m_Wheel.GetCompItem<WheelController>("wheel_controller");
            SpriteRenderer background = m_Wheel.GetCompItem<SpriteRenderer>("background");
            float width = background.bounds.size.x;
            float height = background.bounds.size.y;
            var cameraBounds = GameUtils.GetVisibleBounds();
            background.transform.localScale = new Vector3(
                cameraBounds.size.x * 2f / width,
                cameraBounds.size.y * 2f / height);
            
            m_SpinButton.SetOnClick(m_WheelController.StartSpin);
            
            m_WheelController.Init(m_DialogViewer);
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