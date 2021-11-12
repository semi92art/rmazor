using GameHelpers;
using Games.RazorMaze.Views;
using LeTai.Asset.TranslucentImage;
using UnityEngine;

public interface ICameraProvider
{
    Camera MainCamera { get; }
}
    
public class DefaultCameraProvider : MonoBehaviour, ICameraProvider, IOnLevelStageChanged
{
    private TranslucentImageSource m_TranslucentSource;
    
    public Camera MainCamera { get; private set; }

    private void Awake()
    {
        MainCamera = Camera.main;
        m_TranslucentSource = MainCamera.GetComponent<TranslucentImageSource>();
        PrefabUtilsEx.GetObject<ScalableBlurConfig>("views", "level_blur_config").Strength = 10f;
    }

    public void OnLevelStageChanged(LevelStageArgs _Args)
    {
        m_TranslucentSource.enabled = _Args.Stage == ELevelStage.Paused;
    }
}