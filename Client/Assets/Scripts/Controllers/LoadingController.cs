using System;
using System.Collections.Generic;
using System.Linq;
using UI.Panels;
using UnityEngine.Events;
using Utils;

namespace Controllers
{
    public class LoadingController
    {
        private readonly ILoadingHandler m_LoadingHandler;
        private readonly UnityAction<LoadingResult> m_OnFinish;
        private readonly List<LoadingStage> m_Stages;
        private readonly List<string> m_CurrentMessages = new List<string>();
        private float m_Percents;
        private int m_FinishedStagesCount;

        private List<string> CurrentMessages => 
            !m_CurrentMessages.Any() ? new List<string> {"Loading"} : m_CurrentMessages;

        public LoadingController(
            ILoadingHandler _LoadingHandler,
            UnityAction<LoadingResult> _OnFinish,
            List<LoadingStage> _Stages)
        {
            m_LoadingHandler = _LoadingHandler;
            m_OnFinish = _OnFinish;
            m_Stages = _Stages;
        }

        public void StartStage(int _Index, Func<bool> _Finished, Func<IEnumerable<string>> _Errors)
        {
            if (_Finished == null)
                return;
            var stage = m_Stages.First(_S => _S.Index == _Index);
            if (stage.StageAction == null)
                return;
            m_CurrentMessages.Add(stage.Description);
            m_LoadingHandler.SetProgress(m_Percents, CurrentMessages);
            stage.StageAction.Invoke();
            bool finished = false;
            bool failed = false;
            Coroutines.Run(Coroutines.WaitWhile(() => !_Finished.Invoke(),
                () =>
                {
                    m_FinishedStagesCount++;
                    m_Percents += GetPercents(stage);
                    m_CurrentMessages.Remove(stage.Description);
                    m_LoadingHandler.SetProgress(m_Percents, CurrentMessages);
                    finished = true;
                    if (m_FinishedStagesCount == m_Stages.Count)
                        FinishLoading();
                }, () => failed));
            if (_Errors == null)
                return;
            Coroutines.Run(Coroutines.WaitWhile(() =>
                {
                    var errors = _Errors.Invoke();
                    return errors == null || !errors.Any();
                },
                () =>
                {
                    string error = _Errors.Invoke().FirstOrDefault();
                    BreakLoading(error);
                    failed = true;
                }));
        }
        
        public void BreakLoading(string _Error)
        {
            m_LoadingHandler.Break(_Error);
            m_OnFinish?.Invoke(LoadingResult.Fail);
        }
        
        public void FinishLoading()
        {
            m_LoadingHandler.Break(null);
            m_OnFinish?.Invoke(LoadingResult.Success);
        }
        
        private float GetPercents(LoadingStage _Stage) => 
            100f * _Stage.Share / m_Stages.Sum(_S => _S.Share);
    }

    public class LoadingStage
    {
        public int Index { get; }
        public string Description { get; }
        public float Share { get; }
        public UnityAction StageAction { get; }

        public LoadingStage(int _Index, string _Description, float _Share, UnityAction _StageAction)
        {
            Index = _Index;
            Description = _Description;
            Share = _Share;
            StageAction = _StageAction;
        }
    }

    public enum LoadingResult
    {
        Success,
        Fail
    }
}