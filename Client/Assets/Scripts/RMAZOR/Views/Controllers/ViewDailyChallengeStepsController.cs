
using System;
using System.Collections.Generic;
using System.Linq;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Rotation;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Controllers
{
    public interface IViewDailyChallengeStepsController
        : IViewDailyChallengeController,
          ICharacterMoveFinished,
          IMazeRotationFinished { }
    
    public class ViewDailyChallengeStepsController
        : ViewDailyChallengeControllerBase, 
          IViewDailyChallengeStepsController
    {
        #region nonpublic members
        
        protected override string IconPrefabName     => "icon_steps_game";
        protected override string DailyChallengeType => ParameterChallengeTypeSteps;

        #endregion
        
        #region inject
        
        private IViewInputCommandsProceeder CommandsProceeder { get; }

        private ViewDailyChallengeStepsController(
            IAudioManager               _AudioManager,
            IPrefabSetManager           _PrefabSetManager,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(_AudioManager, _PrefabSetManager, _LevelStageSwitcher)
        {
            CommandsProceeder = _CommandsProceeder;
        }
        
        private int  m_StartSteps;
        private int  m_CurrentSteps;
        private bool m_DoInvokeChallengeFailed;

        #endregion

        #region api

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (m_DoInvokeChallengeFailed)
                RaiseChallengeFailedEvent();
        }

        public void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            if (m_DoInvokeChallengeFailed)
                RaiseChallengeFailedEvent();
        }

        #endregion

        #region nonpublic methods

        private void OnCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            if (!DoProceed)
                return;
            if (!GetValidCommands().Contains(_Command))
                return;
            if (++m_CurrentSteps == m_StartSteps)
                m_DoInvokeChallengeFailed = true;
            int stepsLeft = m_StartSteps - m_CurrentSteps;
            GetPanelObjects().Text.text = stepsLeft.ToString("D");
        }
        
        protected override void InitController()
        {
            CommandsProceeder.Command += OnCommand;
            base.InitController();
        }

        protected override void SetGoal(object _GoalArg)
        {
            m_StartSteps = Convert.ToInt32(_GoalArg);
            m_CurrentSteps = 0;
            m_DoInvokeChallengeFailed = false;
            var panObjects = GetPanelObjects();
            panObjects.IconChallenge.transform.SetLocalPosX(1.8f);
            panObjects.Text.transform.SetLocalPosX(6.7f);
            GetPanelObjects().Text.text = m_StartSteps.ToString("D");
        }

        private static IEnumerable<EInputCommand> GetValidCommands()
        {
            return RmazorUtils.MoveAndRotateCommands;
        } 

        #endregion
    }
}