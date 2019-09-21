namespace mardevmil.UI
{

    using System;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Window Transitions/Scale Transition")]
    public class ScaleTransition : BaseTransition
    {
        public GoEaseType Type = GoEaseType.ExpoInOut;

        private GoTweenConfig _showConfig;
        private GoTweenConfig _hideConfig;
        private GoTweenConfig _showPanelConfig;
        private GoTweenConfig _hidePanelConfig;

        public Vector3 _startVector3 = Vector3.zero;
        public Vector3 _finishVector3 = Vector3.zero;
        public Vector3 _upscaled = new Vector3(16, 16, 16);
        public Vector3 _downscaled = new Vector3(0.01f, 0.01f, 0.01f);

        [Range(0.1f, 2f)]
        public float ShowTransitionTime = 0.8f;
        [Range(0.1f, 2f)]
        public float HideTransitionTime = 0.5f;
        [Range(0.1f, 2f)]
        public float AlphaInDuration = 0.5f;
        [Range(0.1f, 2f)]
        public float AlphaOutDuration = 0.8f;
        [Range(0.1f, 2f)]
        public float Duration = 1f;

        public override void Initialize()
        {
            _showConfig = new GoTweenConfig().scale(Vector3.one).setEaseType(Type).setUpdateType(GoUpdateType.TimeScaleIndependentUpdate);
            _showPanelConfig = new GoTweenConfig().floatProp("Alpha", 1f).setEaseType(Type).setUpdateType(GoUpdateType.TimeScaleIndependentUpdate);

            _hideConfig = new GoTweenConfig().scale(_upscaled).setEaseType(Type).setUpdateType(GoUpdateType.TimeScaleIndependentUpdate);
            _hidePanelConfig = new GoTweenConfig().floatProp("Alpha", 0f).setEaseType(Type).setUpdateType(GoUpdateType.TimeScaleIndependentUpdate);
        }

        public override void Show(BaseWindow t, Action<AbstractGoTween> onComplete)
        {
            t.CachedTransform.localPosition = _startVector3;
            t.CachedTransform.localScale = _downscaled;

            _showConfig.onCompleteHandler = onComplete;
            Go.to(t.CachedTransform, AlphaInDuration, _showConfig);
            Go.to(t, ShowTransitionTime, _showPanelConfig);
        }

        public override void Hide(BaseWindow t, Action<AbstractGoTween> onComplete)
        {
            t.CachedTransform.localPosition = _finishVector3;

            _hideConfig.onCompleteHandler = onComplete;

            Go.to(t.CachedTransform, AlphaOutDuration, _hideConfig);
            Go.to(t, HideTransitionTime, _hidePanelConfig);
        }

        public override void Show(BaseWindow t, Action onComplete)
        {
            Debug.Log("Not implemented");
        }

        public override void Hide(BaseWindow t, Action onComplete)
        {
            Debug.Log("Not implemented");
        }
    }

}