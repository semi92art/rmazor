using System.Collections;
using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities.UI;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IRateGameDialogPanel : IDialogPanel
    {
        bool CanBeClosed { get; set; }
        void SetDialogTitle(string _Text);
    }
    
    public class RateGameDialogPanelFake : FakeDialogPanel, IRateGameDialogPanel
    {
        public bool CanBeClosed                 { get; set; }
        public void SetDialogTitle(string _Text) { }
    }
    
    public class RateGameDialogPanel : DialogPanelBase, IRateGameDialogPanel
    {
        #region nonpublic members

        private Animator           m_Animator;
        private Button             m_ButtonClose;
        private Button             m_ButtonRateGame;
        private TextMeshProUGUI    m_TextTitle;
        private TextMeshProUGUI    m_TextRateGame;

        #endregion

        #region inject
        
        private IViewInputCommandsProceeder CommandsProceeder    { get; }

        public RateGameDialogPanel(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            IDialogViewersController    _DialogViewersController,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _Managers, 
                _Ticker, 
                _DialogViewersController, 
                _CameraProvider,
                _ColorProvider)
        {
            CommandsProceeder    = _CommandsProceeder;
        }

        #endregion

        #region api

        public override EUiCategory Category      => EUiCategory.RateGame;
        public override bool        AllowMultiple => false;
        public override Animator    Animator      => m_Animator;

        public bool CanBeClosed
        {
            get => m_ButtonClose.gameObject.activeSelf;
            set => m_ButtonClose.SetGoActive(value);
        }

        public override void LoadPanel()
        {
            base.LoadPanel();
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Proposal);
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    dv.Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "rate_game_panel");
            PanelObject = go.RTransform();
            go.SetActive(false);
            m_Animator        = go.GetCompItem<Animator>("animator");
            m_ButtonClose     = go.GetCompItem<Button>("close_button");
            m_ButtonRateGame  = go.GetCompItem<Button>("rate_game_button");
            m_TextRateGame    = go.GetCompItem<TextMeshProUGUI>("rate_game_text");
            m_TextTitle       = go.GetCompItem<TextMeshProUGUI>("title");
            var panel = go.GetCompItem<SimpleUiDialogPanelView>("panel");
            panel.Init(Ticker, ColorProvider, Managers.AudioManager, Managers.LocalizationManager, Managers.PrefabSetManager);
            var button = go.GetCompItem<SimpleUiButtonView>("rate_game_button");
            button.Init(Ticker, ColorProvider, Managers.AudioManager, Managers.LocalizationManager, Managers.PrefabSetManager);
            button.Highlighted = true;
            m_ButtonClose.onClick.AddListener(OnCloseButtonClick);
            m_ButtonRateGame.onClick.AddListener(OnRateGameButtonClick);
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_TextRateGame, ETextType.MenuUI, "rate_game"));
            m_TextRateGame.text = Managers.LocalizationManager.GetTranslation("rate_game");
            m_TextTitle.text = Managers.LocalizationManager.GetTranslation("we_need_comment");
            var closeButtonAnimator = m_ButtonClose.GetComponent<Animator>();
            if (closeButtonAnimator.IsNotNull())
                closeButtonAnimator.enabled = false;
        }

        public override void OnDialogStartAppearing()
        {
            Cor.Run(Cor.WaitNextFrame(() =>
            {
                CommandsProceeder.LockCommands(GetCommandsToLock(), nameof(IRateGameDialogPanel));
            }));
            m_ButtonClose.SetGoActive(false);
            Cor.Run(OnPanelStartAnimationFinished());
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            CommandsProceeder.UnlockCommands(GetCommandsToLock(), nameof(IRateGameDialogPanel));
            base.OnDialogDisappeared();
        }

        public void SetDialogTitle(string _Text)
        {
            m_TextTitle.text = _Text;
        }
        
        #endregion

        #region nonpublic methods

        private IEnumerator OnPanelStartAnimationFinished()
        {
            const float closeButtonEnableDelay = 3f;
            float time = Ticker.Time;
            yield return Cor.WaitWhile(
                () => time + closeButtonEnableDelay > Ticker.Time,
                () => m_ButtonClose.SetGoActive(true));
        }
        
        private void OnRateGameButtonClick()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.RateGameButton2Pressed);
            Managers.ShopManager.RateGame(false);
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Proposal);
            dv.Back();
            
        }

        private void OnCloseButtonClick()
        {
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Proposal);
            dv.Back();
        }

        private static IEnumerable<EInputCommand> GetCommandsToLock()
        {
            return RmazorUtils.MoveAndRotateCommands;
        }

        #endregion
    }
}