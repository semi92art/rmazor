using System.Collections;
using System.Linq;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Views.Utils;
using Shapes;
using SpawnPools;
using SRF;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.Common
{
    [ExecuteInEditMode]
    public class Firework : MonoBehaviour, ISpawnPoolItem
    {
        [SerializeField] private float duration;
        [SerializeField] private float force;
        
        private Disc[]          m_Discs;
        private Rigidbody[]     m_Bodies;
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
        
        public void InitFirework(Disc[] _Discs, Rigidbody[] _Bodies, IViewGameTicker _Ticker)
        {
            m_Discs = _Discs;
            m_Bodies = _Bodies;
            m_Ticker = _Ticker;
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
                // m_Bodies[i].velocity = force * dir;
                m_Discs[i].Color = col;
            }
            yield return Cor.Lerp(
                1f,
                0f,
                duration,
                _Progress =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        m_Discs[i].Color = m_Discs[i].Color.SetA(_Progress);
                    }
                },
                m_Ticker,
                (_, __) =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        m_Bodies[i].constraints = RigidbodyConstraints.FreezeAll;
                        m_Discs[i].Color = m_Discs[i].Color.SetA(0f);
                        ActivatedInSpawnPool = false;
                    }
                });
        }
    }
    
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(Firework))]
    public class FireworkEditor : UnityEditor.Editor
    {
        private Firework o;

        private void OnEnable()
        {
            o = target as Firework;
            o.ActivatedInSpawnPool = true;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Init"))
            {
                var content = o.GetCompItem<Transform>("content");
                var discs = content.GetComponentsInChildren<Disc>().ToArray();
                var bodies = content.GetComponentsInChildren<Rigidbody>().ToArray();
                var ticker = new ViewGameTicker();
                o.InitFirework(discs, bodies, ticker);
            }
            if (GUILayout.Button("Launch"))
            {
                o.LaunchFirework();
            }
        }
    }
#endif
}