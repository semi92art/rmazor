using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Extensions;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.UI.Game_Logo
{
    public class ViewUIGameLogoBladyMaze2 : InitBase, IViewUIGameLogo
    {
        #region constants

        private const float HideBackgroundTime = 0.3f;

        #endregion

        #region nonpublic members

        private static int AnimKeyGameLogoShow        => AnimKeys.Anim2;
        private static int AnimKeyGameLogoStartHiding => AnimKeys.Anim3;
        
        private static Dictionary<string, float> GameLogoPartsDelaysDict => new Dictionary<string, float>
        {
            {"top_text_animator", 0.5f},
            {"bottom_line_animator", 0.8f},
            {"bottom_text_animator", 1.0f}
        };

        private List<Component> m_Renderers;

        private GameObject                   m_GameLogoObj;
        private bool                         m_GameLogoAppeared;
        private Dictionary<string, Animator> m_GameLogoCharAnims;
        private bool                         m_OnStart = true;
        
        #endregion
        
        #region inject

        private ICameraProvider                  CameraProvider      { get; }
        private IViewInputCommandsProceeder      CommandsProceeder   { get; }
        private IColorProvider                   ColorProvider       { get; }
        private IViewGameTicker                  GameTicker          { get; }
        private IPrefabSetManager                PrefabSetManager    { get; }
        private IViewMazeGameLogoTextureProvider LogoTextureProvider { get; }

        private ViewUIGameLogoBladyMaze2(
            ICameraProvider                  _CameraProvider,
            IViewInputCommandsProceeder      _CommandsProceeder,
            IColorProvider                   _ColorProvider,
            IViewGameTicker                  _GameTicker,
            IPrefabSetManager                _PrefabSetManager,
            IViewMazeGameLogoTextureProvider _LogoTextureProvider)
        {
            CameraProvider      = _CameraProvider;
            CommandsProceeder   = _CommandsProceeder;
            ColorProvider       = _ColorProvider;
            GameTicker          = _GameTicker;
            PrefabSetManager    = _PrefabSetManager;
            LogoTextureProvider = _LogoTextureProvider;
        }

        #endregion

        #region api

        public bool              WasShown { get; private set; }
        public event UnityAction Shown;
        
        public override void Init()
        {
            if (Initialized)
                return;
            CommandsProceeder.Command          += OnCommand;
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            InitGameLogo();
            base.Init();
        }

        private void OnActiveCameraChanged(Camera _Camera)
        {
            m_GameLogoObj.transform.SetParent(_Camera.transform);
            m_GameLogoObj.transform.SetLocalPosXY(Vector2.zero);
            LogoTextureProvider.Renderer.transform.SetParent(_Camera.transform);
            LogoTextureProvider.Renderer.transform.SetLocalPosXY(Vector2.zero);
        }

        public void Show()
        {
            if (m_GameLogoAppeared)
                return;
            ShowGameLogoCoroutine();
        }
        
        #endregion

        #region nonpublic members
        
        private void OnCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            if (!m_OnStart || !RmazorUtils.MoveAndRotateCommands.ContainsAlt(_Command)) 
                return;
            m_OnStart = false;
        }

        private void ShowGameLogoCoroutine()
        {
            m_GameLogoAppeared = true;
            LockGameplayAndUiCommands(true);
            GetStartGameLogoTransform(out Vector2 position, out float scale);
            SetGameLogoTransform(position, scale);
            LogoTextureProvider.Activate(true);
            LogoTextureProvider.SetColor(MazorCommonData.CompanyLogoBackgroundColor);
            LogoTextureProvider.SetTransitionValue(1f, true);
            SetColors(ColorProvider.GetColor(ColorIds.UI));
            ShowGameLogo();
        }
        
        private void SetGameLogoTransform(Vector2 _Position, float _Scale)
        {
            m_GameLogoObj.transform.SetLocalPosXY(_Position);
            m_GameLogoObj.transform.localScale = Vector3.one * _Scale;
        }

        private void InitGameLogo()
        {
            const string prefabName = "game_logo_blady_maze";
            var go = PrefabSetManager.InitPrefab(
                null,
                CommonPrefabSetNames.UiGame,
                prefabName);
            var trigerrer = go.GetCompItem<AnimationTriggerer>("triggerer");
            trigerrer.Trigger1 += () => Cor.Run(HideGameLogoCoroutine());
            m_GameLogoCharAnims = GameLogoPartsDelaysDict.Keys
                .ToDictionary(
                    _C => _C, 
                    _C => go.GetCompItem<Animator>(_C));
            LogoTextureProvider.Init();
            m_GameLogoObj = go;
            Object.DontDestroyOnLoad(m_GameLogoObj);
        }
        
        private void GetStartGameLogoTransform(out Vector2 _Position, out float _Scale)
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            _Position = screenBounds.center;
            _Scale = GraphicUtils.AspectRatio * 6.5f;
        }
        
        private void SetColors(Color _Color)
        {
            if (m_Renderers == null)
            {
                var shapeTypes = new [] {typeof(Line), typeof(TextMeshPro), typeof(SpriteRenderer)};
                m_Renderers = shapeTypes.SelectMany(_Type => m_GameLogoObj
                        .GetComponentsInChildren(_Type, false))
                    .ToList();
            }
            foreach (var renderer in m_Renderers)
            {
                switch (renderer)
                {
                    case TextMeshPro textMeshPro:
                        textMeshPro.sortingOrder = SortingOrders.GameLogoForeground;
                        textMeshPro.color = _Color.SetA(textMeshPro.color.a);
                        break;
                    case SpriteRenderer spriteRenderer:
                        spriteRenderer.color = _Color.SetA(spriteRenderer.color.a);
                        break;
                    case Line lineRenderer:
                        lineRenderer.SetSortingOrder(SortingOrders.GameLogoForeground)
                            .SetColor(_Color);
                        break;
                }
            }
        }
        
        private IEnumerator HideGameLogoCoroutine()
        {
            StartHidingGameLogo(HideBackgroundTime);
            yield return Cor.Delay(1.2f, GameTicker);
            yield return Cor.Lerp(
                GameTicker,
                HideBackgroundTime,
                _OnProgress: _P => LogoTextureProvider.SetTransitionValue(1f - _P, false), 
                _OnFinish: () =>
                {
                    LockGameplayAndUiCommands(false);
                    LogoTextureProvider.Activate(false);
                    Shown?.Invoke();
                    WasShown = true;
                });
        }

        private void LockGameplayAndUiCommands(bool _Lock)
        {
            var commandsToLock = RmazorUtils.GetCommandsToLockInUiMenues();
            const string lockGroup = nameof(IViewUIGameLogo); 
            if (_Lock)
                CommandsProceeder.LockCommands(commandsToLock, lockGroup);
            else
                CommandsProceeder.UnlockCommands(commandsToLock, lockGroup);
        }

        private void ShowGameLogo()
        {
            foreach (string key in m_GameLogoCharAnims.Keys)
            {
                RaiseGameLogoItemAnimTrigger(key, AnimKeyGameLogoShow, 0f);
            }
        }
        
        private void StartHidingGameLogo(float _Time)
        {
            foreach ((string key, var animator) in m_GameLogoCharAnims)
            {
                animator.speed = 1f / _Time;
                RaiseGameLogoItemAnimTrigger(key, AnimKeyGameLogoStartHiding, GameLogoPartsDelaysDict[key]);
            }
        }

        private void RaiseGameLogoItemAnimTrigger(string _Key, int _Trigger, float _Delay)
        {
            float time = GameTicker.Time;
            Cor.Run(Cor.WaitWhile(
                () => time + _Delay > GameTicker.Time,
                () => m_GameLogoCharAnims[_Key].SetTrigger(_Trigger)));
        }
        
        #endregion
    }
}