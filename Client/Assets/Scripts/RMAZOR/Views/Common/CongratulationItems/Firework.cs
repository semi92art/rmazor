using System.Collections;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Views.Utils;
using Shapes;
using SpawnPools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.Common.CongratulationItems
{
    [ExecuteInEditMode]
    public class Firework : MonoBehaviour, ISpawnPoolItem
    {
        #region serialized fields
        
        [SerializeField] private float duration;
        [SerializeField] private float force;

        #endregion

        #region nonpublic members
        
        private Disc[]          m_Discs;
        private Rigidbody[]     m_Bodies;
        private IViewGameTicker m_Ticker;
        private IColorProvider  m_ColorProvider;
        private bool            m_ActivatedInSpawnPool;
        
        private static readonly Color[] CongradColorSet =
        {
            new Color(0.72f, 1f, 0.58f),
            new Color(1f, 0.81f, 0.45f),
            new Color(0.56f, 1f, 0.86f),
            new Color(0.44f, 0.55f, 1f),
            new Color(0.91f, 0.52f, 1f),
            new Color(1f, 0.49f, 0.52f)
        };

        #endregion

        #region api
        
        public bool ActivatedInSpawnPool
        {
            get => m_ActivatedInSpawnPool;
            set
            {
                m_ActivatedInSpawnPool = value;
                if (m_Discs == null)
                    return;
                int count = m_Discs.Length;
                for (int i = 0; i < count; i++)
                    m_Bodies[i].gameObject.SetActive(value);
            }
        }
        
        public void InitFirework(
            Disc[]          _Discs,
            Rigidbody[]     _Bodies, 
            IViewGameTicker _Ticker,
            IColorProvider  _ColorProvider)
        {
            m_Discs = _Discs;
            m_Bodies = _Bodies;
            m_Ticker = _Ticker;
            m_ColorProvider = _ColorProvider;
            _ColorProvider.ColorThemeChanged += OnColorThemeChanged;
            int count = m_Discs.Length;
            for (int i = 0; i < count; i++)
            {
                m_Bodies[i].MovePosition(transform.position);
                m_Bodies[i].constraints = RigidbodyConstraints.FreezeAll;
                m_Discs[i].Color = m_Discs[i].Color.SetA(1f);
                m_Discs[i].SortingOrder = SortingOrders.BackgroundItem;
            }
        }
        
        public void LaunchFirework()
        {
            Cor.Run(LaunchFireworkCoroutine());
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorThemeChanged(EColorTheme _Theme)
        {
            foreach (var disc in m_Discs)
                SetColorByTheme(disc, _Theme);
        }
 
        private IEnumerator LaunchFireworkCoroutine()
        {
            int colIdx = Mathf.FloorToInt(Random.value * CongradColorSet.Length);
            var col = CongradColorSet[colIdx];
            static float RandDir()
            {
                return (Random.value - 0.5f) * 2f;
            }
            int count = m_Discs.Length;
            for (int i = 0; i < count; i++)
            {
                var dir = new Vector2(RandDir(), RandDir());
                m_Bodies[i].constraints = RigidbodyConstraints.FreezeRotation;
                m_Bodies[i].transform.localPosition = Vector3.zero;
                m_Bodies[i].AddForce(force * dir, ForceMode.VelocityChange);
                SetColorByTheme(m_Discs[i], m_ColorProvider.CurrentTheme, col);
            }
            yield return Cor.Lerp(
                1f,
                0f,
                duration,
                _Progress =>
                {
                    for (int i = 0; i < count; i++)
                        m_Discs[i].Color = m_Discs[i].Color.SetA(_Progress);
                },
                m_Ticker,
                (_Broken, _Progress) =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        m_Bodies[i].constraints = RigidbodyConstraints.FreezeAll;
                        m_Discs[i].Color = m_Discs[i].Color.SetA(0f);
                        ActivatedInSpawnPool = false;
                    }
                });
        }
        
        private void SetColorByTheme(ShapeRenderer _Renderer, EColorTheme _Theme, Color? _Color = null)
        {
            float saturation = _Theme == EColorTheme.Light ? 80f / 100f : 50f / 100f;
            var col = _Color ?? _Renderer.Color;
            Color.RGBToHSV(col, out float h, out _, out float v);
            col = Color.HSVToRGB(h, saturation, v);
            _Renderer.Color = col;
        }

        #endregion
    }
}