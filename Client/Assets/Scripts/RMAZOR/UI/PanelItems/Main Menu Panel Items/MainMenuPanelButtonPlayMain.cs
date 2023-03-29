using System;
using System.Globalization;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using UnityEngine;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuPanelButtonPlayMain : MainMenuPanelButtonPlayBase
    {
        #region nonpublic members
        
        private Func<int> GetMainGameModeStagesTotalCount  { get; set; }
        private Func<int> GetMainGameModeCurrentStageIndex { get; set; }

        protected override SaveKey<bool> BadgeNewGameModeMustBeHidden => null;
        
        #endregion

        #region api

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            Func<int>            _GetMainGameModeStagesTotalCount,
            Func<int>            _GetMainGameModeCurrentStageIndex)
        {
            (GetMainGameModeStagesTotalCount, GetMainGameModeCurrentStageIndex)
                = (_GetMainGameModeStagesTotalCount, _GetMainGameModeCurrentStageIndex);
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            var locTextInfo = new LocTextInfo(title, ETextType.MenuUI_H1, "stage", GetTitleText);
            LocalizationManager.AddLocalization(locTextInfo);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            string stageWord = LocalizationManager.GetTranslation("stage");
            title.font = LocalizationManager.GetFont(ETextType.MenuUI_H1);
            title.text = GetTitleText(stageWord);
            int currentStageIndex = GetMainGameModeCurrentStageIndex();
            int totalStagesCount = GetMainGameModeStagesTotalCount();
            float progress = (float) currentStageIndex / totalStagesCount;
            progressBar.value = progress;
            float progressInPercents = progress * 100f;
            int progressInPercentsRounded = Mathf.FloorToInt(progressInPercents);
            progressBarText.text = progressInPercentsRounded + "%";
        }

        private string GetTitleText(string _StageWord)
        {
            int currentStageIndex = GetMainGameModeCurrentStageIndex();
            return _StageWord.ToUpper(CultureInfo.CurrentUICulture) + " " + currentStageIndex;   
        }
        
        #endregion
    }
}