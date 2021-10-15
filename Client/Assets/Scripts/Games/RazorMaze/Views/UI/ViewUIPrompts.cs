using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Ticker;
using TMPro;
using UnityEngine;
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

        private const string KeyPromptHowToMove = "swipe_to_start";
        private const string KeyPromptHowToRotate = "touch_to_rotate";

        #endregion

        #region nonpublic members

        private bool m_HowToMovePromptShown, m_HowToMovePromptHidden;
        private bool m_HowToRotatePromptShown, m_HowToRotatePromptHidden;
        private readonly Dictionary<string, PromptArgs> m_Prompts = new Dictionary<string, PromptArgs>();


        #endregion

        #region inject

        private IModelGame Model { get; }
        private IGameTicker GameTicker { get; }
        private IContainersGetter ContainersGetter { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private ILocalizationManager LocalizationManager { get; }

        public ViewUIPrompts(
            IModelGame _Model,
            IGameTicker _GameTicker,
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter,
            ILocalizationManager _LocalizationManager)
        {
            Model = _Model;
            GameTicker = _GameTicker;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            LocalizationManager = _LocalizationManager;
        }

        #endregion

        #region api

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            System.Func<bool> mazeContainsGravityItems = () => Model.GetAllProceedInfos()
                .Any(_Info => _Info.Type == EMazeItemType.GravityBlock
                              || _Info.Type == EMazeItemType.GravityTrap);
            if (_Args.Stage == ELevelStage.ReadyToStartOrContinue)
            {
                if (!m_HowToMovePromptShown)
                {
                    ShowPromptHowToMove();
                    m_HowToMovePromptShown = true;
                }

                if (!m_HowToRotatePromptShown && mazeContainsGravityItems())
                {
                    //ShowPromptHowToRotate();
                    m_HowToRotatePromptShown = true;
                }
                
            }
            else if (_Args.Stage == ELevelStage.StartedOrContinued)
            {
                if (!m_HowToMovePromptHidden)
                {
                    HidePrompt(KeyPromptHowToMove); 
                    m_HowToMovePromptHidden = true;
                }

                if (!m_HowToRotatePromptHidden && mazeContainsGravityItems())
                {
                    //HidePrompt(KeyPromptHowToRotate);
                    m_HowToRotatePromptHidden = true;
                }
            }
        }

        #endregion

        #region nonpublic methods

        private void ShowPromptHowToMove()
        {
            var go = PrefabUtilsEx.InitPrefab(
                null, "ui_labels", "prompt_how_to_move");
            go.transform.position = CoordinateConverter.GetMazeCenter();
            var text = go.GetCompItem<TextMeshPro>("label");
            text.text = LocalizationManager.GetTranslation(KeyPromptHowToMove);
            text.color = DrawingUtils.ColorLines.SetA(0f);

            var mazeBounds = CoordinateConverter.GetMazeBounds();
            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mazeBounds.size.x);
            text.transform.position = new Vector3(
                mazeBounds.center.x, 
                mazeBounds.min.y - CoordinateConverter.GetScreenOffsets().z * 0.1f);

            ShowPrompt(KeyPromptHowToMove, text);
        }

        private void ShowPromptHowToRotate()
        {
            var go = PrefabUtilsEx.InitPrefab(
                null, "ui_labels", "prompt_how_to_rotate");
            go.transform.position = CoordinateConverter.GetMazeCenter();
            var text = go.GetCompItem<TextMeshPro>("label");
            text.text = LocalizationManager.GetTranslation(KeyPromptHowToRotate);
            text.color = DrawingUtils.ColorLines.SetA(0f);

            var mazeBounds = CoordinateConverter.GetMazeBounds();
            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mazeBounds.size.x * 0.5f);
            text.transform.position = new Vector3(
                mazeBounds.center.x, 
                mazeBounds.min.y - CoordinateConverter.GetScreenOffsets().z * 0.9f);
            
            ShowPrompt(KeyPromptHowToRotate, text);
        }
                
        private void ShowPrompt(string _Key, TextMeshPro _Text)
        {
            if (!m_Prompts.ContainsKey(_Key))
                m_Prompts.Add(_Key, new PromptArgs { Prompt = _Text });
            Coroutines.Run(ShowPromptCoroutine(_Key, _Text));
        }

        private void HidePrompt(string _Key)
        {
            if (m_Prompts.ContainsKey(_Key))
                m_Prompts[_Key].NeedToHide = true;
        }

        private IEnumerator ShowPromptCoroutine(string _Key, TextMeshPro _Text)
        {
            const float loopTime = 1f;
            if (m_Prompts[_Key].NeedToHide)
            {
                m_Prompts[_Key].Prompt.gameObject.DestroySafe();
                m_Prompts.Remove(_Key);
                yield break;
            }
            yield return Coroutines.Lerp(
                0f,
                1f,
                loopTime * 2f,
                _Progress => _Text.color = DrawingUtils.ColorLines.SetA(_Progress),
                GameTicker,
                (_, __) => Coroutines.Run(ShowPromptCoroutine(_Key, _Text)),
                _ProgressFormula: _Progress => _Progress < 0.5f ? 2f * _Progress : 2f * (1f - _Progress));
        }

        #endregion
    }
}