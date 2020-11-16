using System.Collections.Generic;
using Entities;
using Helpers;
using Managers;
using UnityEngine;
using Utils;
using TMPro;

namespace MkeyFW
{
    [ExecuteInEditMode]
    public class Sector : MonoBehaviour
    {
        #region serialized fields
        
        [SerializeField] private List<GameObject> hitPrefabs;
        [SerializeField] private SpriteRenderer icon;
        [SerializeField] private TextMeshPro text; 
        
        #endregion

        #region private members
        
        private const float DestroyTime = 3f;
        private MoneyType m_MoneyType;
        
        #endregion

        #region api
        
        public long Coins { get; private set; }
        public bool BigWin 
        {
            get
            {
                switch (m_MoneyType)
                {
                    case MoneyType.Gold:
                        return Coins >= 1000000;
                    case MoneyType.Diamonds:
                        return Coins >= 100;
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        public void Init(SectorMoney _SectorMoney)
        {
            m_MoneyType = _SectorMoney.type;
            Coins = _SectorMoney.count;
            string iconName;
            switch (m_MoneyType)
            {
                case MoneyType.Gold:
                    iconName = "gold_coin_0";
                    break;
                case MoneyType.Diamonds:
                    iconName = "diamond_coin_0";
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            icon.sprite = PrefabInitializer.GetObject<Sprite>("coins", iconName);
        }
        
        
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
                            Destroy(partT.gameObject, DestroyTime);
                    }
                }
            }

            MoneyToBank();
        }
        
        #endregion

        #region engine methods
        
        private void Start()
        {
            RefreshText();
        }

        private void OnValidate()
        {
            Coins = System.Math.Max(0, Coins);
            RefreshText();
        }
        
        #endregion

        #region private methods
        
        private void RefreshText()
        {
            text.text = Coins.ToNumeric();
        }

        private void MoneyToBank()
        {
            var income = new Dictionary<MoneyType, long> {{m_MoneyType, Coins}};
            MoneyManager.Instance.PlusMoney(income);
        }
        
        #endregion
    }
}