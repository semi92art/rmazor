using Common;
using Common.Helpers;
using Common.Utils;

namespace RMAZOR.Helpers
{
    public interface IMoneyCounter : IInit
    {
        long CurrentLevelMoney      { get; set; }
        long CurrentLevelGroupMoney { get; set; }
    }
    
    public class MoneyCounter : InitBase, IMoneyCounter
    {
        #region nonpublic members

        private long m_CurrentLevelGroupMoney;

        #endregion

        #region api
        
        public long CurrentLevelMoney { get; set; }

        public long CurrentLevelGroupMoney
        {
            get => m_CurrentLevelGroupMoney;
            set
            {
                SaveUtils.PutValue(SaveKeysRmazor.CurrentLevelGroupMoney, value);
                m_CurrentLevelGroupMoney = value;
            }
        }
        
        public override void Init()
        {
            m_CurrentLevelGroupMoney = SaveUtils.GetValue(SaveKeysRmazor.CurrentLevelGroupMoney);
            base.Init();
        }
        
        #endregion
    }
}