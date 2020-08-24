using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Clickers
{
    public class GameLoader : MonoBehaviour
    {
        public GameObject consolePrefab;
        
        private void Start()
        {
            GoogleAdsManager.Instance.Init();
            SceneManager.activeSceneChanged += (previous, current) =>
            {
                Canvas canvas = UIFactory.UICanvas(
                    "Canvas",
                    RenderMode.ScreenSpaceOverlay,
                    true,
                    0,
                    AdditionalCanvasShaderChannels.None,
                    CanvasScaler.ScaleMode.ScaleWithScreenSize,
                    new Vector2Int(1920,1080),
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
                
                if (current.buildIndex == 1)
                    Instantiate(consolePrefab, canvas.rectTransform());
            };
            
            SceneManager.LoadScene(1);
        }
    }
}