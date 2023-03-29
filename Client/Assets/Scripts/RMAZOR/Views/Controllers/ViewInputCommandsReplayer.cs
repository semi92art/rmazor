using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Rotation;
using UnityEngine;
using UnityEngine.Events;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Controllers
{
    public interface IViewInputCommandsReplayer
        : IInit, 
          IOnLevelStageChanged,
          ICharacterMoveFinished,
          IMazeRotationFinished { }

    public class ViewInputCommandsReplayer : InitBase, IViewInputCommandsReplayer
    {
        #region nonpblic members

        private TimeSpan    m_LastSpan;
        private int         m_TakenSteps;
        private bool        m_DoPlay;
        private IEnumerator m_LastPlayCoroutine;

        #endregion
        
        #region inject

        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IViewGameTicker             ViewGameTicker    { get; }

        private ViewInputCommandsReplayer(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker             _ViewGameTicker)
        {
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
            ViewGameTicker    = _ViewGameTicker;
        }

        #endregion

        #region api

        public override void Init()
        {
            CommandsProceeder.Command += OnCommand;
            base.Init();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.ReadyToStart || _Args.PreviousStage != ELevelStage.Loaded)
                return;
            string gameMode = (string) _Args.Arguments.GetSafe(KeyGameMode, out _);
            switch (gameMode)
            {
                case ParameterGameModePuzzles:
                    ProceedPuzzlesGameModeOnLevelStageChanged(_Args);
                    break;
                case ParameterGameModeRandom:
                    ProceedRandomGameModeOnLevelStageChanged(_Args);
                    break;
                case ParameterGameModeMain:
                case ParameterGameModeDailyChallenge:
                case ParameterGameModeBigLevels:
                    m_DoPlay = false;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(gameMode);
            }
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            m_TakenSteps++;
        }
        
        public void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            m_TakenSteps++;
        }

        #endregion

        #region nonpublic methods
        
        private void OnCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            if (_Command != EInputCommand.PlayRecordedCommands)
                return;
            var records = (InputCommandsRecord) _Args.GetSafe(KeyPassCommandsRecord, out _);
            Play(records, false);
        }

        private void ProceedPuzzlesGameModeOnLevelStageChanged(LevelStageArgs _Args)
        {
            object showHintArg = _Args.Arguments.GetSafe(KeyShowPuzzleLevelHint, out bool keyExist);
            if (!keyExist)
            {
                Cor.Stop(m_LastPlayCoroutine);
                return;
            }
            bool showHint = (bool) showHintArg;
            LockMoveAndRotateInputCommands(showHint);
            if (showHint)
            {
                var commandsRecord = (InputCommandsRecord) _Args.Arguments.GetSafe(KeyPassCommandsRecord, out _);
                _Args.Arguments.SetSafe(KeyShowPuzzleLevelHint, false);
                _Args.Arguments.SetSafe(KeyLevelIndex, Model.LevelStaging.LevelIndex);
                Play(commandsRecord, false);
            }
            else
            {
                Cor.Stop(m_LastPlayCoroutine);
                _Args.Arguments.RemoveSafe(KeyShowPuzzleLevelHint, out _);
            }
        }

        private void ProceedRandomGameModeOnLevelStageChanged(LevelStageArgs _Args)
        {
            object aiSimulationArg = _Args.Arguments.GetSafe(KeyAiSimulation, out bool keyExist);
            if (!keyExist)
            {
                LockMoveAndRotateInputCommands(false);
                Cor.Stop(m_LastPlayCoroutine);
                return;
            }
            bool aiSimulation = Convert.ToBoolean(aiSimulationArg);
            LockMoveAndRotateInputCommands(aiSimulation);
            if (!aiSimulation)
            {
                Cor.Stop(m_LastPlayCoroutine);
                return;
            }
            var commandsRecord = Model.Data.Info.AdditionalInfo.PassCommandsRecord;
            Play(commandsRecord, true);
        }

        private void LockMoveAndRotateInputCommands(bool _Lock)
        {
            m_DoPlay = _Lock;
            var action = _Lock ? (UnityAction<IEnumerable<EInputCommand>, string>)
                CommandsProceeder.LockCommands 
                : CommandsProceeder.UnlockCommands;
            action(RmazorUtils.MoveAndRotateCommands, nameof(IViewInputCommandsReplayer));
        }
        
        private void Play(InputCommandsRecord _Records, bool _IgnoreTimeSpan)
        {
            m_TakenSteps = 0;
            Cor.Stop(m_LastPlayCoroutine);
            m_LastPlayCoroutine = _IgnoreTimeSpan 
                ? PlayWithTimeSpanIgnore(_Records)
                : PlayCoroutine(_Records);
            Cor.Run(m_LastPlayCoroutine);
        }

        private IEnumerator PlayCoroutine(InputCommandsRecord _Records)
        {
            yield return Cor.Delay(0.5f, ViewGameTicker);
            foreach (var record in _Records.Records)
                yield return RaiseCommandCoroutine(record, false);
        }
        
        private IEnumerator PlayWithTimeSpanIgnore(InputCommandsRecord _Records)
        {
            yield return Cor.Delay(0.5f, ViewGameTicker);
            yield return RaiseCommandCoroutine(_Records.Records.First(), true);
            while (m_TakenSteps < _Records.Records.Count || !BreakPlayPredicate())
            {
                int currentStep = m_TakenSteps;
                yield return Cor.WaitWhile(
                    () => m_TakenSteps == currentStep);
                if (BreakPlayPredicate()) yield break;
                CheckForInvalidMove(_Records);
                yield return Cor.Delay(0.5f, ViewGameTicker);
                if (BreakPlayPredicate()) yield break;
                if (m_TakenSteps >= _Records.Records.Count) break;
                yield return RaiseCommandCoroutine(_Records.Records[m_TakenSteps], true);
            }
            LockMoveAndRotateInputCommands(false);
            if (BreakPlayPredicate()) yield break;
            while (!BreakPlayPredicate())
            {
                yield return Cor.Delay(0.5f, ViewGameTicker);
                var randomMoveCommand = GetRandomMoveCommand();
                var commandRecord = new InputCommandRecord {Command = randomMoveCommand};
                if (BreakPlayPredicate()) yield break;
                yield return RaiseCommandCoroutine(commandRecord, true);
            }
        }

        private void CheckForInvalidMove(InputCommandsRecord _Records)
        {
            if (m_TakenSteps >= _Records.Records.Count)
                return;
            var command = _Records.Records[m_TakenSteps].Command;
            var dir = GetDirectionByMoveCommand(command);
            var newPos = Model.Character.Position + dir;
            while (
                Model.Data.Info.PathItems.All(_Item => _Item.Position != newPos)
                && ++m_TakenSteps < _Records.Records.Count)
            {
                command = _Records.Records[m_TakenSteps].Command;
                dir = GetDirectionByMoveCommand(command);
                newPos = Model.Character.Position + dir;
            }
        }

        private static V2Int GetDirectionByMoveCommand(EInputCommand _Command)
        {
            return _Command switch
            {
                EInputCommand.MoveLeft  => V2Int.Left,
                EInputCommand.MoveRight => V2Int.Right,
                EInputCommand.MoveDown  => V2Int.Down,
                EInputCommand.MoveUp    => V2Int.Up,
                _                       => throw new SwitchExpressionException(_Command)
            };
        }
        
        private IEnumerator RaiseCommandCoroutine(
            InputCommandRecord _Record,
            bool               _IgnoreTimeSpan)
        {
            if (!_IgnoreTimeSpan)
            {
                float delayInSecs = (float) (_Record.Span - m_LastSpan).TotalSeconds;
                m_LastSpan = _Record.Span;
                yield return Cor.Delay(
                    delayInSecs,
                    ViewGameTicker,
                    () =>
                    {
                        if (BreakPlayPredicate()) 
                            return;
                        CommandsProceeder.RaiseCommand(_Record.Command, null, true);
                    });
            }
            else
                CommandsProceeder.RaiseCommand(_Record.Command, null, true);
        }

        private static EInputCommand GetRandomMoveCommand()
        {
            int idx = Mathf.FloorToInt(UnityEngine.Random.value * (4f - MathUtils.Epsilon));
            return RmazorUtils.MoveCommands[idx];
        }
        
        private bool BreakPlayPredicate()
        {
            var levelStage = Model.LevelStaging.LevelStage;
            return m_DoPlay 
                   && levelStage != ELevelStage.StartedOrContinued
                   && levelStage != ELevelStage.Paused;
        }

        #endregion
    }
}