using TMPro;
using UICreationSystem;
using UICreationSystem.Factories;
using UICreationSystem.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MenuUI : MonoBehaviour
{
    #region attributes

    private Canvas m_canvas;
    private Resolution m_resolution;

    #endregion

    #region engine methods

    private void Start()
    {
        m_resolution = Screen.currentResolution;
        CreateCanvas();

        UiFactory.UiImage(
            UiFactory.UiRectTransform(
                m_canvas.RTransform(),
                "background",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero
            ),
            "menu_background");

        LoginPanel.CreatePanel(m_canvas);
    }


    #endregion

    public void CreateCanvas()
    {
        m_canvas = UiFactory.UiCanvas(
                   "MenuCanvas",
                   RenderMode.ScreenSpaceOverlay,
                   true,
                   0,
                   AdditionalCanvasShaderChannels.None,
                   CanvasScaler.ScaleMode.ScaleWithScreenSize,
                   new Vector2Int(1920, 1080),
                   CanvasScaler.ScreenMatchMode.Shrink,
                   0f,
                   100,
                   true,
                   GraphicRaycaster.BlockingObjects.None);
    }
}