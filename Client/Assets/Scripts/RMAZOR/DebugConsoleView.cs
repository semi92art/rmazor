using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Entities.UI;
using Common.Exceptions;
using Common.Extensions;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.PlatformGameServices;
using Common.Utils;
using Lean.Common;
using RMAZOR.DebugConsole;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR
{
    public delegate void VisibilityChangedHandler(bool _Visible);
    
    public class DebugConsoleView : MonoBehaviour
    {
        #region singleton
        
        private static DebugConsoleView _instance;
        public static DebugConsoleView Instance => _instance.IsNull() ? _instance = Create() : _instance;

        #endregion
        
        #region factory

        private static DebugConsoleView Create()
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

            return new PrefabSetManager(new AssetBundleManagerFake()).InitUiPrefab(
                UIUtils.UiRectTransform(canvas.RTransform(), RectTransformLite.FullFill),
                "debug_console", "console")?.GetComponent<DebugConsoleView>();
        }

        #endregion
        
        #region serialized fields

        [SerializeField] private GameObject      viewContainer; 
        [SerializeField] private Text            logTextArea;
        [SerializeField] private InputField      inputField;
        [SerializeField] private ScrollRect      textScrollbar;
        [SerializeField] private TextMeshProUGUI monitoringValuesArea;

        #endregion

        #region nonpublic members

        private Vector3 m_SwipeFirstPosition;
        private Vector3 m_SwipeLastPosition;
        private float   m_SwipeDragThreshold;
        private int     m_CurrentCommand;
        private int     m_Index;
        private bool    m_IsVisible;
        private bool    m_EnableDebug;
 
        private Vector2 m_FirstPressPos;
        private Vector2 m_SecondPressPos;
        private Vector2 m_CurrentSwipe;

        private IViewInputCommandsProceeder m_CommandsProceeder;
        
        private readonly Dictionary<string, System.Func<object>> m_MonitoringValuesDict = 
            new Dictionary<string, System.Func<object>>();

        #endregion
        
        #region api

        public IDebugConsoleController        Controller { get; } = new DebugConsoleController();
        public event VisibilityChangedHandler VisibilityChanged;
        
        public void Init(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IAdsManager                 _AdsManager,
            IScoreManager               _ScoreManager)
        {
            m_CommandsProceeder = _CommandsProceeder;
            Controller.Init(_Model, _CommandsProceeder, _AdsManager, _ScoreManager);
        }
        
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
        }

        public void RunCommand()
        {
            Controller.RunCommandString(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
            inputField.Select();
            m_CurrentCommand = 0;
        }

        #endregion

        #region engine methods

        private void Start()
        {
            Controller.OnLogChanged += OnLogChanged;
            UpdateLogStr(Controller.Log);
            m_SwipeDragThreshold = GraphicUtils.ScreenSize.x * 0.2f;
            SetVisibility(false);
        }

        private void OnDestroy()
        {
            Controller.OnLogChanged -= OnLogChanged;
        }

        private void Update()
        {
            if (!m_EnableDebug)
                return;
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

            if (LeanInput.GetTouchCount() > 0)
            {
                CommonUtils.GetTouch(0, out _, out Vector2 pos, out _, out bool began, out bool ended);
                if (began)
                    m_FirstPressPos = pos;
                else if (ended)
                {
                    m_SecondPressPos = pos;
                    m_CurrentSwipe = new Vector2(
                        m_SecondPressPos.x - m_FirstPressPos.x, 
                        m_SecondPressPos.y - m_FirstPressPos.y);
                    if (m_CurrentSwipe.magnitude < m_SwipeDragThreshold)
                    {
                        Dbg.Log("m_CurrentSwipe.magnitude: " + m_CurrentSwipe.magnitude + ", m_SwipeDragThreshold: " + m_SwipeDragThreshold);
                        return;
                    }
                    m_CurrentSwipe.Normalize();
                    if (Mathf.Abs(m_CurrentSwipe.x) > Mathf.Abs(m_CurrentSwipe.y)
                        && m_CurrentSwipe.x < 0
                        && !m_IsVisible)
                    {
                        ToggleVisibility();
                    }
                }
            }
            if (m_IsVisible)
            {
                var sb = new StringBuilder();
                foreach ((string key, var value) in m_MonitoringValuesDict)
                    sb.AppendLine(key + " " + value);
                monitoringValuesArea.text = sb.ToString();
            }
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

        #endregion

        #region nonpublic methods

        private void ToggleVisibility()
        {
            SetVisibility(!viewContainer.activeSelf);
            textScrollbar.verticalNormalizedPosition = 0f;
            inputField.ActivateInputField();
            inputField.Select();
        }

        private void SetVisibility(bool _Visible)
        {
            Dbg.Log(nameof(SetVisibility) + " " + _Visible);
            VisibilityChanged?.Invoke(_Visible);
            var commands = new[] {EInputCommand.ShopMenu, EInputCommand.SettingsMenu}
                .Concat(RmazorUtils.MoveAndRotateCommands);
            if (_Visible)
                m_CommandsProceeder.LockCommands(commands, nameof(DebugConsoleView));
            else
                m_CommandsProceeder.UnlockCommands(commands, nameof(DebugConsoleView));
            m_IsVisible = _Visible;
            viewContainer.SetActive(_Visible);
            if (inputField.text == "`")
                inputField.text = "";
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