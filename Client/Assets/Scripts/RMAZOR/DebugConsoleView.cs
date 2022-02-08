using System.Linq;
using Common;
using Common.Entities.UI;
using Common.Extensions;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Scores;
using Common.Utils;
using GameHelpers;
using Lean.Common;
using Managers;
using RMAZOR;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DebugConsole
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
                "debug_console", "console").GetComponent<DebugConsoleView>();
        }

        #endregion
        
        #region serialized fields

        // Container for console view, should be a child of this GameObject
        [SerializeField] private GameObject viewContainer; 
        [SerializeField] private Text       logTextArea;
        [SerializeField] private InputField inputField;
        [SerializeField] private ScrollRect textScrollbar;

        #endregion
        
        #region public members

        public IDebugConsoleController        Controller { get; } = new DebugConsoleController();
        public event VisibilityChangedHandler VisibilityChanged;
        
        #endregion

        #region nonpublic members

        private Vector3 m_SwipeFirstPosition;
        private Vector3 m_SwipeLastPosition;
        private float m_SwipeDragDistance;
        private int m_CurrentCommand;
        private int m_Index;
        private bool m_IsVisible;
 
        private Vector2 m_FirstPressPos;
        private Vector2 m_SecondPressPos;
        private Vector2 m_CurrentSwipe;
        public enum Swipe { None, Up, Down, Left, Right }

        private IViewInputCommandsProceeder m_CommandsProceeder;

        #endregion

        #region engine methods

        private void Start()
        {
            Controller.OnLogChanged += OnLogChanged;
            UpdateLogStr(Controller.Log);
            m_SwipeDragDistance = Screen.width * 30 * 0.01f;
        }

        private void OnDestroy()
        {
            Controller.OnLogChanged -= OnLogChanged;
        }

        private void Update()
        {
            if (LeanInput.GetDown(KeyCode.Escape) && m_IsVisible)
                ToggleVisibility();
            if (LeanInput.GetDown(KeyCode.Return))
                RunCommand();
            //Toggle visibility when tilde key pressed
            if (LeanInput.GetDown(KeyCode.KeypadDivide))
                ToggleVisibility();

            //Arrow up in history
            if (LeanInput.GetDown(KeyCode.UpArrow))
                UpCommand();
            if (LeanInput.GetDown(KeyCode.DownArrow))
                DownCommand();

            //Visibility on mouse swipe
            if (LeanInput.GetMouseDown(0))
                m_SwipeFirstPosition = Mouse.current.position.ReadValue();
            else if (LeanInput.GetMouseUp(0))
            {
                m_SwipeLastPosition = Mouse.current.position.ReadValue();
                if ((m_SwipeFirstPosition.x - m_SwipeLastPosition.x > m_SwipeDragDistance) && !m_IsVisible)
                    ToggleVisibility();
                if ((m_SwipeLastPosition.x - m_SwipeFirstPosition.x > m_SwipeDragDistance) && m_IsVisible)
                    ToggleVisibility();
            }
            
            if (LeanInput.GetTouchCount() > 0 )
            {
                LeanInput.GetTouch(0, out int _, out var _, out float _, out bool set);
                var t = Touchscreen.current.touches[0];
                if (set)
                    m_FirstPressPos = t.position.ReadValue();
                if (t.press.wasReleasedThisFrame)
                {
                    m_SecondPressPos = t.position.ReadValue();
                    m_CurrentSwipe = new Vector3(m_SecondPressPos.x - m_FirstPressPos.x, m_SecondPressPos.y - m_FirstPressPos.y);
                    if (m_CurrentSwipe.magnitude < m_SwipeDragDistance)
                        return;
                    m_CurrentSwipe.Normalize();
                    if (!(m_SecondPressPos == m_FirstPressPos))
                    {
                        if (Mathf.Abs(m_CurrentSwipe.x) > Mathf.Abs(m_CurrentSwipe.y))
                        {
                            if (m_CurrentSwipe.x < 0 && !m_IsVisible)
                            {
                                ToggleVisibility();
                            }
                            else if (m_CurrentSwipe.x > 0 && m_IsVisible)
                            {
                                ToggleVisibility();
                            }
                        }
                        else
                        {
                            if (m_CurrentSwipe.y < 0)
                            {
                                Dbg.Log("down");
                            }
                            else
                            {
                                Dbg.Log("up");
                            }
                        }
                    }
                }
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
            }
        }

        #endregion

        #region public methods

        public void Init(IViewInputCommandsProceeder _CommandsProceeder, IAdsManager _AdsManager, IScoreManager _ScoreManager)
        {
            m_CommandsProceeder = _CommandsProceeder;
            Controller.Init(_CommandsProceeder, _AdsManager, _ScoreManager);
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
            VisibilityChanged?.Invoke(_Visible);
            var commands = new[] {EInputCommand.ShopMenu, EInputCommand.SettingsMenu}
                .Concat(RazorMazeUtils.MoveAndRotateCommands);
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