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

        public DebugConsoleController Controller => m_Controller;
        
        #endregion

        #region nonpublic members

        private readonly DebugConsoleController m_Controller = new DebugConsoleController();
        private Vector3 m_SwipeFirstPosition;
        private Vector3 m_SwipeLastPosition;
        private float m_SwipeDragDistance;
        private readonly List<Vector3> m_TouchPositions = new List<Vector3>();
        private int m_CurrentCommand;
        private int m_Index;
        private bool m_IsVisible;
        

        #endregion

        #region engine methods

        private void Start()
        {
            m_Controller.VisibilityChanged += OnVisibilityChanged;
            m_Controller.OnLogChanged += OnLogChanged;
            UpdateLogStr(m_Controller.Log);
            m_SwipeDragDistance = Screen.width * 30 * 0.01f;
        }

        private void OnDestroy()
        {
            m_Controller.VisibilityChanged -= OnVisibilityChanged;
            m_Controller.OnLogChanged -= OnLogChanged;
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
                m_SwipeFirstPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            else if (Input.GetMouseButtonUp(0))
            {
                m_SwipeLastPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if ((m_SwipeFirstPosition.x - m_SwipeLastPosition.x > m_SwipeDragDistance) && !m_IsVisible)
                    ToggleVisibility();
                if ((m_SwipeLastPosition.x - m_SwipeFirstPosition.x > m_SwipeDragDistance) && m_IsVisible)
                    ToggleVisibility();
            }

#endif

            //Toggle visibility when right swipe
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Moved)
                    m_TouchPositions.Add(touch.position);

                if (touch.phase == TouchPhase.Ended)
                {
                    m_SwipeFirstPosition = m_TouchPositions[0];
                    m_SwipeLastPosition = m_TouchPositions[m_TouchPositions.Count - 1];

                    //if swipeDragDistance > 30% from screen edge
                    if ((m_SwipeLastPosition.x - m_SwipeFirstPosition.x > m_SwipeDragDistance) && m_IsVisible)
                        ToggleVisibility();
                }
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
            m_CurrentCommand++;
            m_Index = m_Controller.CommandHistory.Count - m_CurrentCommand;
            if (m_Index >= 0 && m_Controller.CommandHistory.Count != 0)
                inputField.text = m_Controller.CommandHistory[m_Index];
            else
                m_CurrentCommand = m_Controller.CommandHistory.Count;

            inputField.ActivateInputField();
            inputField.Select();
        }

        private void DownCommand()
        {
            m_CurrentCommand--;
            m_Index = m_Controller.CommandHistory.Count - m_CurrentCommand;
            if (m_Index < m_Controller.CommandHistory.Count)
                inputField.text = m_Controller.CommandHistory[m_Index];
            else
            {
                inputField.text = "";
                m_CurrentCommand = 0;
            }
            inputField.ActivateInputField();
            inputField.Select();
        }

        private void RunCommand()
        {
            m_Controller.RunCommandString(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
            inputField.Select();
            m_CurrentCommand = 0;
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
            m_IsVisible = _Visible;
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
