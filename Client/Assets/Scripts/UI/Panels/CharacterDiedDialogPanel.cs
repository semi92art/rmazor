using System.Collections;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.InputConfigurators;
using Ticker;
using TMPro;
using UI.Factories;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using Utils;

namespace UI.Panels
{
    public interface ICharacterDiedDialogPanel : IDialogPanel { }
    
    public class CharacterDiedDialogPanel : DialogPanelBase, ICharacterDiedDialogPanel
    {
        #region constants
        
        private const float  CountdownTime              = 5f;
        private const int    PayToContinueMoneyCount    = 50;

        #endregion
        
        #region nonpublic members
        
        private static int AkShow => AnimKeys.Anim;
        
        private Animator           m_Animator;
        private AnimationTriggerer m_Triggerer;
        private Image              m_MoneyIcon1;
        private Image              m_MoneyIcon2;
        private Image              m_IconWatchAds;
        private TextMeshProUGUI    m_TextYouHaveMoney;
        private TextMeshProUGUI    m_TextMoneyCount;
        private TextMeshProUGUI    m_TextContinue;
        private TextMeshProUGUI    m_TextPayMoneyCount;
        private TextMeshProUGUI    m_TextNotEnoughMoney;
        private Button             m_ButtonWatchAds;
        private Button             m_ButtonPayMoney;
        private Animator           m_AnimLoadingAds;
        private Animator           m_AnimLoadingMoneyCount;
        private Animator           m_AnimLoadingMoneyCount2;
        private ProceduralImage    m_RoundFilledBorder;
        private bool               m_AdsWatched;
        private bool               m_MoneyPayed;
        private int                m_MoneyCount;

        #endregion

        #region inject

        private ViewSettings           ViewSettings         { get; }
        private IProposalDialogViewer  ProposalDialogViewer { get; }
        private IViewInputConfigurator InputConfigurator    { get; }

        public CharacterDiedDialogPanel(
            ViewSettings _ViewSettings,
            IBigDialogViewer _DialogViewer,
            IProposalDialogViewer _ProposalDialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider,
            IViewInputConfigurator _InputConfigurator) 
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            ViewSettings = _ViewSettings;
            ProposalDialogViewer = _ProposalDialogViewer;
            InputConfigurator = _InputConfigurator;
        }
        
        #endregion
        
        #region api
        
        public override EUiCategory Category => EUiCategory.CharacterDied;

        public override void Init()
        {
            base.Init();
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    ProposalDialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "character_died_panel");
            Panel = go.RTransform();
            go.SetActive(false);

            m_Animator                = go.GetCompItem<Animator>("animator");
            m_Triggerer               = go.GetCompItem<AnimationTriggerer>("triggerer");
            m_ButtonWatchAds          = go.GetCompItem<Button>("watch_ads_button");
            m_ButtonPayMoney          = go.GetCompItem<Button>("pay_money_button");
            m_TextYouHaveMoney        = go.GetCompItem<TextMeshProUGUI>("you_have_text");
            m_TextMoneyCount          = go.GetCompItem<TextMeshProUGUI>("money_count_text");
            m_TextPayMoneyCount       = go.GetCompItem<TextMeshProUGUI>("pay_money_count_text");
            m_TextContinue            = go.GetCompItem<TextMeshProUGUI>("continue_text");
            m_TextNotEnoughMoney      = go.GetCompItem<TextMeshProUGUI>("not_enough_money_text");
            m_AnimLoadingAds          = go.GetCompItem<Animator>("loading_ads_anim");
            m_AnimLoadingMoneyCount   = go.GetCompItem<Animator>("loading_money_count_anim");
            m_AnimLoadingMoneyCount2  = go.GetCompItem<Animator>("loading_money_count_anim_2");
            m_MoneyIcon1              = go.GetCompItem<Image>("money_icon_1");
            m_MoneyIcon2              = go.GetCompItem<Image>("money_icon_2");
            m_IconWatchAds            = go.GetCompItem<Image>("watch_ads_icon");
            m_RoundFilledBorder       = go.GetCompItem<ProceduralImage>("round_filled_border");
            m_Triggerer.Trigger1      = () => Coroutines.Run(StartCountdown());
            m_TextPayMoneyCount.text  = PayToContinueMoneyCount.ToString();
            m_TextYouHaveMoney.text   = Managers.LocalizationManager.GetTranslation("you_have") + ":";
            m_TextContinue.text       = Managers.LocalizationManager.GetTranslation("continue");
            m_TextNotEnoughMoney.text = Managers.LocalizationManager.GetTranslation("not_enough_money");
            var moneyIconSprite = PrefabUtilsEx.GetObject<Sprite>(
                "shop_items", "shop_money_icon");
            m_MoneyIcon1.sprite = moneyIconSprite;
            m_MoneyIcon2.sprite = moneyIconSprite;
            m_ButtonWatchAds.onClick.AddListener(OnWatchAdsButtonClick);
            m_ButtonPayMoney.onClick.AddListener(OnPayMoneyButtonClick);

            m_AdsWatched = false;
            m_MoneyPayed = false;
        }

        public override void OnDialogShow()
        {
            m_Animator.speed = ViewSettings.ProposalDialogAnimSpeed;
            m_Animator.SetTrigger(AkShow);
            IndicateAdsLoading(true);
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Managers.AdsManager.RewardedAdReady,
                () => IndicateAdsLoading(false)));

            var moneyCountEntity = Managers.ScoreManager.GetScore(DataFieldIds.Money);
            IndicateMoneyCountLoading(true);
            Coroutines.Run(Coroutines.WaitWhile(
                () => !moneyCountEntity.Loaded,
                () =>
                {
                    var moneyCount = moneyCountEntity.GetFirstScore();
                    if (!moneyCount.HasValue)
                        return;
                    m_MoneyCount = moneyCount.Value;
                    m_TextMoneyCount.text = moneyCount.Value.ToString();
                    IndicateMoneyCountLoading(false, moneyCount.Value >= PayToContinueMoneyCount);
                }));
        }

        #endregion
        
        #region nonpublic methods

        private void IndicateAdsLoading(bool _Indicate)
        {
            m_AnimLoadingAds.SetGoActive(_Indicate);
            m_IconWatchAds.enabled = !_Indicate;
            m_ButtonWatchAds.interactable = !_Indicate;
        }

        private void IndicateMoneyCountLoading(bool _Indicate, bool _IsMoneyEnough = true)
        {
            m_AnimLoadingMoneyCount.SetGoActive(_Indicate);
            m_AnimLoadingMoneyCount2.SetGoActive(_Indicate);
            m_MoneyIcon2.enabled = !_Indicate;
            m_TextMoneyCount.enabled = !_Indicate;
            if (_Indicate)
            {
                m_TextNotEnoughMoney.enabled = false;
                m_TextPayMoneyCount.enabled = false;
                m_MoneyIcon1.enabled = false;
                m_ButtonPayMoney.interactable = false;
            }
            else
            {
                m_TextNotEnoughMoney.enabled = !_IsMoneyEnough;
                m_TextPayMoneyCount.enabled = _IsMoneyEnough;
                m_MoneyIcon1.enabled = _IsMoneyEnough;
                m_ButtonPayMoney.interactable = _IsMoneyEnough;
            }
        }

        private void OnWatchAdsButtonClick()
        {
#if UNITY_EDITOR
            m_AdsWatched = true;
#else
            Managers.AdsManager.ShowRewardedAd(() => m_AdsWatched = true);
#endif
        }

        private void OnPayMoneyButtonClick()
        {
            Managers.ScoreManager.SetScore(DataFieldIds.Money, m_MoneyCount - PayToContinueMoneyCount);
            m_MoneyPayed = true;
        }

        private IEnumerator StartCountdown()
        {
            yield return Coroutines.Lerp(
                1f,
                0f,
                CountdownTime,
                _Progress => m_RoundFilledBorder.fillAmount = _Progress,
                Ticker,
                (_Breaked, _) =>
                {
                    void RaiseContinueCommand()
                    {
                        InputConfigurator.RaiseCommand(
                            InputCommands.ReadyToStartLevel,
                            null, 
                            true);
                    }
                    void RaiseLoadFirstLevelInGroupCommand()
                    {
                        InputConfigurator.RaiseCommand(
                            InputCommands.ReadyToUnloadLevel,
                            new object[] { CommonInputCommandArgs.LoadFirstLevelFromGroupArg }, 
                            true);
                    }
                    ProposalDialogViewer.Back(_Breaked ?
                        (UnityAction)RaiseContinueCommand : RaiseLoadFirstLevelInGroupCommand);
                },
                () => m_AdsWatched || m_MoneyPayed);
        }
        
        #endregion
    }
}