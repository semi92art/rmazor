using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Exceptions;
using Common.Extensions;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI.Game_Logo;
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

        #endregion
        
        #region inject

        private IViewBetweenLevelTransitioner BetweenLevelTransitioner { get; }
        private IColorProvider                ColorProvider            { get; }
        private IViewUIPrompt                 Prompt                   { get; }
        private IViewUICongratsMessage        CongratsMessage          { get; }
        private IViewUIGameLogo               GameLogo                 { get; }
        private IViewUILevelsPanel            LevelsPanel              { get; }
        private IViewUIRotationControls       RotationControls         { get; }
        private IViewUITopButtons             TopButtons               { get; }
        private IViewUITutorial               Tutorial                 { get; }

        public ViewUIGameControls(
            IModelGame                    _Model,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewBetweenLevelTransitioner _BetweenLevelTransitioner,
            IColorProvider                _ColorProvider,
            IViewUIPrompt                 _Prompt,
            IViewUICongratsMessage        _CongratsMessage,
            IViewUIGameLogo              _GameLogo,
            IViewUILevelsPanel            _LevelsPanel,
            IViewUIRotationControls       _RotationControls,
            IViewUITopButtons             _TopButtons,
            IViewUITutorial               _Tutorial)
            : base(_Model, _CommandsProceeder)
        {
            BetweenLevelTransitioner = _BetweenLevelTransitioner;
            ColorProvider            = _ColorProvider;
            Prompt                   = _Prompt;
            CongratsMessage          = _CongratsMessage;
            GameLogo                 = _GameLogo;
            LevelsPanel              = _LevelsPanel;
            RotationControls         = _RotationControls;
            TopButtons               = _TopButtons;
            Tutorial                 = _Tutorial;
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
                TopButtons,
                LevelsPanel,
                Tutorial
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
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:             ShowControls(true, false);  break;
                case ELevelStage.ReadyToUnloadLevel: ShowControls(false, false); break;
            }
        }

        #endregion

        #region nonpublic methds

        private void InitGameUI()
        {
            var allInitItems = GetInterfaceOfProceeders<IInitViewUIItem>();
            foreach (var uiItem in allInitItems)
                uiItem?.Init(new Vector4(0, 0, BottomOffset, TopOffset));
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
                    .Concat(TopButtons.GetRenderers());
            foreach (var rendComp in allRenderers)
            {
                switch (rendComp)
                {
                    case ShapeRenderer rend1:  rend1.Color = _Color; break;
                    case SpriteRenderer rend2: rend2.color = _Color; break;
                    case TMP_Text rend3:       rend3.color = _Color; break;
                }
            }
        }
        
        private void ShowControls(bool _Show, bool _Instantly)
        {
            LevelsPanel.ShowControls(_Show, _Instantly);
            TopButtons.ShowControls(_Show, _Instantly);
            if (_Instantly)
                return;
            var allRenderers = GetInterfaceOfProceeders<IViewUIGetRenderers>()
                .Where(_R => _R != null)
                .SelectMany(_Item => _Item.GetRenderers());
            BetweenLevelTransitioner.DoAppearTransition(
                _Show, 
                new Dictionary<IEnumerable<Component>, Func<Color>>
                {
                    {allRenderers, () => ColorProvider.GetColor(ColorIds.UI)}
                },
                _Type: EAppearTransitionType.WithoutDelay);
        }

        private void LockCommands(LevelStageArgs _Args)
        {
            const string group = nameof(IViewUIGameControls);
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                case ELevelStage.Paused:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    CommandsProceeder.LockCommands(CommandsProceeder.GetAllCommands(), group);
                    CommandsProceeder.UnlockCommand(EInputCommand.ShopMenu, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.SettingsMenu, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.ReadyToUnloadLevel, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.UnloadLevel, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.PauseLevel, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.UnPauseLevel, group);
                    break;
                case ELevelStage.Finished:
                    CommandsProceeder.LockCommands(CommandsProceeder.GetAllCommands(), group);
                    CommandsProceeder.UnlockCommand(EInputCommand.ShopMenu, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.SettingsMenu, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.ReadyToUnloadLevel, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.PauseLevel, group);
                    CommandsProceeder.UnlockCommand(EInputCommand.UnPauseLevel, group);
                    break;
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                    CommandsProceeder.UnlockAllCommands(group);
                    
                    if (MazeContainsGravityItems())
                        CommandsProceeder.UnlockCommands(RmazorUtils.RotateCommands, group);
                    else
                        CommandsProceeder.LockCommands(RmazorUtils.RotateCommands, group);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        private bool MazeContainsGravityItems()
        {
            return Model.GetAllProceedInfos()
                .Any(_Info => RmazorUtils.GravityItemTypes.ContainsAlt(_Info.Type));
        }

        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return Array.ConvertAll(m_ProceedersCached, _Item => _Item as T);
        }
        
        #endregion
    }
}