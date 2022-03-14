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
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.UI.Game_Logo
{
    public interface IViewUIGameLogo : IInitViewUIItem
    {
        void Show();
        void Hide();
    }
    
    public class ViewUIGameLogoMazeBlade : IViewUIGameLogo
    {
        #region constants

        private const float ShowTime    = 1f;
        private const float HideBackgroundTime = 1f;
        private const float HideGameLogoTime = 0.3f;

        #endregion

        #region nonpublic members
        
        private static int AnimKeyGameLogoAppear    => AnimKeys.Anim;
        private static int AnimKeyGameLogoDisappear => AnimKeys.Stop;

        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");

        private static Dictionary<string, float> KeysAndDelays => new Dictionary<string, float>
        {
            {"M", 0}, {"A1", 0.1f}, {"Z", 0.2f}, {"E1", 0.3f},
            {"B", 0}, {"L", 0.1f}, {"A2", 0.2f}, {"D", 0.3f}, {"E2", 0.4f}
        };

        private GameObject                   m_GameLogoObj;
        private bool                         m_LogoShowingAnimationPassed;
        private bool                         m_GameLogoAppeared;
        private bool                         m_GameLogoDisappeared;
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
        private IViewInputTouchProceeder         TouchProceeder      { get; }

        public ViewUIGameLogoMazeBlade(
            ICameraProvider                  _CameraProvider,
            IContainersGetter                _ContainersGetter,
            IViewInputCommandsProceeder      _CommandsProceeder,
            IColorProvider                   _ColorProvider,
            IViewGameTicker                  _GameTicker,
            IPrefabSetManager                _PrefabSetManager,
            IViewMazeGameLogoTextureProvider _LogoTextureProvider,
            IViewInputTouchProceeder         _TouchProceeder)
        {
            CameraProvider      = _CameraProvider;
            ContainersGetter    = _ContainersGetter;
            CommandsProceeder   = _CommandsProceeder;
            ColorProvider       = _ColorProvider;
            GameTicker          = _GameTicker;
            PrefabSetManager    = _PrefabSetManager;
            LogoTextureProvider = _LogoTextureProvider;
            TouchProceeder      = _TouchProceeder;
        }

        #endregion

        #region api

        public void Init(Vector4 _Offsets)
        {
            CommandsProceeder.Command  += OnCommand;
            ColorProvider.ColorChanged += OnColorChanged;
            m_TopOffset = _Offsets.w;
            InitGameLogo();
            SetColors(ColorProvider.GetColor(ColorIdsCommon.UI));
            // TouchProceeder.OnTap += OnTap;
        }
        
        public void Show()
        {
            if (m_GameLogoAppeared)
                return;
            m_GameLogoDisappeared = false;
            Cor.Run(ShowGameLogoCoroutine());
        }

        public void Hide()
        {
            if (!m_GameLogoAppeared || m_GameLogoDisappeared)
                return;
            Cor.Run(HideBackgroundAndDecreaseGameLogoCoroutine());
        }

        #endregion

        #region nonpublic members
        
        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (!m_OnStart || !RazorMazeUtils.MoveAndRotateCommands.ContainsAlt(_Command)) 
                return;
            HideGameLogo(HideGameLogoTime);
            m_OnStart = false;
        }
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIdsCommon.UI)
                return;
            if (!m_GameLogoObj.activeSelf)
                return;
            SetColors(_Color);
        }
        
        private void OnTap(Vector2 _)
        {
            if (!m_GameLogoAppeared || m_GameLogoDisappeared)
                return;
            TouchProceeder.OnTap -= OnTap;
            Cor.Run(HideBackgroundAndDecreaseGameLogoCoroutine());
        }

        private void InitGameLogo()
        {
            string prefabName = "start_logo_maze_blade" + (IsLowAspect() ? string.Empty : "_2");
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
            trigerrer.Trigger1 += () =>
            {
                m_LogoShowingAnimationPassed = true;
                Cor.Run(Cor.Delay(0.5f, () =>
                {
                    Cor.Run(HideBackgroundAndDecreaseGameLogoCoroutine());
                }));
            };
            var rotator = go.GetCompItem<SimpleRotator>("rotator");
            rotator.Init(GameTicker, -2f);
            m_GameLogoObj = go;
        }
        
        private void GetStartGameLogoTransform(out Vector2 _Position, out float _Scale)
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            _Position = screenBounds.center;
            _Scale = GraphicUtils.AspectRatio * (IsLowAspect() ? 10f : 6.5f);
        }
        
        private void GetFinalGameLogoTransform(out Vector2 _Position, out float _Scale)
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            const float additionalOffset = 3f;
            float yPos = screenBounds.max.y - m_TopOffset - additionalOffset;
            _Position = new Vector3(screenBounds.center.x, yPos);
            _Scale = GraphicUtils.AspectRatio * (IsLowAspect() ? 7f : 4.5f);
        }

        private void SetColors(Color _Color)
        {
            var shapeTypes = new [] {typeof(Line), typeof(Disc), typeof(Rectangle)};
            shapeTypes.SelectMany(_Type => m_GameLogoObj
                    .GetComponentsInChildren(_Type, true))
                .Cast<ShapeRenderer>()
                .ToList()
                .ForEach(_Shape =>
                {
                    _Shape.SetSortingOrder(SortingOrders.GameLogoForeground)
                        .SetColor(_Color.SetA(_Shape.Color.a));
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
            ShowGameLogo(ShowTime);
            var logoBackCol = ColorProvider.GetColor(ColorIds.Background1);
            yield return Cor.Lerp(
                0f,
                1f,
                ShowTime,
                _P =>
                {
                    var logoBackTextureColor = Color.Lerp(
                        CommonData.CompanyLogoBackgroundColor, 
                        logoBackCol,
                        _P);
                    LogoTextureProvider.SetColor(logoBackTextureColor);
                }, 
                GameTicker,
                (_Broken, _Progress) => m_GameLogoAppeared = true);
        }

        private IEnumerator HideBackgroundAndDecreaseGameLogoCoroutine()
        {
            GetStartGameLogoTransform(out Vector2 startPos, out float startScale);
            GetFinalGameLogoTransform(out Vector2 finalPos, out float finalScale);
            var logoBackCol = ColorProvider.GetColor(ColorIds.Background1);
            Cor.Run(Cor.Lerp(
                0f,
                1f,
                HideBackgroundTime * 0.5f,
                _P =>
                {
                    var pos = Vector2.Lerp(startPos, finalPos, _P);
                    float scale = Mathf.Lerp(startScale, finalScale, _P);
                    SetGameLogoTransform(pos, scale);
                },
                GameTicker));
            yield return Cor.Lerp(
                0f,
                1f,
                HideBackgroundTime,
                _P =>
                {
                    float alpha = Mathf.Lerp(1f, 0f, _P);
                    LogoTextureProvider.SetColor(logoBackCol.SetA(alpha));
                }, 
                GameTicker,
                (_Broken, _Progress) =>
                {
                    LockGameplayAndUiCommands(false);
                    m_GameLogoDisappeared = true;
                    LogoTextureProvider.Activate(false);
                });
        }

        private void LockGameplayAndUiCommands(bool _Lock)
        {
            var commands = RazorMazeUtils.MoveAndRotateCommands
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

        private void HideGameLogo(float _Time)
        {
            m_GameLogoObj.SetActive(false);
            
            // if (!m_LogoShowingAnimationPassed)
            // {
            //     m_GameLogoObj.SetActive(false);
            //     return;
            // }
            // foreach (var anim in m_GameLogoCharAnims.Values)
            // {
            //     anim.speed = 1f / _Time;
            //     anim.SetTrigger(AnimKeyGameLogoDisappear);
            // }
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

        private static bool IsLowAspect()
        {
            return GraphicUtils.AspectRatio < 0.6f;
        }

        #endregion
    }
}