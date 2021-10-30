using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using Utils;

namespace Controllers
{
    public enum LoadingResult
    {
        Success,
        Fail
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
    
    public interface ILoadingController
    {
        void Init(UnityAction<LoadingResult> _OnFinish, List<LoadingStage> _Stages);
        void StartStage(int _Index, Func<bool> _Finished, Func<IEnumerable<string>> _Errors);
    }
    
    public class LoadingController : ILoadingController
    {
        #region nonpublic members

        private UnityAction<LoadingResult> m_OnFinish;
        private List<LoadingStage> m_Stages;
        private readonly List<string> m_CurrentMessages = new List<string>();
        private float m_Percents;
        private int m_FinishedStagesCount;
        private List<string> CurrentMessages => 
            !m_CurrentMessages.Any() ? new List<string> {"Loading"} : m_CurrentMessages;

        #endregion

        #region api

        public void Init(UnityAction<LoadingResult> _OnFinish, List<LoadingStage> _Stages)
        {
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
            stage.StageAction.Invoke();
            bool finished = false;
            bool failed = false;
            Coroutines.Run(Coroutines.WaitWhile(() => !_Finished.Invoke(),
                () =>
                {
                    m_FinishedStagesCount++;
                    m_Percents += GetPercents(stage);
                    m_CurrentMessages.Remove(stage.Description);
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
            m_OnFinish?.Invoke(LoadingResult.Fail);
        }
        
        public void FinishLoading()
        {
            m_OnFinish?.Invoke(LoadingResult.Success);
        }
        
        #endregion

        #region nonpublic methods

        private float GetPercents(LoadingStage _Stage)
        {
            return 100f * _Stage.Share / m_Stages.Sum(_S => _S.Share);
        }

        #endregion
    }
}