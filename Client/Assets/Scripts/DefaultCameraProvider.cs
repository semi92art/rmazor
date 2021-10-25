using UnityEngine;

public interface ICameraProvider
{
    Camera MainCamera { get; }
}
    
public class DefaultCameraProvider : MonoBehaviour, ICameraProvider
{
    public Camera MainCamera { get; private set; }

    private void Awake()
    {
        MainCamera = Camera.main;
    }
}