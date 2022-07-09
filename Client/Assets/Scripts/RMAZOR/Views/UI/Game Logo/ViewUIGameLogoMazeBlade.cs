using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
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
    public interface IViewUIGameLogo : IInitViewUIItem
    {
        void              Show();
        bool              WasShown { get; }
        event UnityAction Shown;
    }
    
    public class ViewUIGameLogoMazeBlade : IViewUIGameLogo
    {
        #region constants

        private const float ShowTime           = 1.5f;
        private const float HideBackgroundTime = 0.75f;

        #endregion

        #region nonpublic members
        
        private static int AnimKeyGameLogoAppear    => AnimKeys.Anim;

        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");

        private static Dictionary<string, float> KeysAndDelays => new Dictionary<string, float>
        {
            {"M", 0f}, {"A1", 0f}, {"Z", 0f}, {"E1", 0f},
            {"B", 0.2f}, {"L", 0.2f}, {"A2", 0.2f}, {"D", 0.2f}, {"E2", 0.2f},
            {"bottom_line_animator", 1f},
            {"bottom_text_animator", 1f}
        };

        private GameObject                   m_GameLogoObj;
        private bool                         m_GameLogoAppeared;
        private Dictionary<string, Animator> m_GameLogoCharAnims;
        private float                        m_TopOffset;
        private bool                         m_OnStart = true;
        
        #endregion
        
        #region inject

        private ICameraProvider                  CameraProvider      { get; }
        private IContainersGetter                ContainersGetter    { get; }
        private IViewInputCommandsProceeder      CommandsProceeder   { get; }
        private IColorProvider                   ColorProvider       { get; }
        private IViewGameTicker                  GameTicker          { get; }
        private IPrefabSetManager                PrefabSetManager    { get; }
        private IViewMazeGameLogoTextureProvider LogoTextureProvider { get; }

        private ViewUIGameLogoMazeBlade(
            ICameraProvider                  _CameraProvider,
            IContainersGetter                _ContainersGetter,
            IViewInputCommandsProceeder      _CommandsProceeder,
            IColorProvider                   _ColorProvider,
            IViewGameTicker                  _GameTicker,
            IPrefabSetManager                _PrefabSetManager,
            IViewMazeGameLogoTextureProvider _LogoTextureProvider)
        {
            CameraProvider      = _CameraProvider;
            ContainersGetter    = _ContainersGetter;
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

        public void Init(Vector4 _Offsets)
        {
            CommandsProceeder.Command  += OnCommand;
            ColorProvider.ColorChanged += OnColorChanged;
            m_TopOffset = _Offsets.w;
            InitGameLogo();
        }
        
        public void Show()
        {
            if (m_GameLogoAppeared)
                return;
            Cor.Run(ShowGameLogoCoroutine());
        }
        
        #endregion

        #region nonpublic members
        
        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (!m_OnStart || !RmazorUtils.MoveAndRotateCommands.ContainsAlt(_Command)) 
                return;
            HideGameLogo();
            m_OnStart = false;
        }
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.UI)
                return;
            if (!m_GameLogoObj.activeSelf)
                return;
            SetColors(_Color);
        }

        private void InitGameLogo()
        {
            const string prefabName = "start_logo_maze_blade_2";
            var go = PrefabSetManager.InitPrefab(
                ContainersGetter.GetContainer(ContainerNames.GameUI),
                "ui_game",
                prefabName);
            go.GetCompItem<AnimationTriggerer>("triggerer").Trigger1 = () => go.SetActive(false);
            m_GameLogoCharAnims = KeysAndDelays.Keys
                .ToDictionary(
                    _C => _C, 
                    _C => go.GetCompItem<Animator>(_C));
            LogoTextureProvider.Init();
            var trigerrer = go.GetCompItem<AnimationTriggerer>("trigerrer");
            trigerrer.Trigger1 += () => Cor.Run(HideBackgroundAndDecreaseGameLogoCoroutine(1.0f));
            var rotator = go.GetCompItem<SimpleRotator>("rotator");
            rotator.Init(GameTicker, -2f);
            m_GameLogoObj = go;
        }
        
        private void GetStartGameLogoTransform(out Vector2 _Position, out float _Scale)
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            _Position = screenBounds.center;
            _Scale = GraphicUtils.AspectRatio * 6.5f;
        }
        
        private void GetFinalGameLogoTransform(out Vector2 _Position, out float _Scale)
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            const float additionalOffset = 3f;
            float yPos = screenBounds.max.y - m_TopOffset - additionalOffset;
            _Position = new Vector3(screenBounds.center.x, yPos);
            _Scale = GraphicUtils.AspectRatio * 5.5f;
        }

        private void SetColors(Color _Color)
        {
            var shapeTypes = new [] {typeof(Line), typeof(Disc), typeof(TextMeshPro)};
            shapeTypes.SelectMany(_Type => m_GameLogoObj
                    .GetComponentsInChildren(_Type, true))
                .ToList()
                .ForEach(_Shape =>
                {
                    switch (_Shape)
                    {
                        case TextMeshPro textMeshPro:
                            textMeshPro.sortingOrder = SortingOrders.GameLogoForeground;
                            textMeshPro.color = _Color.SetA(textMeshPro.color.a);
                            break;
                        case ShapeRenderer shapeRenderer:
                            shapeRenderer.SetSortingOrder(SortingOrders.GameLogoForeground)
                                .SetColor(_Color.SetA(shapeRenderer.Color.a));
                            break;
                    }

                });
            static IEnumerable<string> GetItems(string _Prefix, int _Count)
            {
                return Enumerable.Range(1, _Count).Select(_I => $"{_Prefix}_{_I}");
            }
            var maskGroups = new Dictionary<IEnumerable<string>, IEnumerable<string>>
            {
                {
                    GetItems("spike", 6).Concat(GetItems("saw", 3)),
                    new[] {"b_mask", "l_mask", "a_mask", "e_mask"}
                },
                {
                    new[] {"saw_3"},
                    new[] {"d_mask"}
                }
            };
            int maskOrder = 1;
            foreach (var (rednererKeys, maskKeys) in maskGroups)
            {
                foreach (string key in rednererKeys)
                {
                    var renderer = m_GameLogoObj.GetCompItem<SpriteRenderer>(key);
                    renderer.sortingOrder = SortingOrders.GameLogoForeground;
                    renderer.sharedMaterial.SetFloat(StencilRefId, 50 + maskOrder);
                    renderer.color = _Color;
                }
                foreach (string key in maskKeys)
                {
                    var mask = m_GameLogoObj.GetCompItem<ShapeRenderer>(key);
                    mask.StencilRefID = Convert.ToByte(50 + maskOrder);
                    mask.Color = new Color(0f, 0f, 0f, 1f / 255f);
                }
                maskOrder++;
            }
        }

        private IEnumerator ShowGameLogoCoroutine()
        {
            LockGameplayAndUiCommands(true);
            GetStartGameLogoTransform(out Vector2 position, out float scale);
            SetGameLogoTransform(position, scale);
            LogoTextureProvider.Activate(true);
            LogoTextureProvider.SetColor(CommonData.CompanyLogoBackgroundColor);
            LogoTextureProvider.SetTransitionValue(1f);
            yield return Cor.Delay(0.5f, GameTicker);
            SetColors(ColorProvider.GetColor(ColorIds.UI));
            ShowGameLogo(ShowTime);
            yield return Cor.Delay(ShowTime, GameTicker,() => m_GameLogoAppeared = true);
        }

        private IEnumerator HideBackgroundAndDecreaseGameLogoCoroutine(float _Delay)
        {
            yield return Cor.Delay(_Delay, GameTicker);
            GetStartGameLogoTransform(out Vector2 startPos, out float startScale);
            GetFinalGameLogoTransform(out Vector2 finalPos, out float finalScale);
            Cor.Run(Cor.Lerp(
                GameTicker,
                HideBackgroundTime,
                _OnProgress: _P =>
                {
                    var pos = Vector2.Lerp(startPos, finalPos, _P);
                    float scale = Mathf.Lerp(startScale, finalScale, _P);
                    SetGameLogoTransform(pos, scale);
                }));
            yield return Cor.Lerp(
                GameTicker,
                HideBackgroundTime,
                _OnProgress: _P => LogoTextureProvider.SetTransitionValue(1f - _P), 
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
            var commands = RmazorUtils.MoveAndRotateCommands
                .Concat(new[] {EInputCommand.ShopMenu, EInputCommand.SettingsMenu});
            const string lockGroup = nameof(IViewUIGameLogo); 
            if (_Lock)
                CommandsProceeder.LockCommands(commands, lockGroup);
            else
                CommandsProceeder.UnlockCommands(commands, lockGroup);
        }
        
        private void ShowGameLogo(float _Time)
        {
            foreach ((string key, var animator) in m_GameLogoCharAnims)
            {
                animator.speed = 1f / _Time;
                ShowGameLogoItem(key, KeysAndDelays[key]);
            }
        }

        private void HideGameLogo()
        {
            m_GameLogoObj.SetActive(false);
        }

        private void ShowGameLogoItem(string _Key, float _Delay)
        {
            float time = GameTicker.Time;
            Cor.Run(Cor.WaitWhile(
                () => time + _Delay > GameTicker.Time,
                () => m_GameLogoCharAnims[_Key].SetTrigger(AnimKeyGameLogoAppear)));
        }
        
        private void SetGameLogoTransform(Vector2 _Position, float _Scale)
        {
            m_GameLogoObj.transform.SetLocalPosXY(_Position);
            m_GameLogoObj.transform.localScale = Vector3.one * _Scale;
        }

        #endregion
    }
}