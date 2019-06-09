using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiManager : Singleton<GuiManager>
{
    public enum TransitionType
    {
        Scale,
        Slide
    }

    [SerializeField]
    private BaseTransition _scaleTransition;
    [SerializeField]
    private BaseTransition _slideTransition;

    [SerializeField]
    private TransitionType _transitionType = TransitionType.Scale;
    private BaseTransition _selectedTransition;

    private bool _inPopupShow, _inPopupHide;
    private bool _inWindowShow, _inWindowHide;
    
    private Dictionary<string, BaseWindow> _windowMap = new Dictionary<string, BaseWindow>();

    private GameObject _canvas;
    public Canvas Canvas { get { return _canvas.GetComponent<Canvas>(); } }

    private Camera _uiCamera;
    public Camera UICamera { get { return _uiCamera; } }

    public BaseWindow CurrentPopup { get; set; }
    public BaseWindow CurrentWindow { get; set; }
    
    public event Action<BaseWindow> OnCurrentWindowChange;
    public event Action<BaseWindow> OnCurrentPopupChange;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Start()
    {
        ShowWindow("UI_MAIN_MENU"); // just for test, it is job for GameManager (to show first window)
    }

    #region Initialization
    public void Initialize()
    {

        //Go.defaultUpdateType = GoUpdateType.TimeScaleIndependentUpdate;

        Debug.Log("GuiManager Initialization");

        _canvas = GetComponentInChildren<Canvas>().gameObject;

        _uiCamera = GetComponentInChildren<Camera>();

        var initializationSuccess = true;

        var windows = GetComponentsInChildren<BaseWindow>();

        if (windows.Length == 0)
        {

            initializationSuccess = false;
            Debug.LogError("GuiManager failed to initialize [No screens found]");
        }
        else
        {
            for (int i = 0; i < windows.Length; i++)
            {
                windows[i].Initialize();
                _windowMap.Add(windows[i].Id, windows[i]);
            }
        }

        // NotificationController
        //NotificationController = GetComponentInChildren<NotificationWindow>();
        //if (NotificationController == null)
        //{
        //    initializationSuccess = false;
        //    promise.Reject(new ApplicationException("GuiManager failed to initialize [NotificationController is null]"));
        //}

        // OverlayController
        //OverlayController = GetComponentInChildren<WindowOverlayController>();
        //if (OverlayController == null)
        //{
        //    initializationSuccess = false;
        //    promise.Reject(new ApplicationException("GuiManager failed to initialize [OverlayController is null]"));
        //}
        //else
        //{
        //    OverlayController.Initialize();
        //}

        // ClickController        
        //ClickController = GetComponentInChildren<ClickPreventionController>();
        //if (ClickController == null)
        //{
        //    initializationSuccess = false;
        //    promise.Reject(new ApplicationException("GuiManager failed to initialize [ClickController is null]"));
        //}
        //else
        //{
        //    ClickController.Initialize();
        //}

        // BackgroundController
        //BackgroundController = GetComponentInChildren<BackgroundController>();
        //if (BackgroundController == null)
        //{
        //    initializationSuccess = false;
        //    promise.Reject(new ApplicationException("GuiManager failed to initialize [BackgroundController is null]"));
        //}

        // TapToPlayController
        //TapToPlayController = GetComponentInChildren<TapToPlayController>();
        //if (TapToPlayController == null)
        //{
        //    initializationSuccess = false;
        //    promise.Reject(new ApplicationException("GuiManager failed to initialize [TapToPlayController is null]"));
        //}
        //HideTapToPlay();

        // Layer color controllers
        ///_layerColorControllers = GetComponentsInChildren<BackgroundLayerColorController>().ToList();
        
        _scaleTransition.Initialize();
        _slideTransition.Initialize();
        switch (_transitionType)
        {
            case TransitionType.Scale:
                _selectedTransition = _scaleTransition;
                break;
            case TransitionType.Slide:
                _selectedTransition = _slideTransition;
                break;            
        }
        //_uiNotificationPool = new GameObjectsPool(UIElementFactory, 10, 0, "_UI Notification Pool", _canvas.transform);

        //_uiNotificationPool.OnSpawn += item => { };
        //_uiNotificationPool.OnDespawn += item => { };
        //_uiNotificationPool.Initialize();

        //_uiFollowPool = new GameObjectsPool(UIFollowFactory, 10, 1, "_UI Follow Pool", _canvas.transform);
        //_uiFollowPool.OnSpawn += item => { };
        //_uiFollowPool.OnDespawn += item => { };
        //_uiFollowPool.Initialize();


        //if (Masking != null) Masking.enabled = false;        
    }
    #endregion

    #region Show/Hide Window

    public void ShowWindow(string windowName, UnityEngine.Object data = null)
    {
        Debug.Log("Show window: " + windowName);               
        if(!_inWindowShow)
        {
            _inWindowShow = true;

            if (_windowMap.ContainsKey(windowName))
            {
                var newWindow = _windowMap[windowName];

                if (CurrentWindow != null)
                {
                    if (CurrentWindow.Id != windowName)
                    {
                        var previous = CurrentWindow;
                        
                        HideWindow(previous.Id, () =>                        
                        {
                            newWindow.Active = true;
                            newWindow.OnInit();

                            _selectedTransition.Show(newWindow, tween =>
                            {
                                CurrentWindow = newWindow;
                                newWindow.OnFocused();

                                _inWindowShow = false;

                                OnCurrentWindowChange?.Invoke(CurrentWindow);
                                
                            });
                        });
                    }
                    else
                    {                        
                        _inWindowShow = false;
                    }
                }
                else
                {
                    newWindow.Active = true;
                    newWindow.OnInit();

                    _selectedTransition.Show(newWindow, tween =>
                    {
                        CurrentWindow = newWindow;
                        newWindow.OnFocused();

                        _inWindowShow = false;

                        OnCurrentWindowChange?.Invoke(CurrentWindow);
         
                    });
                }
            }
        }        
    }

    public void HideWindow(string windowName, Action onComplete = null)
    {
        if(!_inWindowHide)
        {
            _inWindowHide = true;

            if (_windowMap.ContainsKey(windowName))
            {
                var window = _windowMap[windowName];
                _selectedTransition.Hide(window, tween =>
                {                    
                    CurrentWindow = null;
                    onComplete?.Invoke();                    
                    window.Alpha = 0;
                    window.OnUnfocused();
                    
                    _inWindowHide = false;                    
                });
            }
        }        
    }

    #endregion

    #region Show/Hide Popup

    public void ShowPopup(string windowName, string data = null)
    {
        //Debug.Log(data);
        //ClickController.Activate();        
        if (!_inPopupShow)
        {
            _inPopupShow = true;

            if (_windowMap.ContainsKey(windowName))
            {
                var newPopup = _windowMap[windowName];

                if (CurrentPopup != null)
                {
                    if (CurrentPopup.Id != windowName)
                    {
                        var previous = CurrentPopup;
                        HidePopup(previous.Id, () =>
                        {
                            newPopup.Active = true;                            
                            newPopup.OnInit(data);

                            _selectedTransition.Show(newPopup, tween =>
                            {
                                //ClickController.Deactivate();

                                CurrentPopup = newPopup;
                                newPopup.OnFocused();

                                _inPopupShow = false;

                                OnCurrentPopupChange?.Invoke(CurrentPopup);                                
                            });
                        });
                    }
                    else
                    {                        
                        _inPopupShow = false;
                    }
                }
                else
                {
                    newPopup.Active = true;
                    newPopup.OnInit(data);

                    _selectedTransition.Show(newPopup, tween =>
                    {
                        //ClickController.Deactivate();

                        CurrentPopup = newPopup;
                        newPopup.OnFocused();

                        _inPopupShow = false;

                        OnCurrentPopupChange?.Invoke(CurrentPopup);
                    });
                }
            }
        }        
    }

    public void HidePopup(string windowName, Action onComplete = null)
    {
        //ClickController.Activate();
        
        if(!_inPopupHide)
        {
            _inPopupHide = true;

            if (_windowMap.ContainsKey(windowName))
            {
                var window = _windowMap[windowName];

                if (CurrentPopup != null && CurrentPopup.Id == windowName)
                {
                    //window.DeactivatePopupBackground().Then(() =>
                    //{
                        _selectedTransition.Hide(window, tween =>
                        {
                            CurrentPopup = null;
                            onComplete?.Invoke();
                            window.Alpha = 0;
                            window.OnUnfocused();

                            _inPopupHide = false;
                            //ClickController.Deactivate();                            
                        });
                    //});
                }
                else
                {
                    _inPopupHide = false;
                    //ClickController.Deactivate();                    
                }
            }
        }        
    }

    #endregion
}
