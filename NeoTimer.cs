using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITimerSubscriber
{
    void OnTimerStart();
    void OnTimerEnd();
    void OnTimerUpdate(float remainingTime, float progress);
}

public class NeoTimer
{
    ///by   Neoxider
    /// <summary>
    ///     NewTimer timer = new NewTimer(1, 0.05f);
    ///     timer.OnTimerStart  += ()            => Debug.Log("Timer started");
    ///     timer.OnTimerUpdate += remainingTime => Debug.Log("Remaining time: " + remainingTime);
    ///     timer.OnTimerEnd    += ()            => Debug.Log("Timer ended");
    /// </summary>
    public event Action OnTimerStart;
    public event Action OnTimerEnd;
    public event Action<float, float> OnTimerUpdate;

    public float duration;
    private float updateInterval = 0.1f;
    private bool isRunning;
    private CancellationTokenSource cancellationTokenSource;

    public NeoTimer(float duration, float updateInterval = 0.1f)
    {
        this.duration = duration;
        this.updateInterval = updateInterval; 
    }

    public void ResetTimer(float newDuration, float newUpdateInterval = 0.1f)
    {
        StopTimer(); 
        duration = newDuration; 
        updateInterval = newUpdateInterval; 
    }

    public async void StartTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            OnTimerStart?.Invoke();
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await TimerCoroutine(cancellationTokenSource.Token);
                OnTimerEnd?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // Timer was cancelled
            }
            isRunning = false;
        }
    }

    public void StopTimer()
    {
        if (isRunning)
        {
            cancellationTokenSource.Cancel();
        }
    }

    public bool IsTimerRunning()
    {
        return isRunning;
    }

    private async Task TimerCoroutine(CancellationToken cancellationToken)
    {
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken);
            remainingTime -= updateInterval;
            float progress = (remainingTime / duration);  
            OnTimerUpdate?.Invoke(remainingTime, progress);
        }
    }
}