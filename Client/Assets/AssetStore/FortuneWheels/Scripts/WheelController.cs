﻿using UnityEngine;
using Mkey;
using System;
using System.Collections.Generic;
using System.Linq;
using DialogViewers;
using Malee.List;
using Managers;
using UnityEngine.Events;
using Utils;

namespace MkeyFW
{
    internal enum SpinDir {Counter,  ClockWise}
    
    [Serializable]
    public class SectorMoney
    {
        public long count;
        public BankItemType type;
        public double probability;
    }
    
    public class WheelController : MonoBehaviour
    {
        #region types
        
        [Serializable]
        public class SectorsMoneyList : ReorderableArray<SectorMoney>
        { }
        
        #endregion
        
        #region serialized fields

        [Space(10, order = 0)]
        [Header("Sectors Money"), Reorderable(paginate = false)]
        [SerializeField] private SectorsMoneyList sectorsMoney;
        
        [Header("Main references")]
        [Space(10, order = 0)]
        [SerializeField] private Transform Reel;
        [SerializeField] private Animator pointerAnimator;
        [SerializeField] private LampsController lampsController;
        [SerializeField] private SpriteRenderer sectorLight;

        [Header("Spin options")]
        [Space(10, order = 0)]
        [SerializeField] private float inRotTime = 0.2f;
        [SerializeField] private float inRotAngle = 5;
        [SerializeField] private float mainRotTime = 1.0f;
        [SerializeField] private EaseAnim mainRotEase = EaseAnim.EaseLinear;
        [SerializeField] private float outRotTime = 0.2f;
        [SerializeField] private float outRotAngle = 5;
        [SerializeField] private float spinStartDelay;
        [SerializeField] private int spinSpeedMultiplier = 1;
        [SerializeField] private SpinDir spinDir = SpinDir.Counter;
   
        [Header("Lamps control")]
        [Space(10, order = 0)]
        [Tooltip("Before spin")] [SerializeField] private LampsFlash lampsFlashAtStart = LampsFlash.Random;
        [Tooltip("During spin")] [SerializeField] private LampsFlash lampsFlashDuringSpin = LampsFlash.Sequence;
        [Tooltip("After spin")] [SerializeField] private LampsFlash lampsFlashEnd = LampsFlash.All;
      
        [Header("Additional options")]
        [Space(10, order = 0)]
        [Tooltip("Sector light")] [SerializeField]
        private int lightBlinkCount = 4;
        [Tooltip("Help arrow")] [SerializeField] private int arrowBlinkCount = 2;
        [SerializeField] private AudioClip spinSound;

        [Header("Result event, after spin")]
        [Space(10, order = 0)]
        [SerializeField] private UnityEvent resultEvent;

        [Header("Simulation, only for test")] 
        [Space(10, order = 0)] [SerializeField] private bool simulate;
        [SerializeField] private int simPos;

        #endregion

		#region nonpublic members
        
        private Sector[] m_Sectors;
        private int m_Rand;
        private int m_SectorsCount;
        private float m_AngleSpeed;
        private float m_SectorAngleDeg;
        private int m_CurrSector;
        private int m_NextSector;
        private TweenSeq m_Ts;
        private TweenSeq m_LightTs;
        private AudioSource m_AudioSource;
        private float m_RotDirF;
        private IMenuDialogViewer m_MenuDialogViewer;
        private UnityAction<BankItemType, long> m_SpinFinishAction;
		#endregion
 
        #region engine methods
        
        private void OnValidate()
        {
            Validate();
        }

        private void Start()
        {
            m_Sectors = GetComponentsInChildren<Sector>();
            m_Sectors = m_Sectors.OrderBy(_S => _S.transform.GetSiblingIndex()).ToArray();

            int k = 0;
            foreach (var sector in m_Sectors)
                sector.Init(sectorsMoney[k++]);

            m_SectorsCount = (int) m_Sectors?.Length;
            if (m_SectorsCount > 0)
                m_SectorAngleDeg = 360f / m_SectorsCount;
            if (pointerAnimator)
            {
                pointerAnimator.enabled = false;
                pointerAnimator.speed = 0;
                pointerAnimator.transform.localEulerAngles = Vector3.zero;
            }
            if (lampsController) 
                lampsController.lampFlash = lampsFlashAtStart;
            UpdateRand();
            m_AudioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            UpdateRand();
        }

        private void OnDestroy()
        {
            CancelSpin();
        }
        
        #endregion 
        
        #region api

        public void Init(IMenuDialogViewer _MenuDialogViewer, UnityAction<BankItemType, long> _SpinFinishAction)
        {
            m_MenuDialogViewer = _MenuDialogViewer;
            m_SpinFinishAction = _SpinFinishAction;
        }
        
        public void StartSpin()
        {
            if (m_Ts != null)
                return;
            Dbg.Log("rand: " + m_Rand);
            m_NextSector = m_Rand;
            CancelLight();
            
            if (m_AudioSource) m_AudioSource.Stop();
            if (m_AudioSource && spinSound)
            {
                m_AudioSource.clip = spinSound;
                m_AudioSource.Play();
                m_AudioSource.loop = true;
            }
            
            RotateWheel(() =>
            {
                if (sectorLight) SectorLightShow(null);
                if (m_AudioSource) m_AudioSource.Stop();
            });
        }
        
        public long GetWin(out bool _IsBigWin)
        {
            int res = 0;
            _IsBigWin = false;
            if (m_Sectors != null && m_CurrSector >= 0 && m_CurrSector < m_Sectors.Length)
            {
                _IsBigWin = m_Sectors[m_CurrSector].BigWin;
                return m_Sectors[m_CurrSector].Coins;
            }
            return res;
        }

        #endregion
        
        #region nonpublic methods
        
        private void RotateWheel(Action _RotCallBack)
        {
            m_RotDirF = spinDir == SpinDir.ClockWise ? -1f : 1f;
            Validate();
            
            if (lampsController) 
                lampsController.lampFlash = lampsFlashDuringSpin;

            // get next reel position
            m_NextSector = !simulate ? m_NextSector : simPos;
            
            Dbg.Log($"next: {m_NextSector} ;angle: { GetAngleToNextSector(m_NextSector)}");

            // create reel rotation sequence - 4 parts  in - (continuous) - main - out
            float oldVal = 0f;
            m_Ts = new TweenSeq();
            float angleZ;


            m_Ts.Add((_CallBack) => // in rotation part
            {
                SimpleTween.Value(gameObject, 0f, inRotAngle, inRotTime)
                                  .SetOnUpdate(_Val =>
                                  {
                                      if (Reel) Reel.Rotate(0, 0, (-_Val + oldVal) *m_RotDirF);
                                      oldVal = _Val;
                                  })
                                  .AddCompleteCallBack(() =>
                                  {
                                      _CallBack?.Invoke();
                                  }).SetDelay(spinStartDelay);
            });

            m_Ts.Add((_CallBack) =>  // main rotation part
            {
                oldVal = 0f;
                pointerAnimator.enabled = true;
                spinSpeedMultiplier = Mathf.Max(0, spinSpeedMultiplier);
                angleZ = GetAngleToNextSector(m_NextSector) + 360.0f * spinSpeedMultiplier;
                SimpleTween.Value(
                        gameObject,
                        0, 
                        -(angleZ + outRotAngle + inRotAngle),
                        mainRotTime).SetOnUpdate(_Val =>
                          {
                              m_AngleSpeed = (-_Val + oldVal) * m_RotDirF;
                              if (Reel) Reel.Rotate(0, 0, m_AngleSpeed);
                              oldVal = _Val;
                              if (pointerAnimator)
                              {
                                  pointerAnimator.speed = Mathf.Abs(m_AngleSpeed);
                              }
                          })
                          .SetEase(mainRotEase)
                          .AddCompleteCallBack(() =>
                          {
                              if (pointerAnimator)
                              {
                                  pointerAnimator.enabled = false;
                                  pointerAnimator.speed = 0;
                                  pointerAnimator.transform.localEulerAngles = Vector3.zero;
                              }
                              if (lampsController) lampsController.lampFlash = lampsFlashEnd;
                              _CallBack?.Invoke();
                          });
            });

            m_Ts.Add((_CallBack) =>  // out rotation part
            {
                oldVal = 0f;
                SimpleTween.Value(
                        gameObject, 
                        0,
                        outRotAngle,
                        outRotTime).SetOnUpdate( _Val =>
                      {
                          if (Reel) Reel.Rotate(0, 0, (-_Val + oldVal)*m_RotDirF);
                          oldVal = _Val;
                      })
                      .AddCompleteCallBack(() =>
                      {
                          if (pointerAnimator)
                          {
                              pointerAnimator.transform.localEulerAngles = Vector3.zero;
                          }
                          m_CurrSector = m_NextSector;
                          CheckResult();
                          _CallBack?.Invoke();
                      });
            });
            
            m_Ts.Add((_CallBack) =>
            {
                resultEvent?.Invoke();
                _RotCallBack?.Invoke();
                m_Ts = null;
                _CallBack?.Invoke();
            });

            m_Ts.Start();
        }

        private void Validate()
        {
            mainRotTime = Mathf.Max(0.1f, mainRotTime);

            inRotTime = Mathf.Clamp(inRotTime, 0, 1f);
            inRotAngle = Mathf.Clamp(inRotAngle, 0, 10);

            outRotTime = Mathf.Clamp(outRotTime, 0, 1f);
            outRotAngle = Mathf.Clamp(outRotAngle, 0, 10);
            spinSpeedMultiplier = Mathf.Max(1, spinSpeedMultiplier);
            spinStartDelay = Mathf.Max(0, spinStartDelay);

            lightBlinkCount = Mathf.Max(0, lightBlinkCount);

            if (simulate)
            {
                m_Sectors = GetComponentsInChildren<Sector>();
                m_SectorsCount = m_Sectors?.Length ?? 0;
                simPos = Mathf.Clamp(simPos, 0, m_SectorsCount - 1);
            }
        }

        /// <summary>
        /// Return angle in degree to next symbol position in symbOrder array
        /// </summary>
        /// <param name="_NextOrderPosition"></param>
        /// <returns></returns>
        private float GetAngleToNextSector(int _NextOrderPosition)
        {
            m_RotDirF = (spinDir == SpinDir.ClockWise) ? -1f : 1f;
            return (m_CurrSector < _NextOrderPosition) ? 
                m_RotDirF* (_NextOrderPosition - m_CurrSector) * m_SectorAngleDeg : 
                (m_Sectors.Length - m_RotDirF*(m_CurrSector - _NextOrderPosition)) * m_SectorAngleDeg;
        }
        
        private void UpdateRand()
        {
            int k = 0;
            var distributions = sectorsMoney.
                ToDictionary(_S => k++, _S => _S.probability);
            var randGenerator = new WheelRandomNumberGenerator(distributions);
            m_Rand = randGenerator.GetDistributedRandomNumber();
        }

        private void CancelSpin()
        {
            if (!this)
                return;
            CancelLight();

            m_Ts?.Break();
            m_Ts = null;

            SimpleTween.Cancel(gameObject, false);
            if (pointerAnimator)
            {
                pointerAnimator.enabled = false;
                pointerAnimator.speed = 0;
                pointerAnimator.transform.localEulerAngles = Vector3.zero;
            }
        }

        private void CancelLight()
        {
            if (!this) 
                return;
            if (sectorLight)
            {
                SimpleTween.Cancel(sectorLight.gameObject, false);
                sectorLight.color = new Color(1, 1, 1, 0);
            }

            m_LightTs?.Break();
            m_LightTs = null;
        }

        /// <summary>
        /// Check result and invoke sector hit event
        /// </summary>
        private void CheckResult()
        {
            // if (m_Sectors == null || m_CurrSector < 0 || m_CurrSector >= m_Sectors.Length) 
            //     return;
            Sector s = m_Sectors[m_CurrSector];
            s.PlayHit(Reel.position);
            m_SpinFinishAction?.Invoke(s.BankItemType, s.Coins);
            Dbg.Log($"Coins: {s.Coins}; IsBigWin: {s.BigWin}");
        }

        
        private void SectorLightShow(Action _CompleteCallBack)
        {
            if (!sectorLight || m_LightTs != null)
            {
                _CompleteCallBack?.Invoke();
                return;
            }
            m_LightTs = new TweenSeq();
            float fadeTime = 0.2f;
            float delay = 0.2f;
            float stayTime = 0.2f;
            EaseAnim ease = EaseAnim.EaseInOutSine;
            GameObject gO = sectorLight.gameObject;

            for (int i = 0; i < lightBlinkCount; i++)
            {
                m_LightTs.Add(_CallBack =>
                {

                    SimpleTween.Value(gO, 0, 1, fadeTime)
                        .SetOnUpdate(_Val => { 
                            if (this && sectorLight)
                                sectorLight.color = new Color(1, 1, 1, _Val); 
                        })
                        .SetDelay(delay)
                        .SetEase(ease)
                        .AddCompleteCallBack(_CallBack);
                });

                m_LightTs.Add(_CallBack =>
                {
                    SimpleTween.Value(gO, 1, 0, fadeTime)
                        .SetOnUpdate(_Val =>
                        {
                            if (this && sectorLight) 
                                sectorLight.color = new Color(1, 1, 1, _Val);
                        })
                        .SetDelay(stayTime)
                        .SetEase(ease)
                        .AddCompleteCallBack(_CallBack);
                });
            }

            m_LightTs.Add(_CallBack =>
            {
                m_LightTs = null;
                _CompleteCallBack?.Invoke();
                _CallBack?.Invoke();
            });
            m_LightTs.Start();
        }
        
        #endregion
    }
}