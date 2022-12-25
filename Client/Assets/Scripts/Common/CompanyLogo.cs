using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Common
{
    public class CompanyLogo : MonoBehInitBase
    {
        #region serialized fields
    
        [SerializeField] private MeshRenderer    background;
        [SerializeField] private Image           circles1, circles2;
        [SerializeField] private TextMeshProUGUI loadingText;

        #endregion

        #region nonpublic members

        private static readonly int ColorId         = Shader.PropertyToID("_Color");
        private static readonly int Color1Id        = Shader.PropertyToID("_Color1");
        private static readonly int Color2Id        = Shader.PropertyToID("_Color2");
        private static readonly int ShColor1Id      = Shader.PropertyToID("_ShColor1");
        private static readonly int ShColor2Id      = Shader.PropertyToID("_ShColor2");
        private static readonly int AuxColorId      = Shader.PropertyToID("_AuxColor");
        private static readonly int AuxOrbitColorId = Shader.PropertyToID("_AuxOrbitColor");

        #endregion

        #region inject

        private IPrefabSetManager PrefabSetManager { get; set; }
        private IViewGameTicker   Ticker           { get; set; }
    
        [Inject]
        internal void Inject(
            IPrefabSetManager _PrefabSetManager,
            IViewGameTicker _Ticker)
        {
            PrefabSetManager = _PrefabSetManager;
            Ticker = _Ticker;
        }

        #endregion

        #region api

        public override void Init()
        {
            InitLogo();
            base.Init();
        }

        public void HideLogo()
        {
            Cor.Run(Cor.Lerp(
                Ticker,
                0.5f,
                1f,
                0f,
                _P =>
                {
                    var col = new Color(1f,1f,1f,_P);
                    circles1.material.SetColor(Color1Id, col);
                    circles1.material.SetColor(Color2Id, col);
                    circles1.material.SetColor(ShColor1Id, col);
                    circles1.material.SetColor(ShColor2Id, col);
            
                    circles2.material.SetColor(ColorId, col);
                    circles2.material.SetColor(AuxOrbitColorId, col);
                    circles2.material.SetColor(AuxColorId, col);
                    circles2.material.SetColor(Color2Id, col);
                    loadingText.color = col;
                },
                () =>
                {
                    gameObject.DestroySafe();        
                }));
        }

        #endregion

        #region nonpublic methods

        private void InitLogo()
        {
            background.enabled = true;
            background.material = PrefabSetManager.InitObject<Material>(
                "materials", "background_solid");
            background.sharedMaterial.SetColor(Color1Id, MazorCommonData.CompanyLogoBackgroundColor);
            ScaleTextureToViewport(background.transform);

            var startCol = Color.white;
            circles1.material.SetColor(Color1Id, startCol);
            circles1.material.SetColor(Color2Id, startCol);
            circles1.material.SetColor(ShColor1Id, startCol);
            circles1.material.SetColor(ShColor2Id, startCol);
            
            circles2.material.SetColor(ColorId, startCol);
            circles2.material.SetColor(AuxOrbitColorId, startCol);
            circles2.material.SetColor(AuxColorId, startCol);
            circles2.material.SetColor(Color2Id, startCol);

            background.enabled = true;
            circles1.enabled = true;
            circles2.enabled = true;
            loadingText.enabled = true;
        }
        
        private static void ScaleTextureToViewport(Transform _Transform)
        {
            var cam = Camera.main;
            _Transform.position = cam!.transform.position.PlusZ(20f);
            var bds = GraphicUtils.GetVisibleBounds();
            _Transform.localScale = new Vector3(bds.size.x, 1f, bds.size.y) * 0.1f;
        }

        #endregion
    }
}