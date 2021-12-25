using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.ContainerGetters;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IRotatingPossibilityIndicator
    {
        Animator           Animator  { get; }
        Rectangle          Shape     { get; }
        AnimationTriggerer Triggerer { get; }
    }
    
    public class RotatingPossibilityIndicator : IRotatingPossibilityIndicator
    {
        private IContainersGetter ContainersGetter { get; }
        private IManagersGetter   Managers         { get; }

        public RotatingPossibilityIndicator(
            IContainersGetter _ContainersGetter,
            IManagersGetter _Managers)
        {
            ContainersGetter = _ContainersGetter;
            Managers = _Managers;
        }

        public Animator           Animator  { get; private set; }
        public Rectangle          Shape     { get; private set; }
        public AnimationTriggerer Triggerer { get; private set; }

        public void Init()
        {
            const float scale = 3f;
            var screenBounds = GraphicUtils.GetVisibleBounds();
            var cont = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var goIndicator = Managers.PrefabSetManager.InitPrefab(
                cont, "ui_game", "rotating_possibility_indicator");
            Shape = goIndicator.GetCompItem<Rectangle>("indicator");
            Animator = goIndicator.GetCompItem<Animator>("animator");
            Triggerer = goIndicator.GetCompItem<AnimationTriggerer>("triggerer");
            goIndicator.transform.localScale = Vector3.one * scale;
            goIndicator.transform.SetPosXY(
                screenBounds.center.x,
                screenBounds.min.y + 10f);
        }
    }
}