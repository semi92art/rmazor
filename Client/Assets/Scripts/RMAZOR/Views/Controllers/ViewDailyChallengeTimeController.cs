using System;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Controllers
{
    public interface IViewDailyChallengeTimeController 
        : IViewDailyChallengeController { }
    
    public class ViewDailyChallengeTimeController 
        : ViewDailyChallengeControllerBase,
          IViewDailyChallengeTimeController,
          IUpdateTick
    {
        #region nonpublic members

        protected override string IconPrefabName     => "icon_timer_game";
        protected override string DailyChallengeType => ParameterChallengeTypeTimer;

        private float m_StartTime;
        private float m_CurrentTime;
        
        #endregion

        #region inject
        
        private IViewGameTicker ViewGameTicker { get; }
        private IModelGame      Model          { get; }

        private ViewDailyChallengeTimeController(
            IAudioManager           _AudioManager,
            IPrefabSetManager       _PrefabSetManager,
            IViewLevelStageSwitcher _LevelStageSwitcher,
            IViewGameTicker         _ViewGameTicker,
            IModelGame              _Model) 
            : base(_AudioManager, _PrefabSetManager, _LevelStageSwitcher)
        {
            ViewGameTicker = _ViewGameTicker;
            Model          = _Model;
        }

        #endregion

        #region api

        public void UpdateTick()
        {
            if (!DoProceed)
                return;
            float newTime = m_StartTime - Model.LevelStaging.LevelTime;
            if (!MathUtils.Equals(newTime, m_CurrentTime))
            {
                m_CurrentTime = Mathf.Max(0f, m_StartTime - Model.LevelStaging.LevelTime);
                GetPanelObjects().Text.text = m_CurrentTime.ToString("F3");
            } 
            else
                return;
            if (newTime < 0f)
                RaiseChallengeFailedEvent();
        }

        #endregion

        #region nonpublic methods

        protected override void InitController()
        {
            ViewGameTicker.Register(this);
            base.InitController();
        }

        protected override void SetGoal(object _GoalArg)
        {
            m_StartTime = Convert.ToSingle(_GoalArg);
            var panObjects = GetPanelObjects();
            panObjects.IconChallenge.transform.SetLocalPosX(0f);
            panObjects.Text.transform.SetLocalPosX(4.9f);
        }

        #endregion
    }
}