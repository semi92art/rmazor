#if YANDEX_GAMES
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Utils;
using UnityEngine.Events;
using YG;
// ReSharper disable InconsistentNaming

namespace Common.Managers.IAP
{
    public class IAP_ShopManagerYandexGames : ShopManagerBase
    {
        #region nonpublic members

        private readonly Dictionary<int, List<UnityAction>> m_OnPurchaseActionsDictDict =
            new          Dictionary<int, List<UnityAction>>();
        private readonly Dictionary<int, List<UnityAction>> m_OnDeferredActionsDictDict =
            new Dictionary<int, List<UnityAction>>();

        #endregion
        
        #region inject

        private IYandexGameActionsProvider ActionsProvider { get; }
        private IYandexGameFacade          YandexGame      { get; }

        private IAP_ShopManagerYandexGames(
            IYandexGameActionsProvider _ActionsProvider,
            IYandexGameFacade          _YandexGame)
        {
            ActionsProvider = _ActionsProvider;
            YandexGame      = _YandexGame;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            ActionsProvider.OnGetPayments          += OnGetPayments;
            ActionsProvider.OnPurchaseSuccess      += OnPurchaseSuccess;
            ActionsProvider.OnPurchaseFailed       += OnPurchaseFailed;
            ActionsProvider.OnResolveAuthorization += YandexGame.GetPaymentsDec;
        }

        public override void RestorePurchases()
        {
            // Yandex Games do not have option to restore purcheses
        }

        public override void Purchase(int _Key)
        {
            string id = GetProductId(_Key);
            YandexGame.BuyPayments(id);
        }

        public override bool RateGame()
        {
            if (YandexGame.GetEnvironmentData().reviewCanShow)
                YandexGame.ReviewShowDec(false);
            else
            {
                MazorCommonUtils.ShowAlertDialog("Упс", "Невозможно открыть всплывающее окно для оценки игры");
            }
            return true;
        }

        public override IAP_ProductInfo GetItemInfo(int _Key)
        {
            var res = new IAP_ProductInfo();
            GetProductItemInfo(_Key, ref res);
            return res;
        }

        public override void AddPurchaseAction(int _ProductKey, UnityAction _Action)
        {
            AddTransactionAction(_ProductKey, _Action, false);
        }

        public override void AddDeferredAction(int _ProductKey, UnityAction _Action)
        {
            AddTransactionAction(_ProductKey, _Action, false);
        }

        #endregion

        #region nonpublic methods
        
        private void OnGetPayments()
        {
            base.Init();
        }
        
        private static void OnPurchaseFailed(string _Id)
        {
            Dbg.LogError($"Failed to purchase product with id {_Id}");
        }

        private void OnPurchaseSuccess(string _Id)
        {
            var info = Products
                .FirstOrDefault(_P => _P.Id == _Id);
            if (info == null)
            {
                Dbg.LogError($"Product with id {_Id} does not have product");
                return;
            }
            if (!m_OnPurchaseActionsDictDict.ContainsKey(info.PurchaseKey))
            {
                Dbg.LogWarning($"Product with id {_Id} does not have purchase action");
                return ;
            }
            var actionsList = m_OnPurchaseActionsDictDict[info.PurchaseKey];
            foreach (var action in actionsList)
                action?.Invoke();
            Dbg.Log($"ProcessPurchase: PASS. Product: '{_Id}'");
        }

        private void AddTransactionAction(int _ProductKey, UnityAction _Action, bool _Deferred)
        {
            var actionsDictDict = _Deferred ?
                m_OnDeferredActionsDictDict : m_OnPurchaseActionsDictDict;
            if (!actionsDictDict.ContainsKey(_ProductKey))
                actionsDictDict.Add(_ProductKey, new List<UnityAction>());
            var actionsList = actionsDictDict[_ProductKey];
            actionsList.Add(_Action);
        }
        
        private void GetProductItemInfo(int _Key, ref IAP_ProductInfo _Args)
        {
            _Args.PurchaseKey = _Key;
            _Args.Result = () => EShopProductResult.Pending;
            if (!Initialized)
            {
                Dbg.LogWarning("Get Product Item Info Failed. Not initialized.");
                _Args.Result = () => EShopProductResult.Fail;
                return;
            }
            string id = GetProductId(_Key);
            var purchase = YandexGame.GetPurchaseById(id);
            if (purchase == null)
            {
                Dbg.LogWarning($"Get Product Item Info Failed. Product with id {id} does not exist");
                _Args.Result = () => EShopProductResult.Fail;
                return;
            }
            _Args.HasReceipt           = purchase.purchased != 0;
            _Args.Currency             = "RUB";
            if (decimal.TryParse(purchase.priceValue, out decimal priceValue))
                _Args.LocalizedPrice   = priceValue;
            _Args.LocalizedPriceString = purchase.priceValue;
            _Args.Result               = () => EShopProductResult.Success;
        }

        #endregion
    }
}
#endif