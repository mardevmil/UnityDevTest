using UnityEngine;
using mardevmil.Core;

public class GameManager : MonoBehaviour
{    
    void Start()
    {
        var index = TimerManager.AddTimer(1f, false);
        TimerManager.Timers[index].onTimerFinished += Teeest;
        TimerManager.Timers[index].IsActive = true;
    }
    
    public void Update()
    {
        TimerManager.Update(Time.deltaTime);
    }

    private void OnDestroy()
    {
        TimerManager.Purge();
        DelayManager.Purge();
    }

    private void Teeest()
    {
        Debug.LogError("+++ Teeest");
    }
}
