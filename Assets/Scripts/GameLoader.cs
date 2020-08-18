using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Clickers
{
    public class GameLoader : MonoBehaviour
    {
        private void Start()
        {
            GoogleAdsManager.Instance.Init();
            SceneManager.activeSceneChanged += (previous, current) =>
            {
                Canvas canvas = UIFactory.UICanvas(
                    "0",
                    RenderMode.ScreenSpaceOverlay,
                    true,
                    0,
                    AdditionalCanvasShaderChannels.None,
                    CanvasScaler.ScaleMode.ScaleWithScreenSize,
                    Vector2Int.one,
                    CanvasScaler.ScreenMatchMode.Shrink,
                    0f,
                    100,
                    true,
                    GraphicRaycaster.BlockingObjects.None);
                
                UIFactory.UIImage(UIFactory.UIRectTransform(
                    canvas.rectTransform(),
                    "image",
                    UIAnchor.Create(0, 0, 0, 0),
                    Vector2.down, Vector2.down, Vector2.down
                    ), "TESTButton");
            };
            
            SceneManager.LoadScene("Menu");
        }
    }
}