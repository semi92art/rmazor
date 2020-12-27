#if UNITY_EDITOR || DEVELOPMENT_BUILD

using System.Collections.Generic;
using Extensions;
using GameHelpers;
using Network;
using UI;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace DebugConsole
{
    public class DebugConsoleView : MonoBehaviour, ISingleton
    {
        #region singleton
        
        private static DebugConsoleView _instance;
        public static DebugConsoleView Instance => _instance.IsNull() ? _instance = Create() : _instance;

        #endregion
        
        
        #region factory

        private static DebugConsoleView Create()
        {
            Canvas canvas = UiFactory.UiCanvas(
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
            
            if (!GameClient.Instance.IsModuleTestsMode)
                DontDestroyOnLoad(canvas);

            return PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(canvas.RTransform(), RtrLites.FullFill),
                "debug_console", "console").GetComponent<DebugConsoleView>();
        }

        #endregion
    
    
        #region serialized fields

        public GameObject viewContainer; //Container for console view, should be a child of this GameObject
        public Text logTextArea;
        public InputField inputField;
        public ScrollRect textScrollbar;
        public Button enterCommand;
        public Button downCommand;
        public Button upCommand;
        public GameObject consoleLog;
        public GameObject consoleScrollBar;

        #endregion
        
        #region public members

        public DebugConsoleController Controller => m_controller;
        
        #endregion

        #region nonpublic members

        private readonly DebugConsoleController m_controller = new DebugConsoleController();
        private Vector3 m_swipeFirstPosition;
        private Vector3 m_swipeLastPosition;
        private float m_swipeDragDistance;
        private readonly List<Vector3> m_touchPositions = new List<Vector3>();
        private int m_currentCommand;
        private int m_index;
        private bool m_isVisible;
        public static Swipe swipeDirection;
 
        Vector2 m_firstPressPos;
        Vector2 m_secondPressPos;
        Vector2 m_currentSwipe;
        public enum Swipe { None, Up, Down, Left, Right };

        #endregion

        #region engine methods

        private void Start()
        {
            m_controller.VisibilityChanged += OnVisibilityChanged;
            m_controller.OnLogChanged += OnLogChanged;
            UpdateLogStr(m_controller.Log);
            m_swipeDragDistance = Screen.width * 30 * 0.01f;
        }

        private void OnDestroy()
        {
            m_controller.VisibilityChanged -= OnVisibilityChanged;
            m_controller.OnLogChanged -= OnLogChanged;
        }

        private void Update()
        {
#if UNITY_EDITOR

            if (Input.GetKeyUp(KeyCode.Return))
                RunCommand();

            //Toggle visibility when tilde key pressed
            if (Input.GetKeyUp("`"))
                ToggleVisibility();

            //Arrow up in history
            if (Input.GetKeyUp(KeyCode.UpArrow))
                UpCommand();
            if (Input.GetKeyUp(KeyCode.DownArrow))
                DownCommand();

            //Visibility on mouse swipe
            if (Input.GetMouseButtonDown(0))
                m_swipeFirstPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            else if (Input.GetMouseButtonUp(0))
            {
                m_swipeLastPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if ((m_swipeFirstPosition.x - m_swipeLastPosition.x > m_swipeDragDistance) && !m_isVisible)
                    ToggleVisibility();
                if ((m_swipeLastPosition.x - m_swipeFirstPosition.x > m_swipeDragDistance) && m_isVisible)
                    ToggleVisibility();
            }

#endif

            if (Input.touches.Length > 0)
            {
                Touch t = Input.GetTouch(0);
 
                if (t.phase == TouchPhase.Began)
                {
                    m_firstPressPos = new Vector2(t.position.x, t.position.y);
                }
 
                if (t.phase == TouchPhase.Ended)
                {
                    m_secondPressPos = new Vector2(t.position.x, t.position.y);
                    m_currentSwipe = new Vector3(m_secondPressPos.x - m_firstPressPos.x, m_secondPressPos.y - m_firstPressPos.y);
 
                    // check istap
                    if (m_currentSwipe.magnitude < m_swipeDragDistance)
                    {
                        swipeDirection = Swipe.None;
                        return;
                    }
 
                    m_currentSwipe.Normalize();
 
                    if (!(m_secondPressPos == m_firstPressPos))
                    {
                        if (Mathf.Abs(m_currentSwipe.x) > Mathf.Abs(m_currentSwipe.y))
                        {
                            if (m_currentSwipe.x < 0 && !m_isVisible)
                            {
                                ToggleVisibility();
                            }
                            else if (m_currentSwipe.x > 0 && m_isVisible)
                            {
                                ToggleVisibility();
                            }
                        }
                        else
                        {
                            if (m_currentSwipe.y < 0)
                            {
                                Debug.Log("down");
                            }
                            else
                            {
                                Debug.Log("up");
                            }
                        }
                    }
                }
                else
                {
                    swipeDirection = Swipe.None;
                }
            }

            if (inputField.touchScreenKeyboard.status == TouchScreenKeyboard.Status.Visible)
            {
                UpdatePositionsForKeyboard();
            }
            else if (inputField.touchScreenKeyboard.status == TouchScreenKeyboard.Status.Done)
            {
                CreatePositions();
                RunCommand();
            }
            else if (inputField.touchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled ||
                     inputField.touchScreenKeyboard.status == TouchScreenKeyboard.Status.LostFocus)
            {
                CreatePositions();
            }
        }

        #endregion

        #region public methods

        public void Init()
        {
            
        }

        #endregion

        #region nonpublic methods
        
        private void UpCommand()
        {
            m_currentCommand++;
            m_index = m_controller.CommandHistory.Count - m_currentCommand;
            if (m_index >= 0 && m_controller.CommandHistory.Count != 0)
                inputField.text = m_controller.CommandHistory[m_index];
            else
                m_currentCommand = m_controller.CommandHistory.Count;

            inputField.ActivateInputField();
            inputField.Select();
        }

        private void DownCommand()
        {
            m_currentCommand--;
            m_index = m_controller.CommandHistory.Count - m_currentCommand;
            if (m_index < m_controller.CommandHistory.Count)
                inputField.text = m_controller.CommandHistory[m_index];
            else
            {
                inputField.text = "";
                m_currentCommand = 0;
            }
            inputField.ActivateInputField();
            inputField.Select();
        }

        private void RunCommand()
        {
            m_controller.RunCommandString(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
            inputField.Select();
            m_currentCommand = 0;
        }

        private void CreatePositions()
        {
            var canvas = GameObject.Find("DebugConsoleCanvas");
            RectTransform canvasRectTransform = canvas.RTransform();
            float screenWidth = canvasRectTransform.sizeDelta.x;
            float screenHeight = canvasRectTransform.sizeDelta.y;
            //Log
            RectTransform logAreaRectTransform = consoleLog.RTransform();
            logAreaRectTransform.SetLeft(0);
            logAreaRectTransform.SetRight(screenWidth - screenWidth * 0.9f);
            logAreaRectTransform.SetTop(0);
            logAreaRectTransform.SetBottom(screenHeight * 0.1f);

            //Scroll
            RectTransform scrollRectTransform = consoleScrollBar.RTransform();
            scrollRectTransform.SetLeft(screenWidth - screenWidth * 0.1f);
            scrollRectTransform.SetRight(0);
            scrollRectTransform.SetTop(0);
            scrollRectTransform.SetBottom(screenHeight * 0.1f);

            //Input
            RectTransform inputRectTransform = inputField.RTransform();
            inputRectTransform.SetLeft(0);
            inputRectTransform.SetRight(screenWidth - screenWidth * 0.7f);
            inputRectTransform.SetTop(screenHeight - screenHeight * 0.1f);
            inputRectTransform.SetBottom(0);        

            //EnterButton
            RectTransform enterButtonRectTransform = enterCommand.RTransform();
            enterButtonRectTransform.SetLeft(screenWidth - screenWidth * 0.3f);
            enterButtonRectTransform.SetRight(0);
            enterButtonRectTransform.SetTop(screenHeight - screenHeight * 0.1f);
            enterButtonRectTransform.SetBottom(screenHeight * 0.05f);

            //UpButton
            RectTransform upButtonRectTransform = upCommand.RTransform();
            upButtonRectTransform.SetLeft(screenWidth - screenWidth * 0.3f);
            upButtonRectTransform.SetRight(screenWidth * 0.15f);
            upButtonRectTransform.SetTop(screenHeight - screenHeight * 0.05f);
            upButtonRectTransform.SetBottom(0);

            //DownButton
            RectTransform downButtonRectTransform = downCommand.RTransform();
            downButtonRectTransform.SetLeft(screenWidth - screenWidth * 0.15f);
            downButtonRectTransform.SetRight(0);
            downButtonRectTransform.SetTop(screenHeight - screenHeight * 0.05f);
            downButtonRectTransform.SetBottom(0);

        }

        private void UpdatePositionsForKeyboard()
        {
            var canvas = GameObject.Find("DebugConsoleCanvas");
            RectTransform canvasRectTransform = canvas.RTransform();
            float screenWidth = canvasRectTransform.sizeDelta.x;
            float screenHeight = canvasRectTransform.sizeDelta.y;
            //Log
            RectTransform logAreaRectTransform = consoleLog.RTransform();
            logAreaRectTransform.SetTop(0);
            logAreaRectTransform.SetBottom(screenHeight * 0.5f);
            //Scroll
            RectTransform scrollRectTransform = consoleScrollBar.RTransform();
            scrollRectTransform.SetTop(0);
            scrollRectTransform.SetBottom(screenHeight * 0.5f);
                
            //Input
            RectTransform inputRectTransform = inputField.RTransform();
            inputRectTransform.SetTop(screenHeight - screenHeight * 0.5f);
            inputRectTransform.SetBottom(0);        

            //EnterButton
            RectTransform enterButtonRectTransform = enterCommand.RTransform();
            enterButtonRectTransform.SetTop(screenHeight - screenHeight * 0.5f);
            enterButtonRectTransform.SetBottom(screenHeight * 0.45f);

            //UpButton
            RectTransform upButtonRectTransform = upCommand.RTransform();
            upButtonRectTransform.SetTop(screenHeight - screenHeight * 0.45f);
            upButtonRectTransform.SetBottom(screenHeight * 0.4f);

            //DownButton
            RectTransform downButtonRectTransform = downCommand.RTransform();
            downButtonRectTransform.SetTop(screenHeight - screenHeight * 0.45f);
            downButtonRectTransform.SetBottom(screenHeight * 0.4f);
        }

        private void ToggleVisibility()
        {
            SetVisibility(!viewContainer.activeSelf);
            CreatePositions();
            textScrollbar.verticalNormalizedPosition = 0f;
            inputField.ActivateInputField();
            inputField.Select();
        
        }

        private void SetVisibility(bool _Visible)
        {
            m_isVisible = _Visible;
            viewContainer.SetActive(_Visible);
            if (inputField.text == "`")
                inputField.text = "";
        }

        private void OnVisibilityChanged(bool _Visible)
        {
            SetVisibility(_Visible);
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

#endif
