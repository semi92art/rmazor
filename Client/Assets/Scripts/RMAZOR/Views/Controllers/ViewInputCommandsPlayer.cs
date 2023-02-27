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
    public interface IViewInputCommandsPlayer
        : IInit, 
          IOnLevelStageChanged,
          ICharacterMoveFinished,
          IMazeRotationFinished { }

    public class ViewInputCommandsPlayer : InitBase, IViewInputCommandsPlayer
    {
        #region nonpblic members

        private TimeSpan m_LastSpan;
        private int      m_TakenSteps;

        #endregion
        
        #region inject

        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IViewGameTicker             ViewGameTicker    { get; }

        private ViewInputCommandsPlayer(
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
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(gameMode);
            }
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            Dbg.Log("On Character Move Finished");
            m_TakenSteps++;
        }
        
        public void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            Dbg.Log("On Maze Rotation Finished");
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
                return;
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
                _Args.Arguments.RemoveSafe(KeyShowPuzzleLevelHint, out _);
            }
        }

        private void ProceedRandomGameModeOnLevelStageChanged(LevelStageArgs _Args)
        {
            object aiSimulationArg = _Args.Arguments.GetSafe(KeyAiSimulation, out bool keyExist);
            if (!keyExist)
                return;
            bool aiSimulation = Convert.ToBoolean(aiSimulationArg);
            if (!aiSimulation)
                return;
            var commandsRecord = Model.Data.Info.AdditionalInfo.PassCommandsRecord;
            Play(commandsRecord, true);
        }

        private void LockMoveAndRotateInputCommands(bool _Lock)
        {
            var action = _Lock ? (UnityAction<IEnumerable<EInputCommand>, string>)
                CommandsProceeder.LockCommands 
                : CommandsProceeder.UnlockCommands;
            action(RmazorUtils.MoveAndRotateCommands, nameof(IViewInputCommandsPlayer));
        }
        
        private void Play(InputCommandsRecord _Records, bool _IgnoreTimeSpan)
        {
            m_TakenSteps = 0;
            var playCoroutine = _IgnoreTimeSpan 
                ? PlayWithTimeSpanIgnore(_Records)
                : PlayCoroutine(_Records);
            Cor.Run(playCoroutine);
        }

        private IEnumerator PlayCoroutine(InputCommandsRecord _Records)
        {
            yield return Cor.Delay(0.5f, ViewGameTicker);
            foreach (var record in _Records.Records)
                yield return RaiseCommandCoroutine(record, false);
        }
        
        private IEnumerator PlayWithTimeSpanIgnore(InputCommandsRecord _Records)
        {
            ELevelStage GetLevelStage() => Model.LevelStaging.LevelStage;
            yield return Cor.Delay(0.5f, ViewGameTicker);
            yield return RaiseCommandCoroutine(_Records.Records.First(), true);
            while (m_TakenSteps < _Records.Records.Count || GetLevelStage() != ELevelStage.Finished)
            {
                int currentStep = m_TakenSteps;
                yield return Cor.WaitWhile(() => m_TakenSteps == currentStep);
                if (GetLevelStage() == ELevelStage.Finished) yield break;
                CheckForInvalidMove(_Records);
                if (GetLevelStage() == ELevelStage.Finished) yield break;
                yield return Cor.Delay(0.5f, ViewGameTicker);
                if (GetLevelStage() == ELevelStage.Finished) yield break;
                if (m_TakenSteps >= _Records.Records.Count) break;
                yield return RaiseCommandCoroutine(_Records.Records[m_TakenSteps], true);
            }
            Dbg.Log("Start raising random commands");
            while (GetLevelStage() != ELevelStage.Finished && GetLevelStage() != ELevelStage.None)
            {
                yield return Cor.Delay(0.5f, ViewGameTicker);
                var randomMoveCommand = GetRandomMoveCommand();
                var commandRecord = new InputCommandRecord {Command = randomMoveCommand};
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

        private V2Int GetDirectionByMoveCommand(EInputCommand _Command)
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
                    () => CommandsProceeder.RaiseCommand(_Record.Command, null, true));
            }
            else
                CommandsProceeder.RaiseCommand(_Record.Command, null, true);
        }

        private EInputCommand GetRandomMoveCommand()
        {
            int idx = Mathf.FloorToInt(UnityEngine.Random.value * (4f - MathUtils.Epsilon));
            return RmazorUtils.MoveCommands[idx];
        }

        #endregion
    }
}