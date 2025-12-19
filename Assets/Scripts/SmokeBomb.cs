using System.Runtime.CompilerServices;
using UnityEngine;

public class SmokeBomb : MonoBehaviour
{
    [Header("Smoke Settings")]
    [SerializeField] private float smokeDuration = 5f;
    [SerializeField] private float smokeRadius = 3f;
    [SerializeField] private LayerMask visionBlockLayer;

    [Header("Visual")]
    [SerializeField] private Color smokeColor = new Color((float)0.5, 0.5f, 0.5f, 0.6f);

    private CircleCollider2D smokeCollider;
    private SpriteRenderer spriteRenderer;
    private bool isActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        smokeCollider = GetComponent<CircleCollider2D>();

        if (smokeCollider == null)
        { 
            smokeCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        smokeCollider.isTrigger = true;
        smokeCollider.radius = smokeRadius;

        // set to obstacle layer so it blocks guard vision
        gameObject.layer = LayerMask.NameToLayer("Obstacle");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        // fade out effect
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0.6f, 0f, 1f - (smokeDuration / 5f));
            spriteRenderer.color = color;
        }
    }

    public void Activate()
    {
        isActive = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = smokeColor;
        }

        // confuse guards inside smoke
        ConfuseGuardsInSmoke();

        Debug.Log("Smoke deployed - guards can't see through it");
    }

    private void ConfuseGuardsInSmoke()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, smokeRadius);

        foreach (Collider2D col in colliders)
        {
            EnemyAI guard = col.GetComponent<EnemyAI>();
            if (guard != null)
            {
                guard.LosePlayerSight();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, smokeRadius);
    }
}
