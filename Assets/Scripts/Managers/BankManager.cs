using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using GameHelpers;
using Network;
using UnityEngine;
using Utils;

namespace Managers
{
    public class BankManager : MonoBehaviour, ISingleton
    {
        #region singleton
    
        private static BankManager _instance;
        public static BankManager Instance => CommonUtils.MonoBehSingleton(ref _instance, "Money Manager");
    
        #endregion
    
        #region nonpublic members

        private const long MinMoneyCount = 0;
        private const long MaxMoneyCount = 999999999999;
    
        #endregion
    
        #region api

        public event MoneyEventHandler OnMoneyCountChanged;
        public event IncomeEventHandler OnIncome;
    
        public BankEntity GetBank(bool _ForcedFromServer = false)
        {
            var result = new BankEntity();
            var adf = new AccountDataFieldFilter(
                GameClientUtils.AccountId,
                DataFieldIds.FirstCurrency,
                DataFieldIds.SecondCurrency);
            adf.Filter(_DataFields =>
            {
                long gold = _DataFields.First(_V =>
                    _V.FieldId == DataFieldIds.FirstCurrency).ToLong();
                long diamonds = _DataFields.First(_V =>
                    _V.FieldId == DataFieldIds.SecondCurrency).ToLong();
                result.BankItems.Add(BankItemType.FirstCurrency, gold);
                result.BankItems.Add(BankItemType.SecondCurrency, diamonds);
                result.Loaded = true;
                OnMoneyCountChanged?.Invoke(new BankEventArgs(result));
            }, _ForcedFromServer);
            return result;
        }
    
        public void PlusBankItems(Dictionary<BankItemType, long> _Money)
        {
            var inBank = GetBank();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !inBank.Loaded,
                () =>
            {
                foreach (var kvp in _Money
                    .Where(_Kvp => inBank.BankItems.ContainsKey(_Kvp.Key)))
                    inBank.BankItems[kvp.Key] += _Money[kvp.Key];
                SetBank(inBank.BankItems);
            }));
        }

        public void PlusBankItems(BankItemType _BankItemType, long _Value)
        {
            var inBank = GetBank();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !inBank.Loaded,
                () =>
            {
                inBank.BankItems[_BankItemType] += _Value;
                SetBank(inBank.BankItems);
            }));
        }

        public bool TryMinusBankItems(Dictionary<BankItemType, long> _Money)
        {
            var inBank = GetBank();
            var mts = new [] {BankItemType.FirstCurrency, BankItemType.SecondCurrency};
            foreach (var mt in mts)
            {
                if (!_Money.ContainsKey(mt))
                    continue;
            
                if (inBank.BankItems[mt] >= _Money[mt])
                    inBank.BankItems[mt] -= _Money[mt];
                else
                    return false;
            }
            SetBank(inBank.BankItems);
            return true;
        }
    
        public void SetBank(Dictionary<BankItemType, long> _BankItems)
        {
            foreach (var kvp in _BankItems.ToArray())
                _BankItems[kvp.Key] = MathUtils.Clamp(kvp.Value, MinMoneyCount, MaxMoneyCount);

            var aff = new AccountDataFieldFilter(GameClientUtils.AccountId,
                DataFieldIds.FirstCurrency,
                DataFieldIds.SecondCurrency);
            
            aff.Filter(_DataFields =>
            {
                if (_BankItems.ContainsKey(BankItemType.FirstCurrency))
                    _DataFields.First(_V =>
                            _V.FieldId == DataFieldIds.FirstCurrency)
                        .SetValue(_BankItems[BankItemType.FirstCurrency])
                        .Save();
                if (_BankItems.ContainsKey(BankItemType.SecondCurrency))
                    _DataFields.First(_V => 
                            _V.FieldId == DataFieldIds.SecondCurrency)
                        .SetValue(_BankItems[BankItemType.SecondCurrency])
                        .Save();
            });
            
            var bank = new BankEntity {BankItems = _BankItems, Loaded = true};
            OnMoneyCountChanged?.Invoke(new BankEventArgs(bank));
        }

        public void SetIncome(Dictionary<BankItemType, long> _Money, RectTransform _From)
        {
            var bank = new BankEntity
            {
                BankItems = _Money,
                Loaded = true
            };
            PlusBankItems(bank.BankItems);
            OnIncome?.Invoke(new IncomeEventArgs(bank, _From));    
        }

        // public bool BankExistLocally
        // {
        //     get
        //     {
        //         
        //     }
        // }
    
        #endregion
    }

    #region types

    public delegate void MoneyEventHandler(BankEventArgs _Args);

    public delegate void IncomeEventHandler(IncomeEventArgs _Args);

    public class BankEventArgs
    {
        public BankEntity BankEntity { get; }

        public BankEventArgs(BankEntity _BankEntity)
        {
            BankEntity = _BankEntity;
        }
    }

    public class IncomeEventArgs : BankEventArgs
    {
        public RectTransform From { get; }
    
        public IncomeEventArgs(BankEntity _BankEntity, RectTransform _From) : base(_BankEntity)
        {
            From = _From;
        }
    }

    public enum BankItemType
    {
        FirstCurrency,
        SecondCurrency
    }

    #endregion
}