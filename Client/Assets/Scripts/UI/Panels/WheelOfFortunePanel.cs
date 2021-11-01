using System;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.Common;
using Lean.Localization;
using Managers;
using MkeyFW;
using Ticker;
using TMPro;
using UI.Entities;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public interface IWheelOfFortuneDialogPanel : IDialogPanel
    {
    }
    
    public class WheelOfFortunePanel : DialogPanelBase, IWheelOfFortuneDialogPanel
    {
        #region notify messages

        public const string NotifyMessageWatchAdButtonClick = nameof(NotifyMessageWatchAdButtonClick);
        public const string NotifyMessageSpinButtonClick = nameof(NotifyMessageSpinButtonClick);
        
        #endregion
        
        #region nonpublic members
        
        private GameObject m_Wheel;
        private WheelController m_WheelController;
        private Button m_SpinButton;
        private TextMeshProUGUI m_Title;
        private Image m_WatchAdImage;
        private GameObject m_WatchAdBackground;
        private bool m_IsLocked;
        
        #endregion

        #region inject

        private IWheelOfFortuneRewardPanel RewardPanel { get; }
        private IProposalDialogViewer ProposalDialogViewer { get; }

        public WheelOfFortunePanel(
            IWheelOfFortuneRewardPanel _RewardPanel,
            IBigDialogViewer _DialogViewer,
            IProposalDialogViewer _ProposalDialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            RewardPanel = _RewardPanel;
            ProposalDialogViewer = _ProposalDialogViewer;
        }

        #endregion
        
        #region api

        public override EUiCategory Category => EUiCategory.WheelOfFortune;



        public override void Init()
        {
            // UiManager.Instance.OnCurrentMenuCategoryChanged += (_Prev, _New) =>
            // {
            //     if (_New != Category)
            //         OnPanelClose();
            // };
            
            base.Init();
            GameObject wofPan = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "wof_panel");

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
            m_Wheel = PrefabUtilsEx.InitPrefab(
                null,
                "wheel_of_fortune",
                "wheel_main");
            m_WheelController = m_Wheel.GetCompItem<WheelController>("wheel_controller");
            SpriteRenderer background = m_Wheel.GetCompItem<SpriteRenderer>("background");
            // background.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiMainBackground);
            var cameraBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
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
            m_WheelController.Init(DialogViewer, SpinFinishAction);
        }

        private void StartSpinOrWatchAd()
        {
            if (!m_IsLocked)
            {
                UIUtils.OnButtonClick(Managers, NotifyMessageSpinButtonClick);
                Coroutines.Run(Coroutines.Action(() => m_WheelController.StartSpin()));
                SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, DateTime.Now.Date);
                m_SpinButton.interactable = false;
            }
            else
            {
                UIUtils.OnButtonClick(Managers, NotifyMessageWatchAdButtonClick);
                Managers.Notify(_OnAdsManager: _AM => _AM.ShowRewardedAd(WatchAdFinishAction));
            }
        }

        private void SpinFinishAction(long _Reward)
        {
            m_SpinButton.interactable = true;
            m_IsLocked = CheckIfWofSpinToday();
            RewardPanel.PreInit(
                _Reward, 
                () => { });
            RewardPanel.Init();
            ProposalDialogViewer.Show(RewardPanel);
        }

        private void WatchAdFinishAction()
        {
            SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, DateTime.Now.Date.AddDays(-1));
            m_IsLocked = CheckIfWofSpinToday();
        }
        
        private bool CheckIfWofSpinToday()
        {
            DateTime lastDate = SaveUtils.GetValue<DateTime>(SaveKey.WheelOfFortuneLastDate);
            bool spinedToday = lastDate == DateTime.Now.Date;
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

        // private void OnPanelClose()
        // {
        //     IAction action = (IAction) DialogViewer;
        //     action.Action = () =>
        //     {
        //         if (m_Wheel != null && UiManager.Instance.CurrentCategory == EUiCategory.MainMenu)
        //             Object.Destroy(m_Wheel);    
        //     };
        // }

        #endregion
    }
}