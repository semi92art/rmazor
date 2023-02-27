using System.Collections;
using Common;
using Common.Constants;
using Common.Extensions;
using Common.Managers;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewUILevelSkippers
{
    public class ViewGameUiLevelSkipperButton : ViewGameUiLevelSkipperButtonBase
    {
        #region nonpublic members

        private Rectangle m_Background;

        #endregion
        
        #region inject

        private ViewSettings      ViewSettings     { get; }
        private IContainersGetter ContainersGetter { get; }
        private IPrefabSetManager PrefabSetManager { get; }
        private IViewGameTicker   ViewGameTicker   { get; }

        private ViewGameUiLevelSkipperButton(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            ICameraProvider             _CameraProvider,
            ILocalizationManager        _LocalizationManager,
            IColorProvider              _ColorProvider,
            IViewGameTicker             _ViewGameTicker,
            IViewBetweenLevelAdShower   _BetweenLevelAdShower,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IRewardCounter              _RewardCounter,
            IViewInputCommandsProceeder _CommandsProceeder,
            ILevelsLoader               _LevelsLoader) 
            : base(
                _Model,
                _CameraProvider,
                _ColorProvider,
                _CommandsProceeder,
                _BetweenLevelAdShower, 
                _LevelStageSwitcher, 
                _LevelsLoader,
                _RewardCounter,
                _LocalizationManager,
                _HapticsManager,
                _TouchProceeder)
        {
            ViewSettings     = _ViewSettings;
            ContainersGetter = _ContainersGetter;
            PrefabSetManager = _PrefabSetManager;
            ViewGameTicker   = _ViewGameTicker;
        }

        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color) { }
        
        protected override void OnSkipLevelButtonPressed()
        {
            ActivateButton(false);
            SkipLevel();
        }
        
        protected override void OnActiveCameraChanged(Camera _Camera)
        {
            ButtonObj.SetParent(_Camera.transform);
            var tr = ButtonObj.transform;
            string nextLevelType = (string)Model.LevelStaging.Arguments.GetSafe(
                ComInComArg.KeyNextLevelType, out _);
            bool isNextLevelBonus = nextLevelType == ComInComArg.ParameterLevelTypeBonus;
            if (!isNextLevelBonus)
            {
                ActivateButton(false);
                return;
            }
            var screenBounds = GraphicUtils.GetVisibleBounds(_Camera);
            float xPos = screenBounds.min.x + 5.5f;
            float yPos = screenBounds.max.y - 6f;
            tr.SetLocalPosXY(xPos, yPos);
        }

        protected override void InitButton()
        {
            var parent = ContainersGetter.GetContainer(ContainerNamesCommon.GameUI);
            var go = PrefabSetManager.InitPrefab(
                parent, CommonPrefabSetNames.UiGame, "skip_level_button");
            m_Background = go.GetCompItem<Rectangle>("background");
            m_Background.SetColor(ColorProvider.GetColor(ColorIds.UI))
                .SetSortingOrder(SortingOrders.GameUI);
            InitButtonCore(go);
            Text.color = Color.black;
        }
        
        protected override IEnumerator ShowButtonCountdownCoroutine()
        {
            yield return Cor.Delay(ViewSettings.skipLevelSeconds, 
                ViewGameTicker,
                () =>
            {
                ActivateButton(true);
            });
        }

        #endregion
    }
}