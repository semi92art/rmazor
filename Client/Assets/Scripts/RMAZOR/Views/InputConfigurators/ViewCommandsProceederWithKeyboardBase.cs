using System.Collections.Generic;
using System.Linq;
using Lean.Common;
using mazing.common.Runtime.Ticker;
using RMAZOR.Models;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace RMAZOR.Views.InputConfigurators
{
    public abstract class ViewCommandsProceederWithKeyboardBase : ViewInputCommandsProceeder
    {
        #region constants

        #endregion

        #region nonpublic members

        #endregion

        #region inject
        
        [Inject]
        protected ViewCommandsProceederWithKeyboardBase(ICommonTicker _CommonTicker)
            : base(_CommonTicker) { }

        #endregion

        #region api
        
        public override void UpdateTick()
        {
            UpdateTimeFromLastCommand();
            const bool forced = true;
            EInputCommand? commandKey = null;
            var args = new Dictionary<string, object>();
            ProceedMovement(ref commandKey);
            ProceedRotation(ref commandKey);
            if (commandKey.HasValue)
                RaiseCommand(commandKey.Value, args, forced);
        }
        
        #endregion

        #region nonpublic methods

        protected static void ProceedMovement(ref EInputCommand? _CommandKey)
        {
            ProceedInputWithAdditionalPressed(
                EInputCommand.MoveLeft, ref _CommandKey, null, 
                new [] {KeyCode.A, KeyCode.LeftArrow},
                new [] {KeyCode.LeftShift, KeyCode.RightShift},
                null);
            ProceedInputWithAdditionalPressed(
                EInputCommand.MoveRight, ref _CommandKey, null, 
                new [] {KeyCode.D, KeyCode.RightArrow},
                new [] {KeyCode.LeftShift, KeyCode.RightShift},
                null);
            
            // ProceedInput(EInputCommand.MoveLeft,  ref _CommandKey, KeyCode.A, KeyCode.LeftArrow);
            // ProceedInput(EInputCommand.MoveRight, ref _CommandKey, KeyCode.D, KeyCode.RightArrow);
            ProceedInput(EInputCommand.MoveUp,    ref _CommandKey, KeyCode.W, KeyCode.UpArrow);
            ProceedInput(EInputCommand.MoveDown,  ref _CommandKey, KeyCode.S, KeyCode.DownArrow);
        }

        protected static void ProceedRotation(ref EInputCommand? _CommandKey)
        {
            ProceedInput(EInputCommand.RotateClockwise,        ref _CommandKey, KeyCode.E);
            ProceedInput(EInputCommand.RotateCounterClockwise, ref _CommandKey, KeyCode.Q);
            ProceedInputWithAdditionalPressed(
                EInputCommand.RotateCounterClockwise, ref _CommandKey, null, 
                new [] {KeyCode.LeftArrow},
                null,
                 new [] {KeyCode.LeftShift, KeyCode.RightShift});
            ProceedInputWithAdditionalPressed(
                EInputCommand.RotateClockwise, ref _CommandKey, null, 
                new [] {KeyCode.RightArrow},
                null,
                new [] {KeyCode.LeftShift, KeyCode.RightShift});
        }

        protected static void ProceedInput(
            EInputCommand         _InputCommand,
            ref    EInputCommand? _ExistingCommand,
            params KeyCode[]      _KeyCodes)
        {
            ProceedInput(_InputCommand, ref _ExistingCommand, null, _KeyCodes);
        }

        protected static void ProceedInput(
            EInputCommand      _InputCommand,
            ref EInputCommand? _ExistingCommand,
            UnityAction        _Action,
            params KeyCode[]   _KeyCodes)
        {
            foreach (var keyCode in _KeyCodes)
                ProceedInput(keyCode, _InputCommand, ref _ExistingCommand, _Action);
        }
        
        private static void ProceedInputWithAdditionalPressed(
            EInputCommand        _InputCommand,
            ref EInputCommand?   _ExistingCommand,
            UnityAction          _Action,
            IEnumerable<KeyCode> _KeyCodes,
            IEnumerable<KeyCode> _KeyCodesRejecting,
            IEnumerable<KeyCode> _AdditionalKeyCodes)
        {
            if (_ExistingCommand.HasValue)
                return;
            if (_KeyCodesRejecting != null && _KeyCodesRejecting.Any(IsKeyPressed))
                return;
            bool isKeyDown = _KeyCodes.Any(IsKeyDown);
            bool isAdditionalKeyPressed = _AdditionalKeyCodes == null || _AdditionalKeyCodes.Any(IsKeyPressed);
            if (isKeyDown && isAdditionalKeyPressed)
                _Action?.Invoke();
            _ExistingCommand = (isKeyDown && isAdditionalKeyPressed) ? _InputCommand : (EInputCommand?)null;
        }
        
        protected static void ProceedInput(
            KeyCode            _KeyCode,
            EInputCommand      _InputCommand,
            ref EInputCommand? _ExistingCommand,
            UnityAction        _Action)
        {
            if (_ExistingCommand.HasValue)
                return;
            bool isKeyDown = IsKeyDown(_KeyCode);
            if (isKeyDown)
                _Action?.Invoke();
            _ExistingCommand = isKeyDown ? _InputCommand : (EInputCommand?)null;
        }
        
        private static bool IsKeyDown(KeyCode _KeyCode)
        {
            return LeanInput.GetDown(_KeyCode);
        }
        
        private static bool IsKeyPressed(KeyCode _KeyCode)
        {
            return LeanInput.GetPressed(_KeyCode);
        }

        #endregion
    }
}