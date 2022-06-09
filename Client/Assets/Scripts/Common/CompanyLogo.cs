using System;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Managers;
using Common.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Common
{
    public class CompanyLogo : MonoBehaviour
    {
        #region serialized fields
    
        [SerializeField] private MeshRenderer   background;
        [SerializeField] private SpriteRenderer logoRend;

        #endregion

        #region nonpublic members
    
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int TimeId = Shader.PropertyToID("_TimeAlt");

        #endregion

        #region inject
    
        private IPrefabSetManager PrefabSetManager { get; set; }
        private ICameraProvider   CameraProvider   { get; set; }
    
        [Inject]
        internal void Inject(
            IPrefabSetManager _PrefabSetManager,
            ICameraProvider   _CameraProvider)
        {
            PrefabSetManager = _PrefabSetManager;
            CameraProvider   = _CameraProvider;
        }

        #endregion

        #region api
    
        private void Start()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Preload)
            {
                background.enabled = false;
                logoRend.enabled = false;
                return;
            }

            logoRend.transform.localScale = Vector3.one;
            background.enabled = true;
            logoRend.enabled = true;
            background.material = PrefabSetManager.InitObject<Material>(
                "materials", "solid_background");
            background.sharedMaterial.SetColor(ColorId, CommonData.CompanyLogoBackgroundColor);
            ScaleTextureToViewport(background.transform);
            logoRend.transform.SetLocalScaleXY(GraphicUtils.AspectRatio * Vector2.one);
        }

        private void Update()
        {
            background.sharedMaterial.SetFloat(TimeId, Time.realtimeSinceStartup);
        }

        public void HideLogo()
        {
            background.enabled = false;
            logoRend.enabled = false;
            gameObject.DestroySafe();
        }
        
        private void ScaleTextureToViewport(Transform _Transform)
        {
            var cam = CameraProvider.MainCamera;
            _Transform.position = cam.transform.position.PlusZ(20f);
            var bds = GraphicUtils.GetVisibleBounds();
            _Transform.localScale = new Vector3(bds.size.x, 1f, bds.size.y) * 0.1f;
        }

        #endregion
    }
}