using System;
using System.Collections.Generic;
using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHeadPartExtended
    {
        Func<GameObject>                                GetCharacterGameObject { set; }
        void                                            ActivateShapes(bool _Active);
        Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear);
    }
    
    public interface IViewCharacterHeadPart :
        IInit,
        IActivated,
        IOnLevelStageChanged,
        IOnPathCompleted,
        IAppear
    {
    }

    public abstract class ViewCharacterHeadPartBase : InitBase, IViewCharacterHeadPart
    {
        #region nonpublic members
        
        private bool m_Activated;

        #endregion

        #region inject

        private   ViewSettings                ViewSettings       { get; }
        protected IColorProvider              ColorProvider      { get; }
        private   IRendererAppearTransitioner AppearTransitioner { get; }

        protected ViewCharacterHeadPartBase(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _AppearTransitioner)
        {
            ViewSettings       = _ViewSettings;
            ColorProvider      = _ColorProvider;
            AppearTransitioner = _AppearTransitioner;
        }

        #endregion

        #region api
        
        public EAppearingState AppearingState { get; private set; }
        
        public Func<GameObject> GetCharacterGameObject { protected get; set; }
        
        public bool Activated
        {
            get => m_Activated;
            set
            {
                if (value)
                    UpdatePrefab();
                ActivateShapes(value);
                m_Activated = value;
            }
        }
        public override void Init()
        {
            if (Initialized)
                return;
            ColorProvider.ColorChanged += OnColorChanged;
            InitPrefab();
            base.Init();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.None:
                    Activated = true;
                    break;
                case ELevelStage.ReadyToStart when 
                    _Args.PreviousStage == ELevelStage.Paused 
                    && _Args.PrePreviousStage == ELevelStage.CharacterKilled:
                {
                    ActivateShapes(true);
                }
                    break;
                case ELevelStage.CharacterKilled:
                    ActivateShapes(false);
                    break;
            }
        }

        public void OnPathCompleted(V2Int _LastPath)
        {
            ActivateShapes(false);
        }

        public void Appear(bool _Appear)
        {
            if (_Appear)
                ActivateShapes(true);
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            AppearTransitioner.DoAppearTransition(
                _Appear,
                GetAppearSets(_Appear),
                ViewSettings.betweenLevelTransitionTime,
                () =>
                {
                    AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
                });
        }

        #endregion

        #region nonpublic methods
        
        protected abstract void OnColorChanged(int _ColorId, Color _Color);
        protected abstract void InitPrefab();
        protected abstract void UpdatePrefab();
        protected abstract void ActivateShapes(bool _Active);

        protected abstract Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear);

        #endregion
    }
}