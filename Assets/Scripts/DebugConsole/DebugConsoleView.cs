using System.Collections.Generic;
using Extensions;
using Helpers;
using UI;
using UI.Factories;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DebugConsole
{
    public class DebugConsoleView : MonoBehaviour
    {
        
#region factory

        public static GameObject Create()
        {
            Canvas canvas = UiFactory.UiCanvas(
                "Canvas",
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

            return PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(canvas.RTransform(), RtrLites.FullFill),
                "debug_console", "console");
        }

#endregion
    
    
        #region public fields

        public GameObject viewContainer; //Container for console view, should be a child of this GameObject
        public Text logTextArea;
        public InputField inputField;
        public ScrollRect textScrollbar;
        public Button enterCommand;
        public Button downCommand;
        public Button upCommand;
        public GameObject ConsoleLog;
        public GameObject ConsoleScrollBar;

        #endregion

        #region private fields

        private readonly DebugConsoleController m_DebugConsole = new DebugConsoleController();
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
            m_DebugConsole.VisibilityChanged += OnVisibilityChanged;
            m_DebugConsole.LogChanged += OnLogChanged;
            UpdateLogStr(m_DebugConsole.Log);
            m_SwipeDragDistance = Screen.width * 30 * 0.01f;
            //DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            m_DebugConsole.VisibilityChanged -= OnVisibilityChanged;
            m_DebugConsole.LogChanged -= OnLogChanged;
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

        public void UpCommand()
        {
            m_CurrentCommand++;
            m_Index = m_DebugConsole.CommandHistory.Count - m_CurrentCommand;
            if (m_Index >= 0 && m_DebugConsole.CommandHistory.Count != 0)
                inputField.text = m_DebugConsole.CommandHistory[m_Index].ToString();
            else
                m_CurrentCommand = m_DebugConsole.CommandHistory.Count;

            inputField.ActivateInputField();
            inputField.Select();
        }

        public void DownCommand()
        {
            m_CurrentCommand--;
            m_Index = m_DebugConsole.CommandHistory.Count - m_CurrentCommand;
            if (m_Index < m_DebugConsole.CommandHistory.Count)
                inputField.text = m_DebugConsole.CommandHistory[m_Index].ToString();
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
            m_DebugConsole.RunCommandString(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
            inputField.Select();
            m_CurrentCommand = 0;
        }

        #endregion

        #region private methods

        private void CreatePositions()
        {
            GameObject canvas = GameObject.Find("Canvas");
            RectTransform canvasRectTransform = canvas.RTransform();
            float screenWidth = canvasRectTransform.sizeDelta.x;
            float screenHeight = canvasRectTransform.sizeDelta.y;
            //Log
            RectTransform logAreaRectTransform = ConsoleLog.RTransform();
            logAreaRectTransform.SetLeft(0);
            logAreaRectTransform.SetRight(screenWidth - screenWidth * 0.9f);
            logAreaRectTransform.SetTop(0);
            logAreaRectTransform.SetBottom(screenHeight * 0.1f);

            //Scroll
            RectTransform scrollRectTransform = ConsoleScrollBar.RTransform();
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
            this.m_IsVisible = _Visible;
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
