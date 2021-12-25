using System;
using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Shapes;
using TMPro;
using UnityEngine;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIGameControls : ViewUIGameControlsBase
    {
        #region constants

        private const float BottomOffset    = 1f;
        private const float TopOffset       = 5f;

        #endregion
        
        #region inject

        private IModelGame              Model              { get; }
        private IViewAppearTransitioner AppearTransitioner { get; }
        private IColorProvider          ColorProvider      { get; }
        private IViewUIPrompt           Prompt             { get; }
        private IViewUICongratsMessage  CongratsMessage    { get; }
        private IViewUIStartLogo        StartLogo          { get; }
        private IViewUILevelsPanel      LevelsPanel        { get; }
        private IViewUIRotationControls RotationControls   { get; }
        private IViewUITopButtons       TopButtons         { get; }
        private IViewUITutorial         Tutorial           { get; }

        public ViewUIGameControls(
            IModelGame _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewAppearTransitioner _AppearTransitioner,
            IColorProvider _ColorProvider,
            IViewUIPrompt _Prompt,
            IViewUICongratsMessage _CongratsMessage,
            IViewUIStartLogo _StartLogo,
            IViewUILevelsPanel _LevelsPanel,
            IViewUIRotationControls _RotationControls,
            IViewUITopButtons _TopButtons,
            IViewUITutorial _Tutorial) 
            : base(_CommandsProceeder)
        {
            Model = _Model;
            AppearTransitioner = _AppearTransitioner;
            ColorProvider = _ColorProvider;
            Prompt = _Prompt;
            CongratsMessage = _CongratsMessage;
            StartLogo = _StartLogo;
            LevelsPanel = _LevelsPanel;
            RotationControls = _RotationControls;
            TopButtons = _TopButtons;
            Tutorial = _Tutorial;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            InitGameUI();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            LockCommands(_Args);
            var allOnLevelStageChangedItems = GetInterfaceOfProceeders<IOnLevelStageChanged>();
            foreach (var uiItem in allOnLevelStageChangedItems)
                uiItem.OnLevelStageChanged(_Args);
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
                uiItem.Init(new Vector4(0, 0, BottomOffset, TopOffset));
            ColorProvider.ColorChanged += OnColorChanged;
            Tutorial.TutorialStarted   += OnTutorialStarted;
            Tutorial.TutorialFinished  += OnTutorialFinished;
        }

        private void OnTutorialStarted(ETutorialType _Type)
        {
            Prompt.OnTutorialStarted(_Type);
            StartLogo.OnTutorialStarted(_Type);
            RotationControls.OnTutorialStarted(_Type);
        }
        
        private void OnTutorialFinished(ETutorialType _Type)
        {
            Prompt.OnTutorialFinished(_Type);
            StartLogo.OnTutorialFinished(_Type);
            Prompt.OnTutorialFinished(_Type);
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
                if (rendComp is ShapeRenderer rend1)
                    rend1.Color = _Color;
                else if (rendComp is SpriteRenderer rend2)
                    rend2.color = _Color;
                else if (rendComp is TMP_Text rend3)
                    rend3.color = _Color;
            }
        }
        
        private void ShowControls(bool _Show, bool _Instantly)
        {
            LevelsPanel.ShowControls(_Show, _Instantly);
            TopButtons.ShowControls(_Show, _Instantly);
            if (_Instantly)
                return;
            var allRenderers = GetInterfaceOfProceeders<IViewUIGetRenderers>()
                .SelectMany(_Item => _Item.GetRenderers());
            AppearTransitioner.DoAppearTransition(
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
                        CommandsProceeder.UnlockCommands(RazorMazeUtils.GetRotateCommands(), group);
                    else
                        CommandsProceeder.LockCommands(RazorMazeUtils.GetRotateCommands(), group);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        private bool MazeContainsGravityItems()
        {
            return Model.GetAllProceedInfos()
                .Any(_Info => RazorMazeUtils.GravityItemTypes().Contains(_Info.Type));
        }

        private List<T> GetInterfaceOfProceeders<T>() where T : class
        {
            var proceeders = new List<object>
                {
                    CongratsMessage, 
                    StartLogo,
                    RotationControls, 
                    Prompt,
                    TopButtons,
                    LevelsPanel, 
                    Tutorial
                }.Where(_Proceeder => _Proceeder != null);
            return proceeders.Where(_Proceeder => _Proceeder is T).Cast<T>().ToList();
        }

        #endregion
    }
}