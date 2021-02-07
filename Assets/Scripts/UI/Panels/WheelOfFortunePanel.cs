using System.Collections;
using System.Collections.Generic;
using Constants;
using DialogViewers;
using Entities;
using Exceptions;
using Extensions;
using GameHelpers;
using Lean.Localization;
using Managers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using WheelController = MkeyFW.WheelController;

namespace UI.Panels
{
    public class WheelOfFortunePanel : DialogPanelBase, IMenuUiCategory
    {
        #region notify messages

        public const string NotifyMessageWatchAdButtonClick = nameof(NotifyMessageWatchAdButtonClick);
        public const string NotifyMessageSpinButtonClick = nameof(NotifyMessageSpinButtonClick);
        
        #endregion
        
        #region nonpublic members
        
        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly INotificationViewer m_NotificationViewer;
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

        public WheelOfFortunePanel(
            IMenuDialogViewer _DialogViewer, INotificationViewer _NotificationViewer)
        {
            m_DialogViewer = _DialogViewer;
            m_NotificationViewer = _NotificationViewer;
            UiManager.Instance.OnCurrentMenuCategoryChanged += (_Prev, _New) =>
            {
                if (_New != Category)
                    OnPanelClose();
            };
        }

        public override void Init()
        {
            GameObject wofPan = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommonStyleNames.MainMenuDialogPanels, "wof_panel");

            m_SpinButton = wofPan.GetCompItem<Button>("spin_button");
            m_Title = m_SpinButton.GetCompItem<TextMeshProUGUI>("title");
            m_WatchAdImage = m_SpinButton.GetCompItem<Image>("watch_ad_image");
            m_WatchAdBackground = m_SpinButton.GetContentItem("watch_ad_background");
            InstantiateWheel();
            Panel = wofPan.RTransform();
        }

        #endregion

        #region nonpublic methods
        
        private void InstantiateWheel()
        {
            m_Wheel = PrefabInitializer.InitPrefab(
                null,
                "wheel_of_fortune",
                "wheel_main");
            m_WheelController = m_Wheel.GetCompItem<WheelController>("wheel_controller");
            SpriteRenderer background = m_Wheel.GetCompItem<SpriteRenderer>("background");
            background.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiMainBackground);
            var cameraBounds = GameUtils.GetVisibleBounds();
            Transform tr = background.transform;
            Vector3 lScale = tr.localScale;
            Bounds bounds = background.bounds;
            tr.localScale = new Vector3(
                cameraBounds.size.x * lScale.x / bounds.size.x,
                cameraBounds.size.y * lScale.y / bounds.size.y);
            tr.position = tr.position.SetY(cameraBounds.center.y);
            Transform outer = m_Wheel.GetCompItem<Transform>("wheel");
            outer.position = outer.position.SetY(GraphicUtils.AspectRatio * 17.68f - 5.5f);
            float xyVal = GraphicUtils.AspectRatio * 15f / 11f + 491f / 1100f;
            outer.localScale = outer.localScale.SetXY(Vector2.one * xyVal);

            m_IsLocked = CheckIfWofSpinToday();
            m_SpinButton.SetOnClick(StartSpinOrWatchAd);
            m_WheelController.Init(m_DialogViewer, SpinFinishAction);
        }

        private void StartSpinOrWatchAd()
        {
            if (!m_IsLocked)
            {
                Notify(this, NotifyMessageSpinButtonClick);
                Coroutines.Run(Coroutines.Action(() => m_WheelController.StartSpin()));
                SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, System.DateTime.Now.Date);
                m_SpinButton.interactable = false;
            }
            else
            {
                Notify(this, NotifyMessageWatchAdButtonClick, (UnityAction)WatchAdFinishAction);
            }
        }

        private void SpinFinishAction(BankItemType _BankItemType, long _Reward)
        {
            var rewardPanel = new WheelOfFortuneRewardPanel(
                m_NotificationViewer, _BankItemType, _Reward, () =>
                    BankManager.Instance.PlusBankItems(_BankItemType, _Reward));
                    
            m_SpinButton.interactable = true;
            m_IsLocked = CheckIfWofSpinToday();
            rewardPanel.Init();
            m_NotificationViewer.Show(rewardPanel);
        }

        private void WatchAdFinishAction()
        {
            SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, System.DateTime.Now.Date.AddDays(-1));
            m_IsLocked = CheckIfWofSpinToday();
        }
        
        private bool CheckIfWofSpinToday()
        {
            System.DateTime lastDate = SaveUtils.GetValue<System.DateTime>(SaveKey.WheelOfFortuneLastDate);
            bool spinedToday = lastDate == System.DateTime.Now.Date;
            m_Title.text = spinedToday ? LeanLocalization.GetTranslationText("WatchAd") : LeanLocalization.GetTranslationText("Spin");
            m_Title.RTransform().Set(new RectTransformLite
            {
                Anchor = UiAnchor.Create(0, 0, 1, 1),
                AnchoredPosition = Vector2.right * (spinedToday ? 50.7f : 0f),
                Pivot = Vector2.one * 0.5f,
                SizeDelta = Vector2.right * (spinedToday ? -129.6f : -26.3f)
            });
            m_WatchAdImage.enabled = spinedToday;
            m_WatchAdBackground.SetActive(false);
            return spinedToday;
        }

        private void OnPanelClose()
        {
            IActionExecutor actionExecutor = (IActionExecutor) m_DialogViewer;
            actionExecutor.Action = () =>
            {
                if (m_Wheel != null && UiManager.Instance.CurrentMenuCategory == MenuUiCategory.MainMenu)
                    Object.Destroy(m_Wheel);    
            };
        }

        #endregion
    }
}