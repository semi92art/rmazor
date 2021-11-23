using GameHelpers;
using LeTai.Asset.TranslucentImage;
using UnityEngine;

public interface ICameraProvider
{
    Camera MainCamera { get; }
    void   EnableTranslucentSource(bool _Enable);
}
    
public class DefaultCameraProvider : MonoBehaviour, ICameraProvider
{
    private TranslucentImageSource m_TranslucentSource;
    
    public Camera MainCamera { get; private set; }


    private void Awake()
    {
        MainCamera = Camera.main;
        m_TranslucentSource = MainCamera.GetComponent<TranslucentImageSource>();
        PrefabUtilsEx.GetObject<ScalableBlurConfig>("views", "level_blur_config").Strength = 10f;
    }

    public void EnableTranslucentSource(bool _Enable)
    {
        m_TranslucentSource.enabled = _Enable;
    }
}