namespace mardevmil.Core
{
    public class DelayManager
    {
        private static DelayManager _instance = null;
        public static DelayManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DelayManager();

                return _instance;
            }

        }

        #region Static API
    
        public static int DelayAction(System.Action action, float delay)
        {
            if (_instance == null)
                _instance = new DelayManager();

            var index = TimerManager.AddTimer(delay, false);        
            TimerManager.Timers[index].onTimerFinished += action;
            TimerManager.Timers[index].IsActive = true;            
            return index;
        }

        public static void StopAt(int index)
        {
            TimerManager.StopAt(index);
        }

        public static void Purge()
        {
            _instance = null;
        }

        #endregion

    }
}
