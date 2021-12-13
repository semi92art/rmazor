using GameHelpers;
using LeTai.Asset.TranslucentImage;
using UnityEngine;
using Zenject;

public interface ICameraProvider
{
    Camera MainCamera { get; }
    void   EnableTranslucentSource(bool _Enable);
}
    
public class DefaultCameraProvider : MonoBehaviour, ICameraProvider
{
    
    private TranslucentImageSource m_TranslucentSource;
    private IPrefabSetManager      PrefabSetManager { get; set; }
    
    [Inject]
    public void Inject(IPrefabSetManager _PrefabSetManager)
    {
        PrefabSetManager = _PrefabSetManager;
    }
    
    public Camera MainCamera { get; private set; }

    public void EnableTranslucentSource(bool _Enable)
    {
        m_TranslucentSource.enabled = _Enable;
    }
    
    private void Awake()
    {
        MainCamera = Camera.main;
        m_TranslucentSource = MainCamera.GetComponent<TranslucentImageSource>();
        PrefabSetManager.GetObject<ScalableBlurConfig>("views", "level_blur_config").Strength = 10f;
    }
}