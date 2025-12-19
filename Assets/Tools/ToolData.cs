using UnityEngine;

[CreateAssetMenu(fileName = "ToolData", menuName = "Tools/ToolData")]
public class ToolData : ScriptableObject
{
    public string toolName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    public int maxCharges = 3;
}
