using Constants;
using Controllers;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Controllers;
using Managers;
using Managers.Advertising;
using Managers.IAP;
using Mono_Installers;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

public class ApplicationInitializer : MonoBehaviour
{
    #region inject
    
    private CommonGameSettings   Settings            { get; set; }
    private IGameClient          GameClient          { get; set; }
    private IAdsManager          AdsManager          { get; set; }
    private IAnalyticsManager    AnalyticsManager    { get; set; }
    private ILocalizationManager LocalizationManager { get; set; }
    private ILevelsLoader        LevelsLoader        { get; set; }
    private IScoreManager        ScoreManager        { get; set; }
    private IHapticsManager      HapticsManager      { get; set; }
    private IShopManager         ShopManager         { get; set; }
    private IPrefabSetManager    PrefabSetManager    { get; set; }

    [Inject] 
    public void Inject(
        CommonGameSettings _Settings,
        IGameClient _GameClient,
        IAdsManager _AdsManager,
        IAnalyticsManager _AnalyticsManager,
        ILocalizationManager _LocalizationManager,
        ILevelsLoader _LevelsLoader,
        IScoreManager _ScoreManager,
        IHapticsManager _HapticsManager,
        IAssetBundleManager _AssetBundleManager,
        IShopManager _ShopManager,
        IPrefabSetManager _PrefabSetManager)
    {
        Settings = _Settings;
        GameClient = _GameClient;
        AdsManager = _AdsManager;
        AnalyticsManager = _AnalyticsManager;
        LocalizationManager = _LocalizationManager;
        LevelsLoader = _LevelsLoader;
        ScoreManager = _ScoreManager;
        HapticsManager = _HapticsManager;
        ShopManager = _ShopManager;
        PrefabSetManager = _PrefabSetManager;
    }


    #endregion
    
    #region engine methods
    
    private void Start()
    {
        if (Settings.SrDebuggerOn)
            CommonUtils.InitSRDebugger();
        Application.targetFrameRate = GraphicUtils.GetTargetFps();
        DataFieldsMigrator.InitDefaultDataFieldValues(GameClient);
        InitGameManagers();
        LevelMonoInstaller.Release = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(SceneNames.Level);
    }
    
    private void OnSceneLoaded(Scene _Scene, LoadSceneMode _)
    {
        if (_Scene.name.EqualsIgnoreCase(SceneNames.Level))
        {
            GameClientUtils.GameId = 1; // если игра будет только одна, то и париться с GameId нет смысла
            InitGameController();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion
    
    #region nonpublic methods
    
    private void InitGameManagers()
    {
        GameClient         .Init();
        AdsManager         .Init();
        AnalyticsManager   .Init();
        LocalizationManager.Init();
        HapticsManager     .Init();
        ScoreManager       .Initialize += OnScoreManagerInitialize;
        ScoreManager       .Init();
        // ShopManager        .Initialize += InitPurchaseActions;
        ShopManager        .Init();
    }
    
    private void InitGameController()
    {
        var controller = GameController.CreateInstance();
        controller.Initialize += () =>
        {
            var levelEntity = ScoreManager.GetScore(DataFieldIds.Level, true);
            Coroutines.Run(Coroutines.WaitWhile(
                () => levelEntity.Result == EEntityResult.Pending,
                () =>
                {
                    var levelIndex = levelEntity.GetFirstScore();
                    if (levelEntity.Result == EEntityResult.Fail
                        || !levelIndex.HasValue)
                    {
                        ScoreManager.SetScore(DataFieldIds.Level, 0, true);
                        LoadLevelByIndex(controller, 0);
                        return;
                    }
                    LoadLevelByIndex(controller, (int)levelIndex.Value);
                }));
        };
        controller.Init();
    }

    private void LoadLevelByIndex(IGameController _Controller, int _LevelIndex)
    {
        var info = LevelsLoader.LoadLevel(1, _LevelIndex);
        _Controller.Model.LevelStaging.LoadLevel(info, _LevelIndex);
    }

    private void OnScoreManagerInitialize()
    {
        const ushort id = DataFieldIds.Money;
        var moneyEntityServer = ScoreManager.GetScore(id, false);
        var moneyEntityCache = ScoreManager.GetScore(id, true);
        Coroutines.Run(Coroutines.WaitWhile(
            () =>
                moneyEntityServer.Result == EEntityResult.Pending
                || moneyEntityCache.Result == EEntityResult.Pending,
            () =>
            {
                var moneyServer = moneyEntityServer.GetFirstScore();
                if (moneyEntityServer.Result == EEntityResult.Fail
                || !moneyServer.HasValue)
                {
                    Dbg.LogWarning("Failed to load money from server");
                    return;
                }
                var moneyCache = moneyEntityCache.GetFirstScore();
                if (moneyEntityCache.Result == EEntityResult.Fail
                    || !moneyCache.HasValue)
                {
                    Dbg.LogWarning("Failed to load money from cache");
                    return;
                }

                if (moneyServer.Value > moneyCache.Value)
                    ScoreManager.SetScore(id, moneyServer.Value, true);
                else if (moneyServer.Value < moneyCache.Value)
                    ScoreManager.SetScore(id, moneyCache.Value, false);
            }));
    }
    
    // private void InitPurchaseActions()
    // {
    //     var set = PrefabSetManager.GetObject<ShopMoneyItemsScriptableObject>(
    //             "shop_money_items_set", "shop_money_panel").set;
    //     foreach (var itemInSet in set)
    //     {
    //         if (itemInSet.watchingAds)
    //             continue;
    //         ShopManager.SetOnPurchaseAction(
    //             itemInSet.purchaseKey, 
    //             () => OnGotCoinsReward(itemInSet.reward));
    //     }
    //     ShopManager.SetOnPurchaseAction(PurchaseKeys.NoAds, BuyHideAdsItem);
    // }
    //
    // private void BuyHideAdsItem()
    // {
    //     AdsManager.ShowAds = new BoolEntity
    //     {
    //         Result = EEntityResult.Success,
    //         Value = false
    //     };
    // }
    //
    // private void OnGotCoinsReward(long _Reward)
    // {
    //     var scoreEntity = ScoreManager.GetScore(DataFieldIds.Money, true);
    //     Coroutines.Run(Coroutines.WaitWhile(
    //         () => scoreEntity.Result == EEntityResult.Pending,
    //         () =>
    //         {
    //             if (scoreEntity.Result == EEntityResult.Fail)
    //             {
    //                 Dbg.LogError("Failed to load score entity");
    //                 return;
    //             }
    //             var firstVal = scoreEntity.GetFirstScore();
    //             if (!firstVal.HasValue)
    //             {
    //                 Dbg.LogError("Money score entity does not contain first value");
    //                 return;
    //             }
    //             ScoreManager.SetScore(DataFieldIds.Money, firstVal.Value + _Reward, false);
    //         }));
    // }
    
    #endregion
}