using System.Collections.Generic;
using LeTai;
using Managers;

public delegate void RevenueEventHandler(MoneyType _MoneyType, long _Revenue);

public interface IRevenueController
{
    event RevenueEventHandler OnRevenueIncome;
    Dictionary<MoneyType, long> TotalRevenue { get; set; }
    void AddRevenue(MoneyType _MoneyType, long _Revenue);
}

public class DefaultRevenueController : IRevenueController
{
    public event RevenueEventHandler OnRevenueIncome;
    public Dictionary<MoneyType, long> TotalRevenue { get; set; }

    public DefaultRevenueController()
    {
        TotalRevenue = new Dictionary<MoneyType, long>();
        foreach (var mt in Utils.CommonUtils.EnumToList<MoneyType>())
            TotalRevenue.Add(mt, 0);
    }

    public void AddRevenue(MoneyType _MoneyType, long _Revenue)
    {
        if (!TotalRevenue.ContainsKey(_MoneyType))
            TotalRevenue.Add(_MoneyType, 0);
        TotalRevenue[_MoneyType] += _Revenue;
        OnRevenueIncome?.Invoke(_MoneyType, _Revenue);
    }
}
