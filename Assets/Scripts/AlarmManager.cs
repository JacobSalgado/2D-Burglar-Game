using UnityEngine;
using UnityEngine.Events;

public class AlarmManager : MonoBehaviour
{
    public static AlarmManager Instance;

    [Header("Alarm Settings")]
    [SerializeField] private int maxAlarmLevel = 100;
    [SerializeField] private float alarmIncreaseRate = 10f; // per second when guards are chasing
    [SerializeField] private float alarmDecreaseRate = 2f; // per second when calm

    [Header("Alarm State")]
    [SerializeField] private int currentAlarmLevel = 0;
    [SerializeField] private bool alarmActive = false;
    [SerializeField] private bool alarmDisabled = false;
    [SerializeField] private float disableTimer = 0f;

    [Header("Reward Multipliers")]
    [SerializeField] private float[] rewardMultipliers = { 3f, 2f, 1.5f, 1f, 0.5f }; // based on alarm level thresholds
    [SerializeField] private int[] alarmThresholds = { 20, 40, 60, 80, 100 }; // alarm level ranges

    [Header("Events")]
    public UnityEvent<int> OnAlarmLevelChanged; // passes current alarm level
    public UnityEvent OnAlarmTriggered;
    public UnityEvent OnAlarmDeactivated;
    public UnityEvent OnAlarmDisabled;
    public UnityEvent OnAlarmReEnabled;

    private void Awake()
    {
        // singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        { 
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // handle alarm disable timer
        if (alarmDisabled)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0f)
            {
                ReEnableAlarm();
            }
            return; // don't process alarm changes while disabled
        }

        // increase of decrease alarm based on state
        if (alarmActive)
        {
            IncreaseAlarm(alarmIncreaseRate * Time.deltaTime);
        }
        else
        {
            DecreaseAlarm(alarmDecreaseRate * Time.deltaTime);
        }
    }

    public void TriggerAlarm()
    {
        if (alarmDisabled) return;

        if (!alarmActive)
        {
            alarmActive = true;
            OnAlarmTriggered?.Invoke();
            Debug.Log("ALARM TRIGGERED");
        }
    }

    public void DeactivateAlarm()
    {
        if (alarmActive)
        {
            alarmActive = false;
            OnAlarmDeactivated?.Invoke();
            Debug.Log("Alarm deactivated (Guards lost sight)");
        }
    }

    public void IncreaseAlarm(float amount)
    {
        if (alarmDisabled) return;

        int previousLevel = currentAlarmLevel;
        currentAlarmLevel = Mathf.Min(maxAlarmLevel, currentAlarmLevel + (int)amount);

        if (currentAlarmLevel != previousLevel)
        {
            OnAlarmLevelChanged?.Invoke(currentAlarmLevel);
        }
    }

    public void DecreaseAlarm(float amount)
    {
        int previousLevel = currentAlarmLevel;
        currentAlarmLevel = Mathf.Max(0, currentAlarmLevel - (int)amount);
        
        if (currentAlarmLevel != previousLevel)
        {
            OnAlarmLevelChanged?.Invoke(currentAlarmLevel);
        }
    }

    public void DisableAlarmFor(float duration)
    {
        alarmDisabled = true;
        disableTimer = duration;
        alarmActive = false; // stop any active alarm

        OnAlarmDisabled?.Invoke();
        Debug.Log($"Alarm DISABLED for {duration} seconds");
    }

    private void ReEnableAlarm()
    {
        alarmDisabled = false;
        disableTimer = 0f;

        OnAlarmReEnabled?.Invoke();
        Debug.Log("Alarm system Re-enabled");
    }

    public void ReduceAlarmLevel(int amount)
    {
        int previousLevel = currentAlarmLevel;
        currentAlarmLevel = Mathf.Max(0, currentAlarmLevel - amount);

        if (currentAlarmLevel != previousLevel)
        {
            OnAlarmLevelChanged?.Invoke(currentAlarmLevel);
        }

        Debug.Log($"Alarm reduced by {amount}. Current: {currentAlarmLevel}");
    }

    public void ResetAlarm()
    {
        currentAlarmLevel = 0;
        alarmActive = false;
        alarmDisabled = false;
        disableTimer = 0f;
        OnAlarmLevelChanged?.Invoke(currentAlarmLevel);
    }

    // getter functions
    public int GetCurrentAlarmLevel() => currentAlarmLevel;
    public int GetMaxAlarmLevel() => maxAlarmLevel;
    public float GetAlarmPercent() => (float)currentAlarmLevel / maxAlarmLevel;
    public bool IsAlarmActive() => alarmActive;
    public bool isAlarmDisabled() => alarmDisabled;
    public float GetDisableTimeRemaining() => disableTimer;

    // Calculate reward multiplier based on alarm level
    public float CalculateRewardMultiplier()
    {
        for (int i = 0; i < alarmThresholds.Length; i++)
        {
            if (currentAlarmLevel <= alarmThresholds[i])
            {
                return rewardMultipliers[i];
            }
        }
        return rewardMultipliers[rewardMultipliers.Length - 1]; // worst multiplier
    }

    public string GetAlarmStatus()
    {
        if (alarmDisabled)
            return "Disabled";
        else if (currentAlarmLevel < 20)
            return "Clear";
        else if (currentAlarmLevel < 50)
            return "Caution";
        else if (currentAlarmLevel < 80)
            return "Alert";
        else
            return "Critical";
    }
}
