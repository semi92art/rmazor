using System;
using System.Collections.Generic;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using UnityEngine;
using UnityEngine.Events;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Controllers
{
    public interface IViewDailyChallengeController : IInit, IOnLevelStageChanged
    {
        event UnityAction                        ChallengeFailed;
        Func<ViewGameDailyChallengePanelObjectsArgs> GetPanelObjects { set; }
    }

    public abstract class ViewDailyChallengeControllerBase : InitBase, IViewDailyChallengeController
    {
        #region nonpublic members

        protected abstract string IconPrefabName     { get; }
        protected abstract string DailyChallengeType { get; }

        private Sprite
            m_IconSpriteChallenge,
            m_IconSpriteSuccess,
            m_IconSpriteFail;
        
        protected bool DoProceed;

        #endregion

        #region inject

        private IAudioManager           AudioManager       { get; }
        private IPrefabSetManager       PrefabSetManager   { get; }
        private IViewLevelStageSwitcher LevelStageSwitcher { get; }

        protected ViewDailyChallengeControllerBase(
            IAudioManager           _AudioManager,
            IPrefabSetManager       _PrefabSetManager,
            IViewLevelStageSwitcher _LevelStageSwitcher)
        {
            AudioManager       = _AudioManager;
            PrefabSetManager   = _PrefabSetManager;
            LevelStageSwitcher = _LevelStageSwitcher;
        }

        #endregion
        
        #region api

        public event UnityAction                            ChallengeFailed;
        public Func<ViewGameDailyChallengePanelObjectsArgs> GetPanelObjects { protected get; set; }

        public override void Init()
        {
            ChallengeFailed += OnFinished;
            InitController();
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:   StartChallengeOnLevelLoad(_Args);        break;
                case ELevelStage.Finished: ShowSuccessOrFailOnLevelFinished(_Args); break;
                case ELevelStage.Unloaded: EndChallengeOnLevelUnload(_Args);        break;
            }
        }

        #endregion

        #region nonpublic methods

        protected virtual void InitController()
        {
            const string prSetName = CommonPrefabSetNames.Icons;
            m_IconSpriteChallenge  = PrefabSetManager.GetObject<Sprite>(prSetName, IconPrefabName);
            m_IconSpriteSuccess    = PrefabSetManager.GetObject<Sprite>(prSetName, "icon_success_game");
            m_IconSpriteFail       = PrefabSetManager.GetObject<Sprite>(prSetName, "icon_fail_game");
        }

        private void StartChallengeOnLevelLoad(LevelStageArgs _Args)
        {
            if (!IsChallengeOfThisType(_Args))
                return;
            GetPanelObjects().IconChallenge.sprite = m_IconSpriteChallenge;
            object goalArg = _Args.Arguments.GetSafe(KeyChallengeGoal, out _);
            SetGoal(goalArg);
            ActivatePanel(true);
            DoProceed = true;
        }

        private void ShowSuccessOrFailOnLevelFinished(LevelStageArgs _Args)
        {
            if (!IsChallengeOfThisType(_Args))
                return;
            bool isSuccess = (bool) _Args.Arguments.GetSafe(KeyIsDailyChallengeSuccess, out _);
            var panelObjects = GetPanelObjects();
            panelObjects.IconSuccessOrFail.sprite = isSuccess ? m_IconSpriteSuccess : m_IconSpriteFail;
            panelObjects.IconSuccessOrFailAnimator.SetTrigger(AnimKeys.Anim);
        }

        private void EndChallengeOnLevelUnload(LevelStageArgs _Args)
        {
            if (!IsChallengeOfThisType(_Args))
                return;
            DoProceed = false;
            ActivatePanel(false);
        }
        
        private void ActivatePanel(bool _Active)
        {
            var panelObjects = GetPanelObjects();
            panelObjects.IconChallenge.enabled             = _Active;
            panelObjects.IconSuccessOrFail.enabled         = _Active;
            panelObjects.IconSuccessOrFailAnimator.enabled = _Active;
            panelObjects.Text.enabled                      = _Active;
        }

        private bool IsChallengeOfThisType(LevelStageArgs _Args)
        {
            string challengeType = (string) _Args.Arguments.GetSafe(KeyChallengeType, out _);
            return challengeType == DailyChallengeType;
        }

        protected void RaiseChallengeFailedEvent()
        {
            ChallengeFailed?.Invoke();
        }
            
        protected abstract void SetGoal(object _GoalArg);
        
        private void OnFinished()
        {
            DoProceed = false;
            var args = new Dictionary<string, object> {{KeyIsDailyChallengeSuccess, false}};
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.FinishLevel, args);
        }
        
        #endregion
    }
}