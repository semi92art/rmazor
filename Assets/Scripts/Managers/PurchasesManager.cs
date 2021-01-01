using Constants;
using Entities;
using Exceptions;
using UI.PanelItems;
using UnityEngine.Events;

namespace Managers
{
    public class PurchasesManager : GameObserver, ISingleton
    {
        #region singleton
    
        private static PurchasesManager _instance;
        public static PurchasesManager Instance => _instance ?? (_instance = new PurchasesManager());
    
        #endregion
        
        #region nonpublic methods
        
        protected override void OnNotify(object _Sender, string _NotifyMessage, params object[] _Args)
        {
            if (_NotifyMessage != CommonNotifyMessages.PurchaseCommand) return;
            if (_Args == null || _Args.Length < 2) return;
            if (!(_Args[0] is ShopItemProps props)) return;
            if (!(_Args[1] is UnityAction action)) return;

            int? purchaseId = null;

            switch (props.Type)
            {
                case ShopItemType.NoAds:
                    break;
                case ShopItemType.Money:
                    switch (props.Size)
                    {
                        case ShopItemSize.Small:
                            break;
                        case ShopItemSize.Big:
                            break;
                        default:
                            throw new InvalidEnumArgumentExceptionEx(props.Size);
                    }
                    break;
                case ShopItemType.Lifes:
                    switch (props.Size)
                    {
                        case ShopItemSize.Small:
                            break;
                        case ShopItemSize.Big:
                            break;
                        default:
                            throw new InvalidEnumArgumentExceptionEx(props.Size);
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentExceptionEx(props.Type);
            }
        }
        
        #endregion
    }
}