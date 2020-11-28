using System.Collections.Generic;
using Managers;

public delegate void RevenueEventHandler(MoneyType _MoneyType, long _Revenue);

public interface IRevenueController
{
    event RevenueEventHandler OnRevenueIncome;
    Dictionary<MoneyType, long> TotalRevenue { get; }
    void ClearTotalRevenue();
    void AddRevenue(MoneyType _MoneyType, long _Revenue);
}

public class DefaultRevenueController : IRevenueController
{
    public event RevenueEventHandler OnRevenueIncome;
    public Dictionary<MoneyType, long> TotalRevenue { get; }

    public DefaultRevenueController()
    {
        TotalRevenue = new Dictionary<MoneyType, long>
        {
            {MoneyType.Gold, 0},
            {MoneyType.Diamonds, 0}
        };
    }
    
    public void ClearTotalRevenue()
    {
        TotalRevenue[MoneyType.Gold] = 0;
        TotalRevenue[MoneyType.Diamonds] = 0;
    }

    public void AddRevenue(MoneyType _MoneyType, long _Revenue)
    {
        TotalRevenue[_MoneyType] += _Revenue;
        OnRevenueIncome?.Invoke(_MoneyType, _Revenue);
    }
}
