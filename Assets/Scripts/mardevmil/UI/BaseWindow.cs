using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseWindow : MonoBehaviour
{
    #region Injects
        
    public GuiManager GuiManager { get { return GuiManager.Instance; } }    
    public GameManager GameManager { get; set; }

    #endregion

    #region Public

    public Transform CachedTransform { get; set; }

    public bool Active
    {
        get { return _canvasGroup.gameObject.activeInHierarchy; }
        set { _canvasGroup.gameObject.SetActive(value); }
    }

    public float Alpha
    {
        get
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            return _canvasGroup.alpha;
        }
        set
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            _canvasGroup.alpha = value;
        }
    }

    public string Id { get; set; }

    #endregion

    #region Protected

    protected bool ButtonsAllowed;
    protected List<Button> Buttons;

    #endregion

    #region Private

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    //private PopupBackgroundController _popupBackgroundController;

    #region Widgets

    //private List<UIWidgetTransform> _widgets = new List<UIWidgetTransform>();

    //private readonly GoTweenConfig _show = new GoTweenConfig()
    //    .scale(Vector3.one)
    //    .setEaseType(GoEaseType.ElasticInOut)
    //    .setUpdateType(GoUpdateType.TimeScaleIndependentUpdate);

    //private readonly GoTweenConfig _hide = new GoTweenConfig()
    //    .scale(new Vector3(1, 0, 1))
    //    .setEaseType(GoEaseType.ElasticInOut)
    //    .setUpdateType(GoUpdateType.TimeScaleIndependentUpdate);

    #endregion

    #endregion

    #region Modal Window Actions


    #endregion

    protected void OnTap()
    {
        //MessageBroker.Default.Publish(new SoundEvent { Id = "OnClick" });
    }

    protected void OnNumberChange()
    {
        //MessageBroker.Default.Publish(new SoundEvent { Id = "OnCountDown" });
    }


    public virtual void Initialize()
    {
        _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        //_popupBackgroundController = GetComponentInChildren<PopupBackgroundController>(true);
        //_widgets = GetComponentsInChildren<UIWidgetTransform>().ToList();
        var buttons = GetComponentsInChildren<Button>();
        Buttons = new List<Button>(buttons.Length);
        for (int i = 0; i < buttons.Length; i++)
            Buttons.Add(buttons[i]);

        Id = name;
        CachedTransform = transform;

        Alpha = 0;
        Active = false;
    }

    public virtual void OnInit(string data = null)
    {
        Buttons.ForEach(button => button.interactable = true);

        ButtonsAllowed = false;
        ActivatePopupBackground();

        #region WIDGETS

        //for (var index = 0; index < _widgets.Count; index++)
        //    _widgets[index].transform.localScale = new Vector3(0, 0, 0);

        #endregion
    }

    public virtual void OnFocused()
    {
        ButtonsAllowed = true;
        Buttons.ForEach(button => button.interactable = true);

        if (Application.isEditor) name = Id + " [Active]";

        #region WIDGETS

        //Job.Make(EffectJob());

        #endregion
    }

    public virtual void OnUnfocused()
    {
        if (Application.isEditor) name = Id;

        ButtonsAllowed = false;
        Alpha = 0;
        Active = false;
    }

    public virtual void ActivatePopupBackground()
    {
        //if (_popupBackgroundController != null) _popupBackgroundController.Activate();
    }

    public virtual void DeactivatePopupBackground()
    {
        //if (_popupBackgroundController != null) _popupBackgroundController.Deactivate(() => promise.Resolve());        
    }    
}
