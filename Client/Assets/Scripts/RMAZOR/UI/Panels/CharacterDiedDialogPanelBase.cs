using System.Collections;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface ICharacterDiedDialogPanel : IDialogPanel { }
    
    public abstract class CharacterDiedDialogPanelBase : DialogPanelBase, ICharacterDiedDialogPanel
    {
        #region constants
        
        private const float  CountdownTime              = 5f;

        #endregion
        
        #region nonpublic members
        
        protected abstract string PrefabName { get; }
        
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
        private Image              m_RoundFilledBorder;
        private bool               m_AdsWatched;
        private bool               m_MoneyPayed;
        private long               m_MoneyCount;
        private bool               m_PanelShowing;

        #endregion

        #region inject

        private CommonGameSettings          CommonGameSettings   { get; }
        private IModelGame                  Model                { get; }
        private IProposalDialogViewer       ProposalDialogViewer { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }

        protected CharacterDiedDialogPanelBase(
            CommonGameSettings          _CommonGameSettings,
            IModelGame                  _Model,
            IBigDialogViewer            _DialogViewer,
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IProposalDialogViewer       _ProposalDialogViewer,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            CommonGameSettings   = _CommonGameSettings;
            Model                = _Model;
            ProposalDialogViewer = _ProposalDialogViewer;
            CommandsProceeder    = _CommandsProceeder;
        }
        
        #endregion
        
        #region api
        
        public override EUiCategory Category      => EUiCategory.CharacterDied;
        public override bool        AllowMultiple => false;

        public override void LoadPanel()
        {
            base.LoadPanel();
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    ProposalDialogViewer.Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, PrefabName);
            PanelObject = go.RTransform();
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
            m_MoneyIcon1              = go.GetCompItem<Image>("money_icon_1");
            m_MoneyIcon2              = go.GetCompItem<Image>("money_icon_2");
            m_IconWatchAds            = go.GetCompItem<Image>("watch_ads_icon");
            m_RoundFilledBorder       = go.GetCompItem<Image>("round_filled_border");
            m_Triggerer.Trigger1      = () => Cor.Run(StartCountdown());
            m_TextPayMoneyCount.text  = CommonGameSettings.payToContinueMoneyCount.ToString();
            m_TextYouHaveMoney.text   = Managers.LocalizationManager.GetTranslation("you_have") + ":";
            m_TextContinue.text       = Managers.LocalizationManager.GetTranslation("continue");
            m_TextNotEnoughMoney.text = Managers.LocalizationManager.GetTranslation("not_enough_money");
            var moneyIconSprite = Managers.PrefabSetManager.GetObject<Sprite>(
                "shop_items", "shop_money_icon");
            m_MoneyIcon1.sprite = moneyIconSprite;
            m_MoneyIcon2.sprite = moneyIconSprite;
            m_ButtonWatchAds.onClick.AddListener(OnWatchAdsButtonClick);
            m_ButtonPayMoney.onClick.AddListener(OnPayMoneyButtonClick);
            
            go.GetCompItem<SimpleUiButtonView>("watch_ads_button").Init(Managers.AudioManager, Ticker, ColorProvider);
            go.GetCompItem<SimpleUiButtonView>("pay_money_button").Init(Managers.AudioManager, Ticker, ColorProvider);
            go.GetCompItem<SimpleUiDialogPanelView>("dialog_panel_view").Init(Managers.AudioManager, Ticker, ColorProvider);

            m_AdsWatched = false;
            m_MoneyPayed = false;
        }

        public override void OnDialogShow()
        {
            m_PanelShowing = true;
            Cor.Run(Cor.WaitEndOfFrame(() =>
            {
                CommandsProceeder.LockCommand(EInputCommand.ShopMenu, nameof(ICharacterDiedDialogPanel));
                CommandsProceeder.LockCommand(EInputCommand.SettingsMenu, nameof(ICharacterDiedDialogPanel));    
            }));
            m_Animator.speed = ProposalDialogViewer.AnimationSpeed;
            m_Animator.SetTrigger(AnimKeys.Anim);
            IndicateAdsLoading(true);
            Cor.Run(Cor.WaitWhile(
                () => !Managers.AdsManager.RewardedAdReady,
                () => IndicateAdsLoading(false),
                () => !m_PanelShowing));

            var savedGameEntity = Managers.ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                true);
            IndicateMoneyCountLoading(true);
            Cor.Run(Cor.WaitWhile(
                () => savedGameEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                    if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                    {
                        Dbg.LogError("Failed to load money count entity");
                        return;
                    }
                    m_MoneyCount = savedGame.Money;
                    m_TextMoneyCount.text = m_MoneyCount.ToString();
                    IndicateMoneyCountLoading(
                        false,
                        m_MoneyCount >= CommonGameSettings.payToContinueMoneyCount);
                }));
        }

        public override void OnDialogHide()
        {
            m_PanelShowing = true;
            CommandsProceeder.UnlockCommand(EInputCommand.ShopMenu, nameof(ICharacterDiedDialogPanel));
            CommandsProceeder.UnlockCommand(EInputCommand.SettingsMenu, nameof(ICharacterDiedDialogPanel));
            CommandsProceeder.UnlockCommands(RazorMazeUtils.MoveAndRotateCommands, "all");
        }

        #endregion
        
        #region nonpublic methods

        private void IndicateAdsLoading(bool _Indicate)
        {
            if (!m_PanelShowing)
                return;
            m_AnimLoadingAds.SetGoActive(_Indicate);
            m_IconWatchAds.enabled = !_Indicate;
            m_ButtonWatchAds.interactable = !_Indicate;
        }

        private void IndicateMoneyCountLoading(bool _Indicate, bool _IsMoneyEnough = true)
        {
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
            Managers.AdsManager.ShowRewardedAd(
                () => CommonData.PausedByAdvertising = true, 
                () => m_AdsWatched = true);
        }

        private void OnPayMoneyButtonClick()
        {
            var savedGame = new SavedGame
            {
                FileName = CommonData.SavedGameFileName,
                Money = m_MoneyCount - CommonGameSettings.payToContinueMoneyCount,
                Level = Model.LevelStaging.LevelIndex
            };
            Managers.ScoreManager.SaveGameProgress(savedGame, false);
            m_MoneyPayed = true;
        }

        private IEnumerator StartCountdown()
        {
            yield return Cor.Lerp(
                1f,
                0f,
                CountdownTime,
                _Progress => m_RoundFilledBorder.fillAmount = _Progress,
                Ticker,
                (_Broken, _Progress) =>
                {
                    void RaiseContinueCommand()
                    {
                        CommandsProceeder.RaiseCommand(
                            EInputCommand.ReadyToStartLevel,
                            null, 
                            true);
                    }
                    void RaiseLoadFirstLevelInGroupCommand()
                    {
                        CommandsProceeder.RaiseCommand(
                            EInputCommand.ReadyToUnloadLevel,
                            new object[] { CommonInputCommandArgs.LoadFirstLevelFromGroupArg }, 
                            true);
                    }
                    ProposalDialogViewer.Back(_Broken ?
                        (UnityAction)RaiseContinueCommand : RaiseLoadFirstLevelInGroupCommand);
                },
                () => m_AdsWatched || m_MoneyPayed);
        }
        
        #endregion
    }
}