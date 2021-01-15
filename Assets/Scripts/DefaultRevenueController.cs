using System.Collections.Generic;
using LeTai;
using Managers;

public delegate void RevenueEventHandler(BankItemType _BankItemType, long _Revenue);

public interface IRevenueController
{
    event RevenueEventHandler OnRevenueIncome;
    Dictionary<BankItemType, long> TotalRevenue { get; set; }
    void AddRevenue(BankItemType _BankItemType, long _Revenue);
}

public class DefaultRevenueController : IRevenueController
{
    public event RevenueEventHandler OnRevenueIncome;
    public Dictionary<BankItemType, long> TotalRevenue { get; set; }

    public DefaultRevenueController()
    {
        TotalRevenue = new Dictionary<BankItemType, long>();
        foreach (var mt in Utils.CommonUtils.EnumToList<BankItemType>())
            TotalRevenue.Add(mt, 0);
    }

    public void AddRevenue(BankItemType _BankItemType, long _Revenue)
    {
        if (!TotalRevenue.ContainsKey(_BankItemType))
            TotalRevenue.Add(_BankItemType, 0);
        TotalRevenue[_BankItemType] += _Revenue;
        OnRevenueIncome?.Invoke(_BankItemType, _Revenue);
    }
}
