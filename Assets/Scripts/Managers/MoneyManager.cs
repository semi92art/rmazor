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
                if (_instance is MoneyManager ptm && !ptm.IsNull())
                    return _instance;
                var go = new GameObject("Money Manager");
                _instance = go.AddComponent<MoneyManager>();
                if (!GameClient.Instance.IsTestMode)
                    DontDestroyOnLoad(go);
                return _instance;
            }
        }
    
        #endregion
    
        #region nonpublic members
    
        private bool m_IsMoneySavedLocal;
    
        #endregion
    
        #region api

        public const long MaxMoneyCount = 999999999999;
        public event MoneyEventHandler OnMoneyCountChanged;
        public event IncomeEventHandler OnIncome;
    
        public BankEntity GetBank(bool _ForcedFromServer = false)
        {
            var result = new BankEntity();
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
                    result.Money.Add(MoneyType.Lifes, profPacket.Response.Lifes);
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
                result = GetMoneyLocal();
            return result;
        }
    
        public void PlusMoney(Dictionary<MoneyType, long> _Money)
        {
            var inBank = GetBank();
            Coroutines.Run(Coroutines.WaitWhile(() =>
            {
                foreach (var kvp in _Money
                    .Where(_Kvp => inBank.Money.ContainsKey(_Kvp.Key)))
                    inBank.Money[kvp.Key] += _Money[kvp.Key];
                SetMoney(inBank.Money);
            }, () => !inBank.Loaded));
        }

        public bool TryMinusMoney(Dictionary<MoneyType, int> _Money)
        {
            var inBank = GetBank();
            var mts = new [] {MoneyType.Gold, MoneyType.Diamonds, MoneyType.Lifes};
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
                var tGold = MoneyType.Gold;
                var tDiamonds = MoneyType.Diamonds;
                var tLifes = MoneyType.Lifes;
                var profPacket = new SetProfilePacket(new SetProfileRequestArgs
                {
                    AccountId = GameClient.Instance.AccountId,
                    Gold = _Money.ContainsKey(tGold) ? _Money[tGold] : bank.Money[tGold],
                    Diamonds = _Money.ContainsKey(tDiamonds) ? _Money[tDiamonds] : bank.Money[tDiamonds],
                    Lifes = _Money.ContainsKey(tLifes) ? _Money[tLifes] : bank.Money[tLifes]
                });
                profPacket.OnFail(() => Debug.LogError(profPacket.ErrorMessage));
                GameClient.Instance.Send(profPacket);    
            }
            bank.Money = _Money;
            SetMoneyLocal(bank);
        }

        public void SetIncome(Dictionary<MoneyType, long> _Money, RectTransform _From)
        {
            var bank = new BankEntity
            {
                Money = _Money,
                Loaded = true
            };
            PlusMoney(bank.Money);
            OnIncome?.Invoke(new IncomeEventArgs(bank, _From));    
        }
    
        #endregion
    
        #region nonpublic methods

        private BankEntity GetMoneyLocal(BankEntity _BankEntity = null)
        {
            var currentMoney = SaveUtils.GetValue<Dictionary<MoneyType, long>>(SaveKey.Money);
            if (_BankEntity == null)
                return new BankEntity
                {
                    Money = currentMoney,
                    Loaded = true
                };
            _BankEntity.Money = currentMoney;
            _BankEntity.Loaded = true;
            return _BankEntity;
        }

        private void SetMoneyLocal(BankEntity _BankEntity)
        {
            var currentMoney = GetMoneyLocal().Money ?? 
                               new Dictionary<MoneyType, long>();
            foreach (var key in _BankEntity.Money.Keys)
                currentMoney.SetEvenIfNotContainKey(key, _BankEntity.Money[key]);
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

    public enum MoneyType
    {
        Gold,
        Diamonds,
        Lifes
    }

    #endregion
}