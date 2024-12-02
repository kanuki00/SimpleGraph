using UnityEngine;
using UnityEngine.Events;
public class TimerTrigger : MonoBehaviour
{
    public float timerDuration = 5.0f;
    public UnityEvent onTimerEnd;

    private float remainingTime;
    private bool hasTriggered = false;

    private void Start() => remainingTime = timerDuration;

    private void Update()
    {
        if (!hasTriggered && (remainingTime -= Time.deltaTime) <= 0)
        {
            onTimerEnd.Invoke();
            hasTriggered = true;
        }
    }
}