using System;
using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.Common.ViewMazeMoneyItems
{
    public interface IViewMazeItemPathItemMoney : 
        IInit, 
        IOnLevelStageChanged,
        ICloneable,
        IAppear
    {
        event UnityAction       Collected;
        bool                    IsCollected { get; set; }
        Func<ViewMazeItemProps> GetProps    { set; }
        Transform               Parent      { set; }
        void InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent);
        void Collect(bool                 _Collect);
        void UpdateShape();
        void EnableInitializedShapes(bool _Enable);
    }

    public class ViewMazeItemPathItemMoneyDisc : InitBase, IViewMazeItemPathItemMoney
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsCollectMoneyItem => 
            new AudioClipArgs("collect_point", EAudioClipType.GameSound);

        private Disc     m_MainDisc;
        private Disc     m_InnerDisc, m_OuterDisc;
        private Animator m_Animator;
        private bool     m_Active;
        
        private bool Active
        {
            get => m_Active;
            set
            {
                m_Active = value;
                if (!Initialized)
                    return;
                m_MainDisc.enabled  = value;
                var additionalBackgroundInfo = BackgroundTextureController.GetBackgroundColorArgs().AdditionalInfo;
                bool isDarkColorTheme = additionalBackgroundInfo != null && additionalBackgroundInfo.dark;
                m_InnerDisc.enabled = value && !isDarkColorTheme;
                m_OuterDisc.enabled = value && !isDarkColorTheme;
            }
        }
        
        #endregion

        #region inject

        private ViewSettings                         ViewSettings                { get; }
        private IPrefabSetManager                    PrefabSetManager            { get; }
        private IColorProvider                       ColorProvider               { get; }
        private ICoordinateConverter                 CoordinateConverter         { get; }
        private IAudioManager                        AudioManager                { get; }
        private IRendererAppearTransitioner          Transitioner                { get; }
        private IViewMazeBackgroundTextureController BackgroundTextureController { get; }

        private ViewMazeItemPathItemMoneyDisc(
            ViewSettings                         _ViewSettings,
            IPrefabSetManager                    _PrefabSetManager,
            IColorProvider                       _ColorProvider,
            ICoordinateConverter                 _CoordinateConverter,
            IAudioManager                        _AudioManager,
            IRendererAppearTransitioner          _Transitioner,
            IViewMazeBackgroundTextureController _BackgroundTextureController)
        {
            ViewSettings                = _ViewSettings;
            PrefabSetManager            = _PrefabSetManager;
            ColorProvider               = _ColorProvider;
            CoordinateConverter         = _CoordinateConverter;
            AudioManager                = _AudioManager;
            Transitioner                = _Transitioner;
            BackgroundTextureController = _BackgroundTextureController;
        }

        #endregion
        
        #region api
        
        public event UnityAction Collected;
        public bool              IsCollected    { get; set; }
        public EAppearingState   AppearingState { get; private set; }
        
        public Func<ViewMazeItemProps> GetProps { private get; set; }
        public Transform               Parent   { private get; set; }

        public object Clone() => 
            new ViewMazeItemPathItemMoneyDisc(
                ViewSettings, 
                PrefabSetManager,
                ColorProvider, 
                CoordinateConverter, 
                AudioManager,
                Transitioner,
                BackgroundTextureController);
        
        public override void Init()
        {
            if (Initialized)
                return;
            ColorProvider.ColorChanged += OnColorChanged;
            base.Init();
        }

        public void InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent)
        {
            GetProps = _GetProps;
            Parent = _Parent;
            Init();
            InitShape();
        }

        public void Collect(bool _Collect)
        {
            IsCollected = _Collect;
            if (!_Collect || !Active)
                return;
            Collected?.Invoke();
            AudioManager.PlayClip(AudioClipArgsCollectMoneyItem);
            Active = false;
        }

        public void UpdateShape()
        {
            if (!GetProps().IsMoneyItem)
            {
                Active = false;
                return;
            }
            Active = true;
            const float commonScaleCoeff = 0.3f;
            float scale = CoordinateConverter.Scale;
            m_MainDisc.transform.SetLocalScaleXY(Vector2.one * scale * commonScaleCoeff);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (!Active)
                return;
            m_Animator.speed = _Args.LevelStage == ELevelStage.Paused ? 0f : 1f;
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart:
                    m_Animator.SetTrigger(AnimKeys.Anim);
                    break;
                case ELevelStage.Unloaded:
                    m_Animator.SetTrigger(AnimKeys.Stop);
                    break;
            }
        }
        
        public void EnableInitializedShapes(bool _Enable)
        {
            if (!Initialized || m_MainDisc.IsNull())
                return;
            var props = GetProps();
            Active = _Enable && props.IsMoneyItem && !props.Blank;
        }
        
        public void Appear(bool _Appear)
        {
            AppearCore(_Appear);
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (!Active || IsCollected)
                return;
            bool isDarkColorTheme = BackgroundTextureController.GetBackgroundColorArgs().AdditionalInfo.dark;
            m_OuterDisc.enabled = !isDarkColorTheme;
            m_InnerDisc.enabled = !isDarkColorTheme; 
            switch (_ColorId)
            {
                case ColorIds.MoneyItem:
                    m_MainDisc.SetColor(_Color);
                    break;
                case ColorIds.Main:
                    m_OuterDisc.Color = _Color;
                    m_InnerDisc.Color = _Color; 
                    break;
            }
        }

        private void InitShape()
        {
            var go = PrefabSetManager.InitPrefab(
                Parent, CommonPrefabSetNames.Views, "money_item");
            m_Animator = go.GetCompItem<Animator>("animator");
            var col = ColorProvider.GetColor(ColorIds.MoneyItem);
            const int sortingOrder = SortingOrders.MoneyItem;
            m_MainDisc = go.GetCompItem<Disc>("main_disc")
                .SetSortingOrder(sortingOrder)
                .SetColor(col);
            m_OuterDisc = go.GetCompItem<Disc>("outer_disc")
                .SetSortingOrder(sortingOrder + 1);
            m_InnerDisc = go.GetCompItem<Disc>("inner_disc")
                .SetSortingOrder(sortingOrder + 1);
            go.transform.SetLocalPosXY(Vector2.zero);
        }

        private void AppearCore(bool _Appear)
        {
            if (!GetProps().IsMoneyItem)
                return;
            var colBody = ColorProvider.GetColor(ColorIds.MoneyItem);
            var appearSets = new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new[] {m_MainDisc}, () => colBody},
            };
            bool isDarkColorTheme = BackgroundTextureController.GetBackgroundColorArgs().AdditionalInfo.dark;
            if (!isDarkColorTheme)
            {
                var colBorder = ColorProvider.GetColor(ColorIds.Main);
                appearSets.Add(new[] {m_InnerDisc, m_OuterDisc}, () => colBorder);
            }
            Cor.Run(Cor.WaitWhile(
                () => !Initialized,
                () =>
                {
                    OnAppearStart(_Appear);
                    Transitioner.DoAppearTransition(
                        _Appear,
                        appearSets,
                        ViewSettings.betweenLevelTransitionTime,
                        () =>
                        {
                            OnAppearFinish(_Appear);
                        });
                }));
        }

        private void OnAppearStart(bool _Appear)
        {
            bool isDarkColorTheme = BackgroundTextureController.GetBackgroundColorArgs().AdditionalInfo.dark;
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
            {
                m_InnerDisc.enabled = !isDarkColorTheme;
                m_OuterDisc.enabled = !isDarkColorTheme;
            }
            var props = GetProps();
            if ((_Appear || (props.Blank || !IsCollected)) &&
                (!_Appear || (!props.Blank && !props.IsStartNode)))
            {
                return;
            }
            if (props.IsMoneyItem)
                Active = false;
        }

        private void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
            {
                m_InnerDisc.enabled = false;
                m_OuterDisc.enabled = false;
            }
            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
        }

        #endregion
    }
}