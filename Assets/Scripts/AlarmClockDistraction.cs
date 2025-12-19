using UnityEngine;

public class AlarmClockDistraction : MonoBehaviour
{
    [Header("Distraction Settings")]
    [SerializeField] private float distractionRadius = 8f;
    [SerializeField] private float distractionDuration = 5f;
    [SerializeField] private bool playSound = true;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject visualEffect; // optional particle effect
    [SerializeField] private Color glowColor = Color.yellow;

    private SpriteRenderer spriteRenderer;
    private float timer = 0f;
    private bool isActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;

        // pulsing effect
        if (spriteRenderer != null)
        {
            float pulseAlpha = Mathf.PingPong(Time.time * 3f, 1f);
            Color color = glowColor;
            color.a = pulseAlpha;
            spriteRenderer.color = color;
        }

        // destroy after duration
        if (timer >= distractionDuration)
        {
            Destroy(gameObject);
        }
    }

    public void Activate()
    {
        isActive = true;
        AlertNearbyGuards();

        if (visualEffect != null)
        {
            Instantiate(visualEffect, transform.position, Quaternion.identity, transform);
        }

        //  play sound
        /*if (playSound)
        { 
            // AudioSource.PlayClipAtPoint(alarmSound, transform.position);
        }*/
    }

    private void AlertNearbyGuards()
    {
        // find all guards in radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, distractionRadius);

        foreach (Collider2D col in colliders)
        {
            EnemyAI guard = col.GetComponent<EnemyAI>();
            if (guard != null)
            {
                guard.InvestigatePosition(transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distractionRadius);
    }
}
