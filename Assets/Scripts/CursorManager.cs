using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 cursorHotspot = Vector2.zero; // hotspot at top-left, adjust for custom centers

    private void Start()
    {
        // set cursor when the game starts
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }
}
