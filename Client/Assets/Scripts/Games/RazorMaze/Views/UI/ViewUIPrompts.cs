using System.Collections;
using System.Collections.Generic;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Views.Utils;
using Lean.Localization;
using Ticker;
using TMPro;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIPrompts : IOnLevelStageChanged
    {
        
    }

    public class ViewUIPrompts : IViewUIPrompts
    {
        #region types

        private class PromptArgs
        {
            public TextMeshPro Prompt { get; set; }
            public bool NeedToHide { get; set; }
        }

        #endregion

        #region constants

        private const string KeySwipeToStartPrompt = "swipe_to_start";

        #endregion
        
        private bool m_FirstTimeLoaded = true;
        private bool m_FirstTimeStarted = true;
        private TextMeshPro m_SwipeToStartPrompt;
        private readonly Dictionary<string, PromptArgs> m_Prompts = new Dictionary<string, PromptArgs>();

        
        private IGameTicker GameTicker { get; }
        
        public ViewUIPrompts(IGameTicker _GameTicker)
        {
            GameTicker = _GameTicker;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded && m_FirstTimeLoaded)
            {
                ShowSwipeToStartPrompt();
                m_FirstTimeLoaded = false;
            }
            else if (_Args.Stage == ELevelStage.StartedOrContinued && m_FirstTimeStarted)
            {
                HideSwipeToStartPrompt();
                m_FirstTimeStarted = false;
            }
        }

        private void ShowSwipeToStartPrompt()
        {
            var go = PrefabUtilsEx.InitPrefab(
                null, "ui_labels", "swipe_to_start");
            m_SwipeToStartPrompt = go.GetCompItem<TextMeshPro>("label");
            m_SwipeToStartPrompt.text = LeanLocalization.GetTranslationText(KeySwipeToStartPrompt);
            m_SwipeToStartPrompt.color = DrawingUtils.ColorLines.SetA(0f);
            Coroutines.Run(ShowPromptCoroutine(KeySwipeToStartPrompt, m_SwipeToStartPrompt));
        }

        private void HideSwipeToStartPrompt()
        {
            m_Prompts[KeySwipeToStartPrompt].NeedToHide = true;
        }

        private IEnumerator ShowPromptCoroutine(string _Key, TextMeshPro _Text, bool _Loop = false)
        {
            const float loopTime = 1f;
            if (!_Loop)
            {
                if (!m_Prompts.ContainsKey(_Key))
                    m_Prompts.Add(_Key, new PromptArgs { Prompt = _Text});
            }
            else
            {
                if (m_Prompts[_Key].NeedToHide)
                {
                    m_Prompts[_Key].Prompt.gameObject.DestroySafe();
                    m_Prompts.Remove(_Key);
                    yield break;
                }
            }

            yield return Coroutines.Lerp(
                DrawingUtils.ColorLines.SetA(0f),
                DrawingUtils.ColorLines,
                loopTime,
                _Color => _Text.color = _Color,
                GameTicker,
                (_, __) =>
                {
                    Coroutines.Run(Coroutines.Lerp(
                        DrawingUtils.ColorLines,
                        DrawingUtils.ColorLines.SetA(0f),
                        loopTime,
                        _Color => _Text.color = _Color,
                        GameTicker,
                        (___, ____) =>
                        {
                            Coroutines.Run(ShowPromptCoroutine(_Key, _Text, true));
                        }));
                });
        }
    }
}