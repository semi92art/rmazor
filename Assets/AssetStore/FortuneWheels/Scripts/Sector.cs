using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using GameHelpers;
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

        #region nonpublic members
        
        private const float DestroyTime = 3f;
        private BankItemType m_BankItemType;
        
        #endregion

        #region api
        
        public long Coins { get; private set; }
        public BankItemType BankItemType => m_BankItemType;
        public bool BigWin 
        {
            get
            {
                switch (m_BankItemType)
                {
                    case BankItemType.Gold:
                        return Coins >= 1000000;
                    case BankItemType.Diamonds:
                        return Coins >= 100;
                    case BankItemType.Lifes:
                        return Coins >= 10;
                    default:
                        throw new SwitchCaseNotImplementedException(m_BankItemType);
                }
            }
        }

        public void Init(SectorMoney _SectorMoney)
        {
            m_BankItemType = _SectorMoney.type;
            Coins = _SectorMoney.count;
            string prefabSetName;
            string iconName;
            switch (m_BankItemType)
            {
                case BankItemType.Gold:
                    prefabSetName = "coins";
                    iconName = "gold_coin_0";
                    break;
                case BankItemType.Diamonds:
                    prefabSetName = "coins";
                    iconName = "diamond_coin_0";
                    break;
                case BankItemType.Lifes:
                    prefabSetName = "icons";
                    iconName = "icon_life";
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(m_BankItemType);
            }

            icon.sprite = PrefabUtilsEx.GetObject<Sprite>(prefabSetName, iconName);
        }
        
        /// <summary>
        /// Instantiate all prefabs and invoke hit event
        /// </summary>
        /// <param name="_Position"></param>
        public void PlayHit(Vector3 _Position)
        {
            if (hitPrefabs == null)
                return;
            
            foreach (var partT in from item in hitPrefabs 
                where item select Instantiate(item).transform)
            {
                partT.position = _Position;
                if (this && partT) 
                    Destroy(partT.gameObject, DestroyTime);
            }
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

        #region nonpublic methods
        
        private void RefreshText()
        {
            text.text = Coins.ToNumeric();
        }
        #endregion
    }
}