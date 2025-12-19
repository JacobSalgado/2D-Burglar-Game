using UnityEngine;
using UnityEngine.Events;

public class PlayerToolSystem : MonoBehaviour
{
    [Header("Tool References")]
    [SerializeField] private ToolData alarmClockData;
    [SerializeField] private ToolData smokeBombData;
    [SerializeField] private ToolData alarmDisablerData;

    [Header("Tool Inventory")]
    [SerializeField] private ToolSlot alarmClockSlot;
    [SerializeField] private ToolSlot smokeBombSlot;
    [SerializeField] private ToolSlot alarmDisablerSlot;

    [Header("Input Keys")]
    [SerializeField] private KeyCode alarmClockKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode smokeBombKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode alarmDisablerKey = KeyCode.Alpha3;

    [Header("Prefabs")]
    [SerializeField] private GameObject alarmClockPrefab;
    [SerializeField] private GameObject smokeBombPrefab;

    [Header("Settings")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float maxThrowDistance = 10f;

    [Header("Events")]
    public UnityEvent<int> OnToolUsed; // pass tool index
    public UnityEvent<int, int> OnToolChargesChanged; // pass tool index and remaining charges

    [Header("Camera Reference")]
    [SerializeField] private Camera gameCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize tool slots
        alarmClockSlot = new ToolSlot(alarmClockData, alarmClockData ? alarmClockData.maxCharges : 3);
        smokeBombSlot = new ToolSlot(smokeBombData, smokeBombData ? smokeBombData.maxCharges : 3);
        alarmDisablerSlot = new ToolSlot(alarmDisablerData, alarmDisablerData ? alarmDisablerData.maxCharges : 2);
    }

    // Update is called once per frame
    void Update()
    {
        // tool input handling
        if (Input.GetKeyDown(alarmClockKey))
            UseAlarmClock();

        if (Input.GetKeyDown(smokeBombKey))
            UseSmokeBomb();

        if (Input.GetKeyDown(alarmDisablerKey))
            UseAlarmDisabler();
    }

    // #region Tool Usage Methods

    private void UseAlarmClock()
    {
        Debug.Log("Alarm clock function being called");

        if (!CanUseTool(alarmClockSlot, 0))
        {
            Debug.Log("Cannot use alarm cloock - failed CanUseTool check");
            return;
        } 

        Vector2 throwPosition = GetMouseWorldPosition();

        // clamp throw distance
        Vector2 playerPos = transform.position;
        Vector2 direction = (throwPosition - playerPos).normalized;
        float distance = Vector2.Distance(playerPos, throwPosition);

        if (distance > maxThrowDistance)
        {
            throwPosition = playerPos + direction * maxThrowDistance;
        }

        // spawn alarm clock at position
        if (alarmClockPrefab != null)
        {
            GameObject alarmClock = Instantiate(alarmClockPrefab, throwPosition, Quaternion.identity);
            AlarmClockDistraction distraction = alarmClock.GetComponent<AlarmClockDistraction>();
            if (distraction != null)
            {
                distraction.Activate();
            }
        }
        else
        {
            // fallback: directly alert guards
            AlertNearbyGuards(throwPosition, 8f);
        }

        ConsumeToolCharge(alarmClockSlot, 0);
        Debug.Log("Alarm Clock thrown to distract guards");
    }

    private void UseSmokeBomb()
    {
        if (!CanUseTool(smokeBombSlot, 1)) return;

        Vector2 spawnPosition = transform.position;

        // spawn smoke bomb at player position
        if (smokeBombPrefab != null)
        {
            GameObject smoke = Instantiate(smokeBombPrefab, spawnPosition, Quaternion.identity);
            SmokeBomb smokeBombScript = smoke.GetComponent<SmokeBomb>();

            if (smokeBombScript != null)
                smokeBombScript.Activate();

            Destroy(smoke, 5f); // smoke lasts 5 seconds
        }

        else
        {
            Debug.LogWarning("Smoke Bomb prefab not assigned");
        }

        ConsumeToolCharge(smokeBombSlot, 1);
        Debug.Log("Smoke bomb deployed");
    }

    void UseAlarmDisabler()
    {
        if (!CanUseTool(alarmDisablerSlot, 2)) return;

        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.DisableAlarmFor(30f); // disable for 30 seconds
        }
        else
        {
            Debug.LogWarning("AlarmManager not found");
        }

        ConsumeToolCharge(alarmDisablerSlot, 2);
        Debug.Log("Alarm system Disabled for 30 seconds");
    }

    // Helper methods
    bool CanUseTool(ToolSlot slot, int toolIndex)
    {
        if (slot == null || slot.toolData == null)
        {
            Debug.LogWarning($"Tool {toolIndex} not configured");
            return false;
        }

        if ( (slot.currentCharges <= 0))
        {
            Debug.Log($"{slot.toolData.toolName} has no charges left");
            return false;
        }

        return true;
    }

    void ConsumeToolCharge(ToolSlot slot, int toolIndex)
    {
        slot.currentCharges--;
        OnToolUsed?.Invoke(toolIndex);
        OnToolChargesChanged?.Invoke(toolIndex, slot.currentCharges);
    }

    void AlertNearbyGuards(Vector2 position, float radius)
    {
        // find all guards and make them investigate the position
        EnemyAI[] guards = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI guard in guards)
        {
            float distance = Vector2.Distance(guard.transform.position, position);
            if ( distance < radius )
            {
                // make guard investigate this position
                guard.InvestigatePosition(position);
            }
        }
    }

    Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    // public methods for UI and game management

    public void RefillAllTools()
    {
        alarmClockSlot.currentCharges = alarmClockSlot.toolData.maxCharges;
        smokeBombSlot.currentCharges = smokeBombSlot.toolData.maxCharges;
        alarmDisablerSlot.currentCharges = alarmDisablerSlot.toolData.maxCharges;

        OnToolChargesChanged?.Invoke(0, alarmClockSlot.currentCharges);
        OnToolChargesChanged?.Invoke(1, smokeBombSlot.currentCharges);
        OnToolChargesChanged?.Invoke(2, alarmDisablerSlot.currentCharges);

        Debug.Log("All tools refilled");
    }

    public void RefillTool(int toolIndex)
    {
        ToolSlot slot = GetToolSlot(toolIndex);
        if (slot != null)
        {
            slot.currentCharges = slot.toolData.maxCharges;
            OnToolChargesChanged?.Invoke(toolIndex, slot.currentCharges);
        }
    }

    public int GetToolCharges(int toolIndex)
    {
        ToolSlot slot = GetToolSlot(toolIndex);
        return slot != null ? slot.currentCharges : 0;
    }

    public ToolData GetToolData(int toolIndex)
    {
        ToolSlot slot = GetToolSlot(toolIndex);
        return slot?.toolData;
    }

    ToolSlot GetToolSlot(int index)
    {
        switch (index)
        {
            case 0: return alarmClockSlot;
            case 1: return smokeBombSlot;
            case 2: return alarmDisablerSlot;
            default: return null;
        }
    }
}
