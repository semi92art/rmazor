using System.Collections.Generic;
using System.Linq;
using Entities;
using Network;
using Network.PacketArgs;
using Network.Packets;
using UnityEngine;
using Utils;

namespace Managers
{
    public class MoneyManager : MonoBehaviour, ISingleton
    {
        #region singleton
    
        private static MoneyManager _instance;

        public static MoneyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("Money Manager");
                    _instance = obj.AddComponent<MoneyManager>();
                }

                return _instance;
            }
        }
    
        #endregion
    
        #region private members
    
        private bool m_IsMoneySavedLocal;
    
        #endregion
    
        #region api

        public const long MaxMoneyCount = 999999999999999;
        public event MoneyEventHandler OnMoneyCountChanged;
        public event IncomeEventHandler OnIncome;
    
        public Bank GetBank(bool _ForcedFromServer = false)
        {
            var result = new Bank();
            if ((!m_IsMoneySavedLocal || _ForcedFromServer)
                && GameClient.Instance.LastConnectionSucceeded)
            {
                var profPacket = new GetProfilePacket(new AccIdGameId
                {
                    AccountId = GameClient.Instance.AccountId
                });
                profPacket.OnSuccess(() =>
                {
                    result.Money.Add(MoneyType.Gold, profPacket.Response.Gold);
                    result.Money.Add(MoneyType.Diamonds, profPacket.Response.Diamonds);
                    result.Loaded = true;
                    SetMoneyLocal(result);
                }).OnFail(() =>
                {
                    Debug.LogError(profPacket.ErrorMessage);
                    result = GetMoneyLocal(result);
                });
            
                GameClient.Instance.Send(profPacket);
            }
            else
            {
                result = GetMoneyLocal();
            }
        
            return result;
        }
    
        public void PlusMoney(Dictionary<MoneyType, long> _Money)
        {
            var inBank = GetBank();
            foreach (var kvp in _Money)
            {
                if (inBank.Money.ContainsKey(kvp.Key))
                    inBank.Money[kvp.Key] += _Money[kvp.Key];
                else
                    inBank.Money.Add(kvp.Key, kvp.Value);
            }
            
            SetMoney(inBank.Money);
        }

        public bool TryMinusMoney(Dictionary<MoneyType, int> _Money)
        {
            var inBank = GetBank();
            var mts = new [] {MoneyType.Gold, MoneyType.Diamonds};
            foreach (var mt in mts)
            {
                if (!_Money.ContainsKey(mt))
                    continue;
            
                if (inBank.Money[mt] >= _Money[mt])
                    inBank.Money[mt] -= _Money[mt];
                else
                    return false;
            }
            SetMoney(inBank.Money);
            return true;
        }
    
        public void SetMoney(Dictionary<MoneyType, long> _Money)
        {
            foreach (var kvp in _Money.ToArray())
            {
                if (kvp.Value > MaxMoneyCount)
                    _Money[kvp.Key] = MaxMoneyCount;
                else if (kvp.Value < 0)
                    _Money[kvp.Key] = 0;
            }
            var bank = GetMoneyLocal();
            if (GameClient.Instance.LastConnectionSucceeded)
            {
                var profPacket = new SetProfilePacket(new SetProfileRequestArgs
                {
                    AccountId = GameClient.Instance.AccountId,
                    Gold = _Money.ContainsKey(MoneyType.Gold) ? _Money[MoneyType.Gold] : bank.Money[MoneyType.Gold],
                    Diamonds = _Money.ContainsKey(MoneyType.Diamonds) ? _Money[MoneyType.Diamonds] : bank.Money[MoneyType.Diamonds]
                });
                profPacket.OnFail(() => Debug.LogError(profPacket.ErrorMessage));
                GameClient.Instance.Send(profPacket);    
            }
            bank.Money = _Money;
            SetMoneyLocal(bank);
        }

        public void SetIncome(Dictionary<MoneyType, long> _Money, RectTransform _From)
        {
            var bank = new Bank
            {
                Money = _Money,
                Loaded = true
            };
            PlusMoney(bank.Money);
            OnIncome?.Invoke(new IncomeEventArgs(bank, _From));    
        }
    
        #endregion
    
        #region private methods

        private Bank GetMoneyLocal(Bank _Bank = null)
        {
            var currentMoney = SaveUtils.GetValue<Dictionary<MoneyType, long>>(SaveKey.Money);
            if (_Bank == null)
                return new Bank
                {
                    Money = currentMoney,
                    Loaded = true
                };
            _Bank.Money = currentMoney;
            _Bank.Loaded = true;
            return _Bank;
        }

        private void SetMoneyLocal(Bank _Bank)
        {
            var currentMoney = SaveUtils.GetValue<Dictionary<MoneyType, long>>(SaveKey.Money);
            if (currentMoney == null)
                currentMoney = new Dictionary<MoneyType, long>();
            foreach (var key in _Bank.Money.Keys)
            {
                if (currentMoney.ContainsKey(key))
                    currentMoney[key] = _Bank.Money[key];
                else
                    currentMoney.Add(key, _Bank.Money[key]);
            }
            SaveUtils.PutValue(SaveKey.Money, currentMoney);

            m_IsMoneySavedLocal = true;
            OnMoneyCountChanged?.Invoke(new BankEventArgs(GetMoneyLocal()));
        }
    
        #endregion
    }

    #region types

    public delegate void MoneyEventHandler(BankEventArgs _Args);

    public delegate void IncomeEventHandler(IncomeEventArgs _Args);

    public class BankEventArgs
    {
        public Bank Bank { get; }

        public BankEventArgs(Bank _Bank)
        {
            Bank = _Bank;
        }
    }

    public class IncomeEventArgs : BankEventArgs
    {
        public RectTransform From { get; }
    
        public IncomeEventArgs(Bank _Bank, RectTransform _From) : base(_Bank)
        {
            From = _From;
        }
    }

    public enum MoneyType
    {
        Gold,
        Diamonds
    }

    #endregion
}