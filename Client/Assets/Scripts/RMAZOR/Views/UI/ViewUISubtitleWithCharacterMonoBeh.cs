using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Utils;
using RMAZOR.Views.Common;
using Shapes;
using TMPro;
using UnityEngine;
using Zenject;

namespace RMAZOR.Views.UI
{
    public class ViewUISubtitleWithCharacterMonoBeh : MonoBehInitBase
    {
        [SerializeField] private ShapeRenderer       charHead;
        [SerializeField] private List<ShapeRenderer> charEyes;
        [SerializeField] private Animator            charAnim;
        [SerializeField] private TextMeshPro         text;

        private IContainersGetter        ContainersGetter    { get; set; }
        private IMazeCoordinateConverter CoordinateConverter { get; set; }
        private IColorProvider           ColorProvider       { get; set; }

        [Inject]
        private void Inject(
            IContainersGetter        _ContainersGetter,
            IMazeCoordinateConverter _CoordinateConverter,
            IColorProvider           _ColorProvider)
        {
            ContainersGetter    = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            ColorProvider       = _ColorProvider;
        }

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            var screenBounds = GraphicUtils.GetVisibleBounds();
            text.transform.SetPosXY(
                screenBounds.center.x,
                screenBounds.min.y + 5f);
            HideSubtitle();
            base.Init();
        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Character)
                charHead.SetColor(_Color);
            else if (_ColorId == ColorIds.Background1 || _ColorId == ColorIds.Background2)
            {
                var avCol = Color.Lerp(
                    ColorProvider.GetColor(ColorIds.Background1),
                    ColorProvider.GetColor(ColorIds.Background2),
                    0.5f);
                charEyes.ForEach(_Eye => _Eye.SetColor(avCol));
            }
            else if (_ColorId == ColorIdsCommon.UiText)
                text.color = _Color;
        }

        public void ShowSubtitle(string _Text, float _Seconds, bool _WithCharacter)
        {
            charAnim.SetTrigger(_WithCharacter ? AnimKeys.Stop2 : AnimKeys.Anim);
            text.text = _Text;
            text.enabled = true;
            Cor.Run(Cor.Delay(
                _Seconds,
                () =>
                {
                    if (_WithCharacter)
                        charAnim.SetTrigger(AnimKeys.Stop);
                }));
        }

        public void HideSubtitle()
        {
            text.enabled = false;
            charAnim.SetTrigger(AnimKeys.Stop2);
        }
    }
}