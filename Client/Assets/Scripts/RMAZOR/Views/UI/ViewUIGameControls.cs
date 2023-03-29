using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewUILevelSkippers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI.Game_Logo;
using RMAZOR.Views.UI.Game_UI_Top_Buttons;
using Shapes;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public class ViewUIGameControls : ViewUIGameControlsBase
    {
        #region constants

        private const float BottomOffset    = 1f;
        private const float TopOffset       = 5f;

        #endregion

        #region nonpublic members

        private object[] m_ProceedersCached;
        private bool     m_ControlsShownFirstTime;

        #endregion
        
        #region inject

        private IRendererAppearTransitioner     AppearTransitioner   { get; }
        private IColorProvider                  ColorProvider        { get; }
        private IViewGameUIPrompt               Prompt               { get; }
        private IViewUICongratsMessage          CongratsMessage      { get; }
        private IViewUIGameLogo                 GameLogo             { get; }
        private IViewGameUILevelsPanel          LevelsPanel          { get; }
        private IViewUIRotationControls         RotationControls     { get; }
        private IViewGameUIButtons              Buttons              { get; }
        private IViewUITutorial                 Tutorial             { get; }
        private IViewGameUiLevelSkipper             LevelSkipper         { get; }
        private IViewUIStarsAndTimePanel        StarsAndTimePanel    { get; }
        private IViewGameUIDailyChallengePanel  DailyChallengePanel  { get; }
        private IViewGameUiCreatingLevelMessage CreatingLevelMessage { get; }
        private IViewGameUiHintPlayingMessage   HintPlayingMessage   { get; }

        public ViewUIGameControls(
            IModelGame                      _Model,
            IViewInputCommandsProceeder     _CommandsProceeder,
            IRendererAppearTransitioner     _AppearTransitioner,
            IColorProvider                  _ColorProvider,
            IViewGameUIPrompt               _Prompt,
            IViewUICongratsMessage          _CongratsMessage,
            IViewUIGameLogo                 _GameLogo,
            IViewGameUILevelsPanel          _LevelsPanel,
            IViewUIRotationControls         _RotationControls,
            IViewGameUIButtons              _Buttons,
            IViewUITutorial                 _Tutorial,
            IViewGameUiLevelSkipper             _LevelSkipper,
            IViewUIStarsAndTimePanel        _StarsAndTimePanel,
            IViewGameUIDailyChallengePanel  _DailyChallengePanel,
            IViewGameUiCreatingLevelMessage _CreatingLevelMessage,
            IViewGameUiHintPlayingMessage   _HintPlayingMessage)
            : base(_Model, _CommandsProceeder)
        {
            AppearTransitioner   = _AppearTransitioner;
            ColorProvider        = _ColorProvider;
            Prompt               = _Prompt;
            CongratsMessage      = _CongratsMessage;
            GameLogo             = _GameLogo;
            LevelsPanel          = _LevelsPanel;
            RotationControls     = _RotationControls;
            Buttons              = _Buttons;
            Tutorial             = _Tutorial;
            LevelSkipper         = _LevelSkipper;
            StarsAndTimePanel    = _StarsAndTimePanel;
            DailyChallengePanel  = _DailyChallengePanel;
            CreatingLevelMessage = _CreatingLevelMessage;
            HintPlayingMessage   = _HintPlayingMessage;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            m_ProceedersCached = new object[]
            {
                CongratsMessage,
                GameLogo,
                RotationControls,
                Prompt,
                Buttons,
                StarsAndTimePanel,
                LevelsPanel,
                Tutorial,
                LevelSkipper,
                DailyChallengePanel,
                CreatingLevelMessage,
                HintPlayingMessage
            };
            InitGameUI();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            LockCommands(_Args);
            var allOnLevelStageChangedItems = GetInterfaceOfProceeders<IOnLevelStageChanged>();
            foreach (var uiItem in allOnLevelStageChangedItems)
                uiItem?.OnLevelStageChanged(_Args);
            ShowControls(_Args);
        }

        public override void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            DailyChallengePanel.OnCharacterMoveFinished(_Args);
        }

        public override void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            DailyChallengePanel.OnMazeRotationFinished(_Args);
        }

        #endregion

        #region nonpublic methds

        private void InitGameUI()
        {
            var allInitItems = GetInterfaceOfProceeders<IInitViewUIItem>();
            var offsets = new Vector4(0, 0, BottomOffset, TopOffset);
            foreach (var uiItem in allInitItems)
                uiItem?.Init(offsets);
            ColorProvider.ColorChanged += OnColorChanged;
            Tutorial.TutorialStarted   += OnTutorialStarted;
            Tutorial.TutorialFinished  += OnTutorialFinished;
        }

        private void OnTutorialStarted(ETutorialType _Type)
        {
            Prompt.OnTutorialStarted(_Type);
            RotationControls.OnTutorialStarted(_Type);
        }
        
        private void OnTutorialFinished(ETutorialType _Type)
        {
            Prompt.OnTutorialFinished(_Type);
            RotationControls.OnTutorialFinished(_Type);
        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.UI) 
                return;
            var allRenderers = 
                CongratsMessage.GetRenderers()
                    .Concat(LevelsPanel.GetRenderers())
                    .Concat(Buttons.GetRenderers());
            foreach (var rendComp in allRenderers)
            {
                switch (rendComp)
                {
                    case ShapeRenderer  rend1: rend1.Color = _Color; break;
                    case SpriteRenderer rend2: rend2.color = _Color; break;
                    case TMP_Text       rend3: rend3.color = _Color; break;
                }
            }
        }
        
        private void ShowControls(LevelStageArgs _Args)
        {
            bool? doShowNullable = MustShowControls(_Args, out bool instantly);
            if (!doShowNullable.HasValue)
                return;
            bool doShow = doShowNullable.Value;
            foreach (var iShowControls in GetInterfaceOfProceeders<IShowControls>())
                iShowControls.ShowControls(doShow, instantly);
            if (instantly)
                return;
            var allRenderers = GetInterfaceOfProceeders<IViewUIGetRenderers>()
                .Where(_R => _R != null)
                .SelectMany(_Item => _Item.GetRenderers());
            AppearTransitioner.DoAppearTransition(
                doShow, 
                new Dictionary<IEnumerable<Component>, Func<Color>>
                {
                    {allRenderers, () => ColorProvider.GetColor(ColorIds.UI)}
                }, 0f);
        }

        private bool? MustShowControls(LevelStageArgs _Args, out bool _Instantly)
        {
            bool? doShow;
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    (doShow, _Instantly) = (true, !m_ControlsShownFirstTime);
                    break;
                case ELevelStage.StartedOrContinued when 
                    _Args.PreviousStage == ELevelStage.ReadyToStart
                    && _Args.PrePrePreviousStage != ELevelStage.CharacterKilled
                    && !m_ControlsShownFirstTime:
                    (doShow, _Instantly) = (true, true);
                    m_ControlsShownFirstTime = true;
                    break;
                case ELevelStage.Unloaded:
                    (doShow, _Instantly) = (false, false);
                    break;
                default:
                    (_Instantly, doShow) = (false, null);
                    break;
            }
            return doShow;
        }

        private void LockCommands(LevelStageArgs _Args)
        {
            const string group = nameof(IViewUIGameControls);
            switch (_Args.LevelStage)
            {
                case ELevelStage.None:
                    CommandsProceeder.LockCommands(CommandsProceeder.GetAllCommands(), group);
                    CommandsProceeder.UnlockCommands(GetUiCommands(), group);
                    CommandsProceeder.UnlockCommand(EInputCommand.SelectCharacter, group);
                    break;
                case ELevelStage.Loaded:
                case ELevelStage.Paused:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                {
                    CommandsProceeder.LockCommands(CommandsProceeder.GetAllCommands(), group);
                    CommandsProceeder.UnlockCommands(GetUiAndLevelStagingCommands(), group);
                }
                    break;
                case ELevelStage.Finished:
                {
                    CommandsProceeder.LockCommands(CommandsProceeder.GetAllCommands(), group);
                    CommandsProceeder.UnlockCommands(GetUiAndLevelStagingCommands(), group);
                }
                    break;
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                    CommandsProceeder.UnlockCommands(CommandsProceeder.GetAllCommands(), group);
                    if (MazeContainsGravityItems())
                        CommandsProceeder.UnlockCommands(RmazorUtils.RotateCommands, group);
                    else
                        CommandsProceeder.LockCommands(RmazorUtils.RotateCommands, group);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.LevelStage);
            }
        }
        
        private bool MazeContainsGravityItems()
        {
            return Model.GetAllProceedInfos()
                .Any(_Info => RmazorUtils.GravityItemTypes.ContainsAlt(_Info.Type));
        }

        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return Array.ConvertAll(m_ProceedersCached, _Item => _Item as T)
                .Where(_I => _I != null)
                .ToArray();
        }

        private static IEnumerable<EInputCommand> GetUiCommands()
        {
            return new[]
            {
                EInputCommand.ShopMoneyPanel,
                EInputCommand.SettingsPanel,
                EInputCommand.DailyGiftPanel,
                EInputCommand.LevelsPanel,
                EInputCommand.MainMenuPanel
            };
        }

        private static IEnumerable<EInputCommand> GetLevelStagingCommands()
        {
            return new[]
            {
                EInputCommand.StartUnloadingLevel,
                EInputCommand.PauseLevel,
                EInputCommand.UnloadLevel,
                EInputCommand.UnPauseLevel
            };
        }
        
        private static IEnumerable<EInputCommand> GetUiAndLevelStagingCommands()
        {
            return GetUiCommands().Concat(GetLevelStagingCommands());
        }
        
        #endregion
    }
}