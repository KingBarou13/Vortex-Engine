using UnityEngine;
using System;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager Instance { get; private set; }

    public bool IsInTimeTrial { get; private set; }
    public float CurrentTime { get; private set; }

    private TimeTrialFinishGate _activeFinishGate;

    public event Action OnTimeTrialStarted;
    public event Action<float> OnTimeTrialFinished;
    public event Action<float> OnTimeUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        if (IsInTimeTrial)
        {
            CurrentTime += Time.deltaTime;
            OnTimeUpdated?.Invoke(CurrentTime);
        }
    }

    public void TryStartTimeTrial(TimeTrialStartGate startGate, TimeTrialFinishGate finishGate)
    {
        if (!IsInTimeTrial)
        {
            IsInTimeTrial = true;
            _activeFinishGate = finishGate;
            CurrentTime = 0f;
            Debug.Log("Time Trial Started!");
            OnTimeTrialStarted?.Invoke();
        }
    }

    public void TryFinishTimeTrial(TimeTrialFinishGate finishGate)
    {
        if (IsInTimeTrial && finishGate == _activeFinishGate)
        {
            IsInTimeTrial = false;
            Debug.Log($"Time Trial Finished! Final Time: {FormatTime(CurrentTime)}");
            OnTimeTrialFinished?.Invoke(CurrentTime);
            _activeFinishGate = null;
        }
    }

    public static string FormatTime(float timeInSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);
        return time.ToString(@"mm\:ss\.fff");
    }
}