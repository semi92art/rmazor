using System.Collections.Generic;
using Entities;
using Managers;
using UnityEngine;
using Utils;

namespace MkeyFW
{
    [ExecuteInEditMode]
    public class Sector : MonoBehaviour
    {
        #region serialized fields
        
        [SerializeField] private int coins;
        [SerializeField] private bool bigWin;
        [SerializeField] private List<GameObject> hitPrefabs;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private MoneyType moneyType;
        
        #endregion

        #region private members
        
        private TextMesh m_TextMesh;
        private float m_DestroyTime = 3f;
        
        #endregion

        #region api

        public AudioClip HitSound => hitSound;
        public int Coins => coins;
        public bool BigWin => bigWin;
        
        /// <summary>
        /// Instantiate all prefabs and invoke hit event
        /// </summary>
        /// <param name="_Position"></param>
        public void PlayHit(Vector3 _Position)
        {
            if (hitPrefabs != null)
            {
                foreach (var item in hitPrefabs)
                {
                    if (item)
                    {
                        Transform partT = Instantiate(item).transform;
                        partT.position = _Position;
                        if (this && partT) 
                            Destroy(partT.gameObject, m_DestroyTime);
                    }
                }
            }

            MoneyToBank();
            SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, System.DateTime.Now.Date);
            // if (hitSound) AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
        
        #endregion

        #region engine methods
        
        private void Start()
        {
            m_TextMesh = GetComponent<TextMesh>();
            RefreshText();
        }

        private void OnValidate()
        {
           coins = Mathf.Max(0, coins);
           RefreshText();
        }
        
        #endregion

        #region private methods
        
        private void RefreshText()
        {
            if (!m_TextMesh) m_TextMesh = GetComponent<TextMesh>();
            if (!m_TextMesh) return;
            m_TextMesh.text = coins.ToNumeric();
        }

        private void MoneyToBank()
        {
            var income = new Dictionary<MoneyType, int>();
            income.Add(moneyType, coins);
            MoneyManager.Instance.PlusMoney(income);
        }
        
        #endregion
    }
}