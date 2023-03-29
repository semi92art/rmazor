using System.Collections;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.SpawnPools;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

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
        private Rigidbody[]     m_RigidBodies;
        private IViewGameTicker m_Ticker;
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
                {
                    var rb = m_RigidBodies[i];
                    if (value)
                    {
                        rb.WakeUp();
                    }
                    else
                    {
                        rb.Sleep();
                        rb.transform.position = Vector3.right * 300f;
                    }
                }
            }
        }
        
        public void InitFirework(
            Disc[]          _Discs,
            Rigidbody[]     _Bodies, 
            IViewGameTicker _Ticker)
        {
            m_Discs = _Discs;
            m_RigidBodies = _Bodies;
            m_Ticker = _Ticker;
            int count = m_Discs.Length;
            for (int i = 0; i < count; i++)
            {
                m_RigidBodies[i].MovePosition(transform.position);
                m_RigidBodies[i].constraints = RigidbodyConstraints.FreezeAll;
                m_Discs[i].Color = m_Discs[i].Color.SetA(0f);
                m_Discs[i].SortingOrder = SortingOrders.BackgroundItem;
            }
        }
        
        public void LaunchFirework()
        {
            Cor.Run(LaunchFireworkCoroutine());
        }

        #endregion

        #region nonpublic methods

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
                m_RigidBodies[i].constraints = RigidbodyConstraints.FreezeRotation;
                m_RigidBodies[i].transform.localPosition = Vector3.zero;
                m_RigidBodies[i].AddForce(force * dir, ForceMode.VelocityChange);
                SetColorByTheme(m_Discs[i], col);
            }
            yield return Cor.Lerp(
                m_Ticker,
                duration,
                1f,
                0f,
                _Progress =>
                {
                    for (int i = 0; i < count; i++)
                        m_Discs[i].Color = m_Discs[i].Color.SetA(_Progress);
                },
                () =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        m_RigidBodies[i].constraints = RigidbodyConstraints.FreezeAll;
                        m_Discs[i].Color = m_Discs[i].Color.SetA(0f);
                        ActivatedInSpawnPool = false;
                    }
                });
        }
        
        private void SetColorByTheme(ShapeRenderer _Renderer, Color? _Color = null)
        {
            const float saturation = 80f / 100f;
            var col = _Color ?? _Renderer.Color;
            Color.RGBToHSV(col, out float h, out _, out float v);
            col = Color.HSVToRGB(h, saturation, v);
            _Renderer.Color = col;
        }

        #endregion
    }
}