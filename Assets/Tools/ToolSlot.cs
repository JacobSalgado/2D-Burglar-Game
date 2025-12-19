using UnityEngine;

[System.Serializable]
public class ToolSlot
{
    public ToolData toolData;
    public int currentCharges;

    public ToolSlot(ToolData data, int charges)
    {
        toolData = data;
        currentCharges = charges;
    }
}
