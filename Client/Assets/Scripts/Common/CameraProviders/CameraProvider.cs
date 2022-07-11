using Common.CameraProviders.Camera_Effects_Props;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Common.CameraProviders
{
    public class CameraProvider : MonoBehaviour, ICameraProvider
    {
        #region constants

        private const string PrefabSetName = "camera_effects";
        
        #endregion
        
        #region nonpublic members

        private bool                m_LevelCameraInitialized;
        private GameObject          m_LevelCameraObj;
        private Camera              m_LevelCamera;
        private FastDOF             m_DepthOfField;
        private FastGlitch          m_FastGlitch;
        private ChromaticAberration m_ChromaticAberration;
        private ColorGrading        m_ColorGrading;
        private Pixellate           m_Pixelate;
        private FXAA                m_Fxaa;
        private string              m_CurrentLevelName;

        #endregion

        #region inject
        
        private IPrefabSetManager PrefabSetManager { get; set; }
        
        [Inject] 
        public void Inject(IPrefabSetManager _PrefabSetManager)
        {
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api

        public Camera MainCamera => m_LevelCameraInitialized && m_CurrentLevelName == SceneNames.Level ? 
            m_LevelCamera : Camera.main;

        public void SetEffectParameters<T>(ECameraEffect _Effect, T _Args) where T : ICameraEffectProps
        {
            switch (_Effect)
            {
                case ECameraEffect.DepthOfField:
                {
                    var props = _Args as FastDofProps;
                    if (props!.BlurAmount.HasValue)
                        m_DepthOfField.BlurAmount = props.BlurAmount.Value;
                }
                    break;
                case ECameraEffect.Glitch:
                {
                    var props = _Args as FastGlitchProps;
                    if (props!.ChromaticGlitch.HasValue) m_FastGlitch.ChromaticGlitch = props.ChromaticGlitch.Value;
                    if (props.FrameGlitch.HasValue)      m_FastGlitch.FrameGlitch     = props.FrameGlitch.Value;
                    if (props.PixelGlitch.HasValue)      m_FastGlitch.PixelGlitch     = props.PixelGlitch.Value;
                }
                    break;
                case ECameraEffect.ColorGrading:
                {
                    var props = _Args as ColorGradingProps;
                    if (props!.Color.HasValue)            m_ColorGrading.Color            = props.Color.Value;
                    if (props.Hue.HasValue)               m_ColorGrading.Hue              = props.Hue.Value;
                    if (props.Contrast.HasValue)          m_ColorGrading.Contrast         = props.Contrast.Value;
                    if (props.Brightness.HasValue)        m_ColorGrading.Brightness       = props.Brightness.Value;
                    if (props.Saturation.HasValue)        m_ColorGrading.Saturation       = props.Saturation.Value;
                    if (props.Exposure.HasValue)          m_ColorGrading.Exposure         = props.Exposure.Value;
                    if (props.Gamma.HasValue)             m_ColorGrading.Gamma            = props.Gamma.Value;
                    if (props.Sharpness.HasValue)         m_ColorGrading.Sharpness        = props.Sharpness.Value;
                    if (props.Blur.HasValue)              m_ColorGrading.Blur             = props.Blur.Value;
                    if (props.VignetteColor.HasValue)     m_ColorGrading.VignetteColor    = props.VignetteColor.Value;
                    if (props.VignetteSoftness.HasValue)  m_ColorGrading.VignetteSoftness = props.VignetteSoftness.Value;
                    if (props.VignetteAmount.HasValue)    m_ColorGrading.VignetteAmount   = props.VignetteAmount.Value;
                }
                    break;
                case ECameraEffect.ChromaticAberration:
                {
                    var props = _Args as ChromaticAberrationProps;
                    if (props!.RedX.HasValue) m_ChromaticAberration.RedX  = props.RedX.Value;
                    if (props.RedY.HasValue)  m_ChromaticAberration.RedY  = props.RedY.Value;
                    if (props.BlueX.HasValue) m_ChromaticAberration.BlueX = props.BlueX.Value;
                    if (props.BlueY.HasValue) m_ChromaticAberration.BlueY = props.BlueY.Value;
                    if (props.FishEyeDistortion.HasValue)
                        m_ChromaticAberration.FishEyeDistortion = props.FishEyeDistortion.Value;
                }
                    break;
                case ECameraEffect.AntiAliasing:
                {
                    var props = _Args as FxaaProps;
                    if (props!.Sharpness.HasValue) m_Fxaa.Sharpness = props.Sharpness.Value;
                    if (props.Threshold.HasValue)  m_Fxaa.Sharpness = props.Threshold.Value;
                }
                    break;
                case ECameraEffect.Pixelate:
                {
                    var props = _Args as PixelateProps;
                    if (props!.Density.HasValue) m_Pixelate.Dencity = props.Density.Value;
                }
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Effect);
            }
        }

        public bool IsEffectEnabled(ECameraEffect _Effect)
        {
            return _Effect switch
            {
                ECameraEffect.DepthOfField        => m_DepthOfField.enabled,
                ECameraEffect.Glitch              => m_FastGlitch.enabled,
                ECameraEffect.ColorGrading        => m_ColorGrading.enabled,
                ECameraEffect.ChromaticAberration => m_ChromaticAberration.enabled,
                ECameraEffect.AntiAliasing        => m_Fxaa.enabled,
                ECameraEffect.Pixelate            => m_Pixelate.enabled,
                _                                 => throw new SwitchCaseNotImplementedException(_Effect)
            };
        }

        public void EnableEffect(ECameraEffect _Effect, bool _Enabled)
        {
            switch (_Effect)
            {
                case ECameraEffect.DepthOfField:        m_DepthOfField.enabled        = _Enabled; break;
                case ECameraEffect.Glitch:              m_FastGlitch.enabled          = _Enabled; break;
                case ECameraEffect.ColorGrading:        m_ColorGrading.enabled        = _Enabled; break;
                case ECameraEffect.ChromaticAberration: m_ChromaticAberration.enabled = _Enabled; break;
                case ECameraEffect.AntiAliasing:        m_Fxaa.enabled                = _Enabled; break;
                case ECameraEffect.Pixelate:            m_Pixelate.enabled            = _Enabled; break;
                default:
                    throw new SwitchCaseNotImplementedException(_Effect);
            }
        }

        #endregion

        #region nonpublic methods

        private void InitLevelCamera()
        {
            var obj = PrefabSetManager.InitPrefab(null, PrefabSetName, "level_camera");
            m_LevelCamera = obj.GetComponent<Camera>();
        }

        private void InitLevelCameraEffectComponents()
        {
            InitDepthOfField();
            InitGlitch();
            InitChromaticAberration();
            InitColorGrading();
            InitFxaa();
            InitPixelate();
        }

        private void InitDepthOfField()
        {
            m_DepthOfField = m_LevelCamera.GetCompItem<FastDOF>("fast_dof");
            m_DepthOfField.enabled = false;
            m_DepthOfField.BlurAmount = 0;
        }

        private void InitGlitch()
        {
            m_FastGlitch = m_LevelCamera.GetCompItem<FastGlitch>("fast_glitch");
            m_FastGlitch.enabled = false;
            m_FastGlitch.material = PrefabSetManager.InitObject<Material>(
                PrefabSetName, "fast_glitch_material");
        }

        private void InitChromaticAberration()
        {
            m_ChromaticAberration = m_LevelCamera.GetCompItem<ChromaticAberration>("chromatic_aberration");
            m_ChromaticAberration.enabled = false;
        }

        private void InitColorGrading()
        {
            m_ColorGrading = m_LevelCamera.GetCompItem<ColorGrading>("color_grading");
            m_ColorGrading.enabled = false;
        }

        private void InitFxaa()
        {
            m_Fxaa = m_LevelCamera.GetCompItem<FXAA>("fxaa");
            m_Fxaa.enabled = false;
        }

        private void InitPixelate()
        {
            m_Pixelate = m_LevelCamera.GetCompItem<Pixellate>("pixelate");
            m_Pixelate.enabled = false;
        }
        
        #endregion

        #region engine methods
        
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
            m_CurrentLevelName = _Scene.name;
            if (m_CurrentLevelName != SceneNames.Level)
                return;
            InitLevelCamera();
            InitLevelCameraEffectComponents();
        }

        #endregion
    }
}