using System.Collections.Generic;
using System.Linq;
using Exceptions;
using GameHelpers;
using Managers;
using UnityEngine;
using TMPro;
using DI.Extensions;

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
        
        #endregion

        #region api
        
        public long         Coins        { get; private set; }
        public bool         BigWin       => true;

        public void Init(SectorMoney _SectorMoney)
        {
            Coins = _SectorMoney.count;
            string prefabSetName;
            string iconName;
            prefabSetName = "coins";
            iconName = "gold_coin_0";

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