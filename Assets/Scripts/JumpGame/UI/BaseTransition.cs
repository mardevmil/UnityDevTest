namespace JumpGame
{
    using System;
    using UnityEngine;

    public abstract class BaseTransition : ScriptableObject
    {
        public abstract void Initialize();
        public abstract void Show(BaseWindow t, Action<AbstractGoTween> onComplete);
        public abstract void Hide(BaseWindow t, Action<AbstractGoTween> onComplete);
        public abstract void Show(BaseWindow t, Action onComplete);
        public abstract void Hide(BaseWindow t, Action onComplete);
    }
}

