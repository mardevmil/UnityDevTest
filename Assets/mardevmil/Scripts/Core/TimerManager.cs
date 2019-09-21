namespace mardevmil.Core
{
    public class TimerManager
    {
        private const int LIMIT = 128;

        public struct TimerData
        {
            public int id;
            public float duration;
            private bool _isActive;
            public bool IsActive
            {
                get { return _isActive; }
                set
                {
                    CurrentTime = duration;
                    _isActive = value;
                }
            }

            public float CurrentTime { get; private set; }
            public float NormalizedTime { get { return 1f - CurrentTime / duration; } }

            public System.Action onTimerFinished;
            public System.Action onTimerStoped;
            public System.Action onTimerPaused;
            public System.Action onTimerResumed;

            public bool Update(float deltaTime)
            {
                CurrentTime -= deltaTime;
                return CurrentTime <= 0f;
            }
        }

        private static TimerManager _instance = null;
        public static TimerManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TimerManager();

                return _instance;
            }

        }

        private TimerData[] _timers = new TimerData[LIMIT];
        public static TimerData[] Timers { get { return Instance._timers; } }


        #region Public API
        public static int AddTimer(float duration, bool isActive)
        {
            for (int i = 0; i < Instance._timers.Length; ++i)
            {
                if (Instance._timers[i].id == 0)
                {
                    var timer = new TimerData()
                    {
                        id = i + 1,
                        duration = duration,
                        IsActive = isActive
                    };

                    Instance._timers[i] = timer;
                    return i;
                }
            }
            UnityEngine.Debug.LogError("TimersManager count of timers exeeded limt of " + LIMIT);
            return default;
        }

        public static void Update(float deltaTime)
        {
            for (int i = 0; i < Instance._timers.Length; ++i)
            {
                if (Instance._timers[i].id > 0 && Instance._timers[i].IsActive)
                {
                    if (Instance._timers[i].Update(deltaTime))
                    {
                        Instance._timers[i].onTimerFinished?.Invoke();
                        Instance._timers[i] = default;
                    }
                }
            }
        }

        public static void StopAt(int index)
        {
            if (index >= 0)
            {
                Instance._timers[index].onTimerStoped?.Invoke();
                Instance._timers[index] = default;
            }
        }

        public static void PauseAt(int index)
        {
            if (index >= 0 && Instance._timers[index].IsActive)
            {
                Instance._timers[index].IsActive = false;
                Instance._timers[index].onTimerPaused?.Invoke();
            }
        }

        public static void ResumeAt(int index)
        {
            if (index >= 0 && !Instance._timers[index].IsActive && Instance._timers[index].CurrentTime < Instance._timers[index].duration)
            {
                Instance._timers[index].IsActive = true;
                Instance._timers[index].onTimerResumed?.Invoke();
            }
        }

        public static void Purge()
        {
            _instance = null;
        }
        #endregion
    }
}