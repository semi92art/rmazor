using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Analytics;
using Common.Managers.PlatformGameServices;
using Common.Utils;
using Lean.Common;
using mazing.common.Runtime;
using mazing.common.Runtime.Debugging;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using RMAZOR.DebugConsole;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR
{
    public delegate void VisibilityChangedHandler(bool _Visible);

    public interface IDebugConsoleView
    {
        event VisibilityChangedHandler VisibilityChanged;
        bool                           Visible { get; }

        void Init(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IAdsManager                 _AdsManager,
            IScoreManager               _ScoreManager,
            IAudioManager               _AudioManager,
            IAnalyticsManager           _AnalyticsManager,
            IFpsCounter                 _FpsCounter);
        void EnableDebug(bool   _Enable);
        void SetVisibility(bool _Visible);
        void Monitor(string     _Name, bool _Enable, System.Func<object> _Value);
    }
    
    public class DebugConsoleView : MonoBehInitBase, IDebugConsoleView
    {
        #region serialized fields

        [SerializeField] private GameObject      viewContainer; 
        [SerializeField] private Text            logTextArea;
        [SerializeField] private InputField      inputField;
        [SerializeField] private ScrollRect      textScrollbar;
        [SerializeField] private TextMeshProUGUI monitoringValuesArea;

        #endregion

        #region nonpublic members

        private int  m_CurrentCommand;
        private int  m_Index;
        private bool m_IsVisible;
        private bool m_EnableDebug;
        
        private Vector3 m_SwipeFirstPosition;
        private Vector3 m_SwipeLastPosition;
 
        private Vector2 m_FirstPressPos;
        private Vector2 m_SecondPressPos;
        private Vector2 m_CurrentSwipe;

        private IViewInputCommandsProceeder m_CommandsProceeder;
        private IDebugConsoleController        Controller { get; } = new DebugConsoleController();
        
        private readonly Dictionary<string, System.Func<object>> m_MonitoringValuesDict = 
            new Dictionary<string, System.Func<object>>();

        #endregion
        
        #region api

        public event VisibilityChangedHandler VisibilityChanged;
        
        public void Init(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IAdsManager                 _AdsManager,
            IScoreManager               _ScoreManager,
            IAudioManager               _AudioManager,
            IAnalyticsManager           _AnalyticsManager,
            IFpsCounter                 _FpsCounter)
        {
            m_CommandsProceeder = _CommandsProceeder;
            Controller.Init(
                _Model,
                _CommandsProceeder, 
                _AdsManager,
                _ScoreManager,
                _AudioManager,
                _AnalyticsManager,
                _FpsCounter);
            SetCanvas();
            Init();
        }

        public bool Visible => viewContainer.activeSelf;
        
        public void EnableDebug(bool _Enable)
        {
            m_EnableDebug = _Enable;
        }

        public void Monitor(string _Name, bool _Enable, System.Func<object> _Value)
        {
            if (_Enable)
                m_MonitoringValuesDict.Add(_Name, _Value);
            else
                m_MonitoringValuesDict.RemoveSafe(_Name, out _);
            var textParent = monitoringValuesArea.rectTransform.parent.gameObject.RTransform();
            textParent.SetGoActive(m_MonitoringValuesDict.Any());
            textParent.sizeDelta = new Vector2(textParent.sizeDelta.x, 20f * m_MonitoringValuesDict.Count);
        }

        public void UpCommand()
        {
            m_CurrentCommand++;
            m_Index = Controller.CommandHistory.Count - m_CurrentCommand;
            if (m_Index >= 0 && Controller.CommandHistory.Count != 0)
                inputField.text = Controller.CommandHistory[m_Index];
            else
                m_CurrentCommand = Controller.CommandHistory.Count;

            inputField.ActivateInputField();
            inputField.Select();
            inputField.caretPosition = inputField.text.Length;
        }

        public void DownCommand()
        {
            m_CurrentCommand--;
            m_Index = Controller.CommandHistory.Count - m_CurrentCommand;
            if (m_Index < Controller.CommandHistory.Count)
                inputField.text = Controller.CommandHistory[m_Index];
            else
            {
                inputField.text = "";
                m_CurrentCommand = 0;
            }
            inputField.ActivateInputField();
            inputField.Select();
            inputField.caretPosition = inputField.text.Length;
        }

        public void RunCommand()
        {
            Controller.RunCommandString(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
            inputField.Select();
            m_CurrentCommand = 0;
        }
        
        public void SetVisibility(bool _Visible)
        {
            Dbg.Log("Set Debug Console visibility: " + _Visible);
            VisibilityChanged?.Invoke(_Visible);
            var commandsToLock = RmazorUtils.GetCommandsToLockInGameUiMenus();
            if (_Visible)
                m_CommandsProceeder.LockCommands(commandsToLock, nameof(DebugConsoleView));
            else
                m_CommandsProceeder.UnlockCommands(commandsToLock, nameof(DebugConsoleView));
            m_IsVisible = _Visible;
            viewContainer.SetActive(_Visible);
            if (inputField.text == "`")
                inputField.text = "";
            
            textScrollbar.verticalNormalizedPosition = 0f;
            inputField.ActivateInputField();
            inputField.Select();
        }

        #endregion

        #region engine methods

        private IEnumerator Start()
        {
            while (!Initialized)
                yield return null;
            Controller.OnLogChanged += OnLogChanged;
            UpdateLogStr(Controller.Log);
            SetVisibility(false);
        }

        private void OnDestroy()
        {
            Controller.OnLogChanged -= OnLogChanged;
        }

        private void Update()
        {
            if (!m_EnableDebug && !Application.isEditor)
                return;
            ProceedInputCommands();
            ProceedTouchScreenKeyboard();
            ShowMonitoringParameters();
        }

        #endregion

        #region nonpublic methods

        private void SetCanvas()
        {
            var canvas = UIUtils.UiCanvas(
                "DebugConsoleCanvas",
                RenderMode.ScreenSpaceOverlay,
                true,
                1,
                AdditionalCanvasShaderChannels.None,
                CanvasScaler.ScaleMode.ScaleWithScreenSize,
                new Vector2Int(1920,1080),
                CanvasScaler.ScreenMatchMode.Shrink,
                0f,
                100,
                true,
                GraphicRaycaster.BlockingObjects.None);
            DontDestroyOnLoad(canvas);
            transform.SetParent(canvas.transform);
            gameObject.RTransform().SetParams(RectTransformLite.FullFill);
        }

        private void ProceedInputCommands()
        {
            if (LeanInput.GetDown(KeyCode.Escape) && m_IsVisible)
                ToggleVisibility();
            if (LeanInput.GetDown(KeyCode.Return))
                RunCommand();
            // хз где у линтача тильда
            if (LeanInput.GetDown(KeyCode.KeypadDivide))
                ToggleVisibility();
            //Arrow up in history
            if (LeanInput.GetDown(KeyCode.UpArrow))
                UpCommand();
            if (LeanInput.GetDown(KeyCode.DownArrow))
                DownCommand();
        }

        private void ShowMonitoringParameters()
        {
            if (!m_IsVisible) 
                return;
            var sb = new StringBuilder();
            foreach ((string key, var value) in m_MonitoringValuesDict)
                sb.AppendLine(key + " " + value);
            monitoringValuesArea.text = sb.ToString();
        }

        private void ProceedTouchScreenKeyboard()
        {
            if (inputField.touchScreenKeyboard == null)
                return;
            switch (inputField.touchScreenKeyboard.status)
            {
                case TouchScreenKeyboard.Status.Visible:
                    break;
                case TouchScreenKeyboard.Status.Done:
                    RunCommand();
                    break;
                case TouchScreenKeyboard.Status.Canceled:
                case TouchScreenKeyboard.Status.LostFocus:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(inputField.touchScreenKeyboard.status);
            }
        }
        
        private void ToggleVisibility()
        {
            SetVisibility(!viewContainer.activeSelf);
        }

        private void OnLogChanged(string[] _NewLog)
        {
            UpdateLogStr(_NewLog);
        }

        private void UpdateLogStr(string[] _NewLog)
        {
            logTextArea.text = _NewLog == null ? "" : string.Join("\n", _NewLog);
        }

        #endregion
    }
}