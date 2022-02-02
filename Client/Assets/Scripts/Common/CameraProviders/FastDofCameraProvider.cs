using Common.Constants;
using Common.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.CameraProviders
{
    public class FastDofCameraProvider : MonoBehaviour, ICameraProvider
    {
        private FastDOF           m_Dof;

        public Camera MainCamera => Camera.main;
        public void SetDofValue(float _Value)
        {
            m_Dof.BlurAmount = _Value;
        }

        public bool DofEnabled
        {
            get => m_Dof.enabled;
            set => m_Dof.enabled = value;
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
        {
            if (_Scene.name != SceneNames.Level)
                return;
            m_Dof = MainCamera.GetComponent<FastDOF>();
            if (!m_Dof.IsNotNull()) return;
            m_Dof.enabled = false;
            m_Dof.BlurAmount = 0;
        }
    }
}