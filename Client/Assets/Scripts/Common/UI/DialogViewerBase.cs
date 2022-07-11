using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.CameraProviders.Camera_Effects_Props;
using Common.Extensions;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Common.UI
{
    public abstract class DialogViewerBase : IDialogViewerBase
    {
        protected IDialogPanel FakePanel = new DialogPanelFake();
        
        protected ICameraProvider   CameraProvider   { get; }
        private   IUITicker         Ticker           { get; }
        protected IPrefabSetManager PrefabSetManager { get; }
        
        protected DialogViewerBase(
            ICameraProvider _CameraProvider,
            IUITicker _Ticker,
            IPrefabSetManager _PrefabSetManager)
        {
            CameraProvider = _CameraProvider;
            Ticker = _Ticker;
            PrefabSetManager = _PrefabSetManager;
        }
        
        public          IDialogPanel  CurrentPanel                { get; protected set; }
        public abstract RectTransform Container                   { get; }
        public          Func<bool>    IsOtherDialogViewersShowing { get; set; }
        
        public abstract void Init(RectTransform _Parent);
        
        protected IEnumerator DoTransparentTransition(
            RectTransform _Item,
            Dictionary<Graphic, float> _GraphicsAndAlphas,
            float _Time,
            bool _Disappear = false,
            UnityAction _OnFinish = null)
        {
            if (_Item == null)
                yield break;

            var graphicsAndAlphas = _GraphicsAndAlphas.ToList();
            _Item.gameObject.SetActive(true);
            var selectables = _Item.GetComponentsInChildrenEnabled<Selectable>();
            foreach (var button in selectables)
                button.Key.enabled = false;
            //do transition for graphic elements
            float currTime = Ticker.Time;
            Cor.Run(DoTranslucentBackgroundTransition(_Disappear, _Time));
            if (!_Disappear)
            {
                while (Ticker.Time < currTime + _Time)
                {
                    float timeCoeff = (currTime + _Time - Ticker.Time) / _Time;
                    float alphaCoeff = 1 - timeCoeff;
                    var collection = graphicsAndAlphas.ToList();
                    foreach (var ga in from ga 
                        in collection let graphic = ga.Key where !graphic.IsNull() select ga)
                        ga.Key.color = ga.Key.color.SetA(ga.Value * alphaCoeff);
                    yield return new WaitForEndOfFrame();
                }
            }
            //enable selectable elements (buttons, toggles, etc.)
            foreach (var button in selectables
                .Where(_Button => !_Button.Key.IsNull()))
                button.Key.enabled = button.Value;
            //set color alphas to finish values for all graphic elements
            foreach (var ga in graphicsAndAlphas.ToList())
            {
                var graphic = ga.Key;
                if (!graphic.IsNull())
                    graphic.color = graphic.color.SetA(_Disappear ? 0 : ga.Value);
            }
            if (_Disappear)
                _Item.gameObject.SetActive(false);
            _OnFinish?.Invoke();
        }

        private IEnumerator DoTranslucentBackgroundTransition(bool _Disappear, float _Time)
        {
            if (_Disappear)
            {
                CameraProvider.EnableEffect(ECameraEffect.DepthOfField, false);
                yield break;
            }
            CameraProvider.EnableEffect(ECameraEffect.DepthOfField, true);
            float currTime = Ticker.Time;
            while (Ticker.Time < currTime + _Time)
            {
                float timeCoeff = (currTime + _Time - Ticker.Time) / _Time;
                float blurAmount = 1 - timeCoeff;
                CameraProvider.SetEffectParameters(
                    ECameraEffect.DepthOfField, new FastDofProps {BlurAmount = blurAmount});
                yield return new WaitForEndOfFrame();
            }
        }
    }
}