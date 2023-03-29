using System;
using System.Collections.Generic;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHeadOneSprite : IViewCharacterHead { }
    
    public abstract class ViewCharacterHeadOneSpriteBase : ViewCharacterHeadBase, IViewCharacterHeadOneSprite
    {
        #region nonpublic members

        private SpriteRenderer m_Head;
        private Sprite         m_HeadSprite;

        private          string SpriteAssetName => $"character_head_sprite_{Id}";
        protected override string PrefabName      => "character_head_one_sprite";

        #endregion
        
        #region inject

        protected ViewCharacterHeadOneSpriteBase(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _AppearTransitioner,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _ViewSettings, 
                _ColorProvider,
                _ContainersGetter, 
                _PrefabSetManager,
                _CoordinateConverter, 
                _AppearTransitioner,
                _CommandsProceeder) { }

        #endregion

        #region api

        public override void Init()
        {
            base.Init();
            m_Head = PrefabObj.GetCompItem<SpriteRenderer>("body");
            m_HeadSprite = PrefabSetManager.GetObject<Sprite>(
                "characters", 
                SpriteAssetName);
            m_Head.sprite = m_HeadSprite;
            m_Head.sortingOrder = SortingOrders.Character;
        }

        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color) { }

        protected override void ActivateShapes(bool _Active)
        {
            m_Head.enabled = _Active;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return new Dictionary<IEnumerable<Component>, Func<Color>>();
        }

        #endregion
    }
}