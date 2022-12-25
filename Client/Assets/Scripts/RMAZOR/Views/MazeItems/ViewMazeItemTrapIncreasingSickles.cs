using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemTrapIncreasing : IViewMazeItem
    {
        void OnIncreasing(MazeItemTrapIncreasingEventArgs _Args);
    }
    
    public class ViewMazeItemTrapIncreasingSickles 
        : ViewMazeItemBase,
          IViewMazeItemTrapIncreasing,
          IUpdateTick
    {
        #region nonpublic members

        protected override string ObjectName => "Trap Increasing Block";
        
        private static AudioClipArgs AudioClipArgsTrapIncreasingOpen =>
            new AudioClipArgs("sword_open", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsTrapIncreasingRotate => 
            new AudioClipArgs("sword_spinning", EAudioClipType.GameSound, _Loop: true);
        private static AudioClipArgs AudioClipArgsTrapIncreasingClose => 
            new AudioClipArgs("sword_close", EAudioClipType.GameSound);
        
        private static int AnimKeyOpen => AnimKeys.Anim;
        private static int AnimKeyClose => AnimKeys.Stop;
        
        private readonly List<Line> m_BladeContainers 
            = new List<Line>();
        private readonly List<SpriteRenderer> m_Blades          
            = new List<SpriteRenderer>();
        
        private Animator           m_Animator;
        private AnimationTriggerer m_Triggerer;
        private List<Vector2>      m_DeathZone;
        
        private Disc               
            m_Center,
            m_Center2;
        private Rectangle
            m_Head,
            m_HeadBorder,
            m_Eye1Idle,
            m_Eye2Idle;
        private Triangle
            m_Eye1Angry,
            m_Eye2Angry;
        private Line m_Mouth;

        private bool  m_IsRotatingSoundPlaying;
        private bool? m_TrapOpened;
        private bool  m_ReadyToKill;

        #endregion

        #region inject
        
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        private ViewMazeItemTrapIncreasingSickles(
            ViewSettings                        _ViewSettings,
            IModelGame                          _Model,
            ICoordinateConverter                _CoordinateConverter,
            IContainersGetter                   _ContainersGetter,
            IViewGameTicker                     _GameTicker,
            IRendererAppearTransitioner         _Transitioner,
            IManagersGetter                     _Managers,
            IColorProvider                      _ColorProvider,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder)
        {
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion
        
        #region api
        
        public override Component[] Renderers => new Component[]{m_Center, m_Center2, m_Head}
            .Concat(m_Blades)
            .Concat(m_BladeContainers)
            .ToArray();
        
        public override object Clone() => new ViewMazeItemTrapIncreasingSickles(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder,
            SwitchLevelStageCommandInvoker);

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_Animator.SetGoActive(value);
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_Animator.speed = _Args.LevelStage == ELevelStage.Paused ? 0f : 1f;
            base.OnLevelStageChanged(_Args);
            switch (_Args.LevelStage)
            {
                case ELevelStage.Finished when m_TrapOpened.HasValue && m_TrapOpened.Value:
                    CloseTrap();
                    break;
            }
        }

        public void OnIncreasing(MazeItemTrapIncreasingEventArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ModelCommonData.TrapIncreasingStageIncreased: OpenTrap();  break;
                case ModelCommonData.StageIdle:                    CloseTrap(); break;
            }
        }
        
        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            if (!m_ReadyToKill)
                return;
            CheckForCharacterDeath();
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Main:
                    m_HeadBorder.SetColor(_Color);
                    m_Eye1Idle  .SetColor(_Color);
                    m_Eye2Idle  .SetColor(_Color);
                    m_Eye1Angry .SetColor(_Color);
                    m_Eye2Angry .SetColor(_Color);
                    m_Mouth     .SetColor(_Color);
                    break;
                case ColorIds.MazeItem1:
                    m_Center .SetColor(_Color);
                    m_Center2.SetColor(_Color);
                    m_Head   .SetColor(_Color);
                    foreach (var item in m_Blades)
                        item.color  = _Color;
                    foreach (var item in m_BladeContainers)
                        item.Color  = _Color.SetA(0.5f);
                    break;
            }

        }

        protected override void InitShape()
        {
            var prefab = Managers.PrefabSetManager.InitPrefab(
                Object.transform, "views", "trap_increasing_2");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            m_Animator   = prefab.GetCompItem<Animator>("animator");
            m_Triggerer  = prefab.GetCompItem<AnimationTriggerer>("triggerer");
            m_Center     = prefab.GetCompItem<Disc>("center");
            m_Center2    = prefab.GetCompItem<Disc>("center_2");
            m_Head       = prefab.GetCompItem<Rectangle>("head");
            m_HeadBorder = prefab.GetCompItem<Rectangle>("head_border");
            m_Eye1Idle   = prefab.GetCompItem<Rectangle>("eye_1_idle");
            m_Eye2Idle   = prefab.GetCompItem<Rectangle>("eye_2_idle");
            m_Eye1Angry  = prefab.GetCompItem<Triangle>("eye_1_angry");
            m_Eye2Angry  = prefab.GetCompItem<Triangle>("eye_2_angry");
            m_Mouth      = prefab.GetCompItem<Line>("mouth");
            int sortingOrder = SortingOrders.GetBlockSortingOrder(EMazeItemType.TrapIncreasing);
            m_Center    .SetSortingOrder(sortingOrder);
            m_Center2   .SetSortingOrder(sortingOrder);
            m_Head      .SetSortingOrder(sortingOrder + 1);
            m_HeadBorder.SetSortingOrder(sortingOrder + 2);
            m_Eye1Idle  .SetSortingOrder(sortingOrder + 2);
            m_Eye2Idle  .SetSortingOrder(sortingOrder + 2);
            m_Eye1Angry .SetSortingOrder(sortingOrder + 2);
            m_Eye2Angry .SetSortingOrder(sortingOrder + 2);
            m_Mouth.SetSortingOrder(sortingOrder + 2);
            for (int i = 1; i <= 4; i++)
            {
                m_BladeContainers.Add(prefab.GetCompItem<Line>($"blade_container_{i}"));
                m_Blades.Add(prefab.GetCompItem<SpriteRenderer>($"blade_{i}"));
            }
            foreach (var bladeContainer in m_BladeContainers)
            {
                bladeContainer.SetSortingOrder(sortingOrder)
                    .SetThickness(0.07f)
                    .SetEndCaps(LineEndCap.Round)
                    .enabled = false;
            }
            foreach (var blade in m_Blades)
            {
                blade.sortingOrder = sortingOrder;
                blade.enabled = false;
            }
            m_Triggerer.Trigger1 += OnTrapOpeningStart;
            m_Triggerer.Trigger2 += OnTrapOpeningFinish;
            m_Triggerer.Trigger3 += OnTrapClosingStart;
            m_Triggerer.Trigger4 += OnTrapClosingFinish;
            m_Triggerer.Trigger5 += OnTrapRotatingStart;
        }

        protected override void UpdateShape()
        {
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            Object.transform.localScale = Vector3.one * CoordinateConverter.Scale * 1.3f;
            m_DeathZone = new List<V2Int>
            {
                Props.Position + V2Int.Down,
                Props.Position + V2Int.Up,
                Props.Position + V2Int.Left,
                Props.Position + V2Int.Right,
                Props.Position + V2Int.Down + V2Int.Left,
                Props.Position + V2Int.Down + V2Int.Right,
                Props.Position + V2Int.Up + V2Int.Left,
                Props.Position + V2Int.Up + V2Int.Right
            }.Select(_P => (Vector2)_P)
                .ToList();
        }
        
        private void OpenTrap()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            if (m_TrapOpened.HasValue && m_TrapOpened.Value)
                return;
            var stage = Model.LevelStaging.LevelStage;
            if (stage == ELevelStage.Finished || stage == ELevelStage.Unloaded)
                return;
            m_TrapOpened = true;
            m_Animator.SetTrigger(AnimKeyOpen);
            Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingOpen);
        }

        private void CloseTrap()
        {
            Managers.AudioManager.StopClip(AudioClipArgsTrapIncreasingRotate);
            m_IsRotatingSoundPlaying = false;
            if (AppearingState != EAppearingState.Appeared)
                return;
            if (m_TrapOpened.HasValue && !m_TrapOpened.Value)
                return;
            m_TrapOpened = false;
            m_Animator.SetTrigger(AnimKeyClose);
            Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingClose);
        }

        private void OnTrapOpeningStart()
        {
            m_ReadyToKill = true;
        }
        
        private void OnTrapOpeningFinish()
        {
            
        }

        private void OnTrapClosingStart()
        {
            
        }

        private void OnTrapClosingFinish()
        {
            m_ReadyToKill = false;
        }

        private void OnTrapRotatingStart()
        {
            if (m_IsRotatingSoundPlaying) 
                return;
            Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingRotate);
            m_IsRotatingSoundPlaying = true;
        }

        protected override void OnAppearStart(bool _Appear)
        {
            if (_Appear)
            {
                m_Animator.enabled = true;
                m_Animator.ResetTrigger(AnimKeyClose);
                m_Animator.ResetTrigger(AnimKeyOpen);
            }
            else
            {
                CloseTrap();
            }
            base.OnAppearStart(_Appear);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
                m_Animator.enabled = false;
            base.OnAppearFinish(_Appear);
        }

        private void CheckForCharacterDeath()
        {
            if (!Model.Character.Alive)
                return;
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return;
            const float distance = 0.5f;
            var character = Model.Character;
            var cPos = character.IsMoving ? 
                character.MovingInfo.PrecisePosition : character.Position;
            bool death = false;
            for (int i = 0; i < m_DeathZone.Count; i++)
            {
                if (!(Vector2.Distance(m_DeathZone[i], cPos) + MathUtils.Epsilon < distance))
                    continue;
                death = true;
                break;
            }
            if (!death) 
                return;
            var args = new Dictionary<string, object>
            {
                {CommonInputCommandArg.KeyDeathPosition, 
                    (V2)CoordinateConverter.ToLocalCharacterPosition(cPos)}
            };
            SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.KillCharacter, args);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var colMazeItem1 = ColorProvider.GetColor(ColorIds.MazeItem1);
            var colMain = ColorProvider.GetColor(ColorIds.Main);
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] { m_Center, m_Center2, m_Head }, () => colMazeItem1}, 
                {new Component[] { m_Center2, m_HeadBorder, m_Eye1Idle, m_Eye2Idle, m_Eye1Angry, m_Eye2Angry, m_Mouth }, () => colMain}, 
                {m_Blades, () => colMazeItem1}, 
                {m_BladeContainers, () => colMazeItem1.SetA(0.5f)}
            };
        }

        #endregion
    }
}