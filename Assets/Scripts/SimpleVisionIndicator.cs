using UnityEngine;

public class SimpleVisionIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private SpriteRenderer visionSprite;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 0f, 0.2f); // yellow in RGBA format
    [SerializeField] private Color alertColor = new Color(1f, 0.5f, 0f, 0.3f); // orange
    [SerializeField] private Color chaseColor = new Color(1f, 0f, 0f, 0.4f); // red

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get reference if not assigned
        if (enemyAI != null)
            enemyAI = GetComponentInParent<EnemyAI>();

        if (visionSprite != null)
            visionSprite = GetComponent<SpriteRenderer>();

        // set initial properties
        if (visionSprite != null)
        {
            visionSprite.sortingOrder = -1; // behind player/enemies
            visionSprite.color = normalColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateVisionColor();
    }

    private void UpdateVisionColor()
    {
        if (enemyAI == null || visionSprite == null) return;

        Color targetColor;

        switch (enemyAI.currentState)
        {
            case GuardState.Patrol:
                targetColor = normalColor;
                break;

            case GuardState.Investigate:
                targetColor = alertColor;
                break;

            case GuardState.Chase:
            case GuardState.Attack:
                targetColor = chaseColor;
                break;

            default:
                targetColor = normalColor;
                break;
        }

        // smooth color transition
        visionSprite.color = Color.Lerp(visionSprite.color, targetColor, Time.deltaTime * 5f);
    }
}
