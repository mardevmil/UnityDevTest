using UnityEngine;
using System;
[CreateAssetMenu(menuName = "Window Transitions/Slide Transition")]
public class SlideTransition : BaseTransition
{
    public GoEaseType Type = GoEaseType.ExpoInOut;

    private GoTweenConfig _showConfig;
    private GoTweenConfig _hideConfig;
    private GoTweenConfig _showPanelConfig;
    private GoTweenConfig _hidePanelConfig;

    public Vector3 _showStartVector3;
    public Vector3 _showFinishVector3;    
    public Vector3 _hideStartVector3;   
    public Vector3 _hideFinishVector3;

    [Range(0.1f, 2f)]
    public float ShowTransitionTime = 0.8f;

    [Range(0.1f, 2f)]
    public float HideTransitionTime = 0.5f;

    [Range(0.1f, 2f)]
    public float AlphaInDuration = 0.5f;

    [Range(0.1f, 2f)]
    public float AlphaOutDuration = 0.8f;

    public override void Initialize()
    {
        _showConfig = new GoTweenConfig().localPosition(Vector3.one).setEaseType(Type);
        _hideConfig = new GoTweenConfig().localPosition(_hideFinishVector3).setEaseType(Type);

        _showPanelConfig = new GoTweenConfig().floatProp("Alpha", 1f).setEaseType(Type);
        _hidePanelConfig = new GoTweenConfig().floatProp("Alpha", 0f).setEaseType(Type);
    }

    public override void Show(BaseWindow t, Action<AbstractGoTween> onComplete)
    {
        t.CachedTransform.localPosition = _showStartVector3;
        t.CachedTransform.localScale = Vector3.one;

        _showConfig.onCompleteHandler = onComplete;

        Go.to(t.CachedTransform, AlphaInDuration, _showConfig.setEaseType(Type));
        Go.to(t, ShowTransitionTime, _showPanelConfig);
    }

    public override void Hide(BaseWindow t, Action<AbstractGoTween> onComplete)
    {
        t.CachedTransform.localPosition = _hideStartVector3;

        _hideConfig.onCompleteHandler = onComplete;

        Go.to(t.CachedTransform, AlphaOutDuration, _hideConfig.setEaseType(Type));
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
