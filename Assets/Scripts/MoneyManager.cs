using System.Collections.Generic;
using Network;
using Network.PacketArgs;
using Network.Packets;
using UICreationSystem;
using UnityEngine;
using Utils;

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
    
    public event MoneyEventHandler OnMoneyCountChanged;
    public event IncomeEventHandler OnIncome;
    
    public Dictionary<MoneyType, int> GetMoney(bool _ForcedFromServer = false)
    {
        var result = new Dictionary<MoneyType, int>();
        if (!m_IsMoneySavedLocal || _ForcedFromServer)
        {
            var profPacket = new GetProfilePacket(new AccIdGameId
            {
                AccountId = GameClient.Instance.AccountId
            });
            profPacket.OnSuccess(() =>
            {
                result.Add(MoneyType.Gold, profPacket.Response.Gold);
                result.Add(MoneyType.Diamonds, profPacket.Response.Diamonds);
                SetMoneyLocal(result);
            }).OnFail(() =>
            {
                Debug.LogError(profPacket.Response);
                result = GetMoneyLocal();
            });
            
            GameClient.Instance.Send(profPacket);
            
            while (!profPacket.IsDone) {}
        }
        else
        {
            result = GetMoneyLocal();
        }
        
        return result;
    }
    
    public void PlusMoney(Dictionary<MoneyType, int> _Money)
    {
        var inBank = GetMoney();
        if (_Money.ContainsKey(MoneyType.Gold))
            inBank[MoneyType.Gold] += _Money[MoneyType.Gold];
        if (_Money.ContainsKey(MoneyType.Diamonds))
            inBank[MoneyType.Diamonds] += _Money[MoneyType.Diamonds];
        SetMoney(inBank);
    }

    public bool TryMinusMoney(Dictionary<MoneyType, int> _Money)
    {
        var inBank = GetMoney();
        var mts = new [] {MoneyType.Gold, MoneyType.Diamonds};
        foreach (var mt in mts)
        {
            if (!_Money.ContainsKey(mt))
                continue;
            
            if (inBank[mt] >= _Money[mt])
                inBank[mt] -= _Money[mt];
            else
                return false;
        }
        SetMoney(inBank);
        return true;
    }
    
    public void SetMoney(Dictionary<MoneyType, int> _Money)
    {
        var bank = GetMoneyLocal();
        var profPacket = new SetProfilePacket(new SetProfileRequestArgs
        {
            AccountId = GameClient.Instance.AccountId,
            Gold = _Money.ContainsKey(MoneyType.Gold) ? _Money[MoneyType.Gold] : bank[MoneyType.Gold],
            Diamonds = _Money.ContainsKey(MoneyType.Diamonds) ? _Money[MoneyType.Diamonds] : bank[MoneyType.Diamonds]
        });
        profPacket.OnFail(() => Debug.LogError(profPacket.ErrorMessage));
        
        GameClient.Instance.Send(profPacket);
        SetMoneyLocal(_Money);
    }

    public void SetIncome(Dictionary<MoneyType, int> _Money, RectTransform _From)
    {
        OnIncome?.Invoke(new IncomeEventArgs(_Money, _From));    
    }
    
    #endregion
    
    #region private methods

    private Dictionary<MoneyType, int> GetMoneyLocal()
    {
        var result = new Dictionary<MoneyType, int>
        {
            {MoneyType.Gold, SaveUtils.GetValue<int>(SaveKey.MoneyGold)},
            {MoneyType.Diamonds, SaveUtils.GetValue<int>(SaveKey.MoneyDiamonds)}
        };
        return result;
    }

    private void SetMoneyLocal(Dictionary<MoneyType, int> _Money)
    {
        if (_Money.ContainsKey(MoneyType.Gold))
            SaveUtils.PutValue(SaveKey.MoneyGold, _Money[MoneyType.Gold]);
        if (_Money.ContainsKey(MoneyType.Diamonds))
            SaveUtils.PutValue(SaveKey.MoneyDiamonds, _Money[MoneyType.Diamonds]);
        
        m_IsMoneySavedLocal = true;
        OnMoneyCountChanged?.Invoke(new MoneyEventArgs(GetMoneyLocal()));
    }
    
    #endregion
}

#region types

public delegate void MoneyEventHandler(MoneyEventArgs _Args);

public delegate void IncomeEventHandler(IncomeEventArgs _Args);

public class MoneyEventArgs
{
    public Dictionary<MoneyType, int> Money { get; }

    public MoneyEventArgs(Dictionary<MoneyType, int> _Money)
    {
        Money = _Money;
    }
}

public class IncomeEventArgs : MoneyEventArgs
{
    public RectTransform From { get; }
    
    public IncomeEventArgs(Dictionary<MoneyType, int> _Money, RectTransform _From) : base(_Money)
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