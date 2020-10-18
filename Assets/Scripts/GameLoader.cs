using Clickers;
using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour
{
    public static GameLoader Instance { get; private set; }

    public GameObject consolePrefab;
    public GameObject loadingPanel;
        
    private void Start()
    {
        Instance = this;       
        GoogleAdsManager.Instance.Init();
        SceneManager.activeSceneChanged += (previous, current) =>
        {
            Canvas canvas = UiFactory.UiCanvas(
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
                
            UiFactory.UiImage(UiFactory.UiRectTransform(
                canvas.RTransform(),
                "image",
                UIAnchor.Create(0, 0, 0, 0),
                Vector2.down, Vector2.down, Vector2.down
            ), "TESTButton");
                
            if (current.buildIndex == 1)
                Instantiate(consolePrefab, canvas.RTransform());
        };
            
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene(1);
    }
}