using System.Collections;
using System.Collections.Generic;
using Constants;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using Lean.Localization;
using Managers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using WheelController = MkeyFW.WheelController;

namespace UI.Panels
{
    public class WheelOfFortunePanel : GameObservable, IMenuDialogPanel
    {
        #region notify messages

        public const string NotifyMessageWatchAdButtonClick = nameof(NotifyMessageWatchAdButtonClick);
        public const string NotifyMessageSpinButtonClick = nameof(NotifyMessageSpinButtonClick);
        
        #endregion
        
        #region private members

        private readonly IMenuDialogViewer m_DialogViewer;
        private GameObject m_Wheel;
        private WheelController m_WheelController;
        private Button m_SpinButton;
        private TextMeshProUGUI m_Title;
        private Image m_WatchAdImage;
        private GameObject m_WatchAdBackground;
        private bool m_IsLocked;
        
        #endregion
        
        #region api

        public MenuUiCategory Category => MenuUiCategory.WheelOfFortune;
        public RectTransform Panel { get; private set; }

        public WheelOfFortunePanel(
            IMenuDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
            UiManager.Instance.OnCurrentMenuCategoryChanged += (_Prev, _New) =>
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

        public void OnEnable() { }

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
            
            m_Title = button.GetCompItem<TextMeshProUGUI>("title");
            m_WatchAdImage = button.GetCompItem<Image>("watch_ad_image");
            m_WatchAdBackground = button.GetContentItem("watch_ad_background");
            
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
            var cameraBounds = GameUtils.GetVisibleBounds();
            var tr = background.transform;
            var lScale = tr.localScale;
            var bounds = background.bounds;
            tr.localScale = new Vector3(
                cameraBounds.size.x * 2f * lScale.x / bounds.size.x,
                cameraBounds.size.y * 2f * lScale.y / bounds.size.y);
            tr.position = tr.position.SetY(cameraBounds.center.y);
            

            m_IsLocked = CheckIfWofSpinToday();
            m_SpinButton.SetOnClick(StartSpinOrWatchAd);
            m_WheelController.Init(m_DialogViewer);
        }

        private void StartSpinOrWatchAd()
        {
            if (!m_IsLocked)
            {
                Notify(this, NotifyMessageSpinButtonClick);
                Coroutines.Run(Coroutines.Action(() => m_WheelController.StartSpin()));
                SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, System.DateTime.Now.Date);
            }
            else
            {
                Notify(this, NotifyMessageWatchAdButtonClick, (UnityAction)WatchAdFinishAction);
            }
        }

        private void WatchAdFinishAction()
        {
            SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, System.DateTime.Now.Date.AddDays(-1));
            m_IsLocked = CheckIfWofSpinToday();
        }
        
        private bool CheckIfWofSpinToday()
        {
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.WheelOfFortuneLastDate);
            bool yes = lastDate == System.DateTime.Now.Date;
            m_Title.text = yes ? LeanLocalization.GetTranslationText("WatchAd") : LeanLocalization.GetTranslationText("Spin");
            m_Title.alignment = yes ? TextAlignmentOptions.Right : TextAlignmentOptions.Center;
            m_WatchAdImage.enabled = yes;
            m_WatchAdBackground.SetActive(yes);
            return yes;
        }

        private void OnPanelClose()
        {
            IActionExecuter actionExecuter = (IActionExecuter) m_DialogViewer;
            actionExecuter.Action = () =>
            {
                if (m_Wheel != null && UiManager.Instance.CurrentMenuCategory == MenuUiCategory.MainMenu)
                    Object.Destroy(m_Wheel);    
            };
        }

        #endregion
        
        
    }
}