using UnityEngine;
using System.Collections;
using UnityEditor.Media;

public enum GuardState 
{ 
    Patrol,
    Investigate,
    Chase,
    Attack
}

public class EnemyAI : MonoBehaviour
{
    [Header("State")]
    public GuardState currentState = GuardState.Patrol;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints; // number of patrol points
    [SerializeField] private float patrolSpeed = 2f; // patrol speed
    [SerializeField] private float patrolWaitTime = 4f; // chase speed
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;

    [Header("Detection Settings")]
    [SerializeField] private float visionRange = 5f; // vision range
    [SerializeField] private float visionAngle = 45f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Investigation Settings")]
    [SerializeField] private float investigateSpeed = 3f;
    [SerializeField] private float investigateTime = 3f;
    private Vector2 investigatePosition;
    private float investigateTimer = 0f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float chaseRange = 7f;
    [SerializeField] private float loseTargetTime = 3f;
    private float loseTargetTimer = 0f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 10;
    private float attackTimer = 0f;

    [Header("Alert Settings")]
    [SerializeField] private float suspicionIncreaseRate = 20f;
    [SerializeField] private float suspicionDecreaseRate = 10f;
    [SerializeField] private float suspicionThreshold = 50f;
    private float currentSuspicion = 0f;

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 lastKnownPlayerPosition;
    private bool hasLineOfSight = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Disable gravity for top-down movement
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (patrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points assigned to EnemyAI.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer -= Time.deltaTime;

        // Check for player detection
        hasLineOfSight = DetectPlayer();

        // State Machine
        switch (currentState)
        {
            case GuardState.Patrol:
                HandlePatrolState();
                break;
            case GuardState.Investigate:
                HandleInvestigateState();
                break;
            case GuardState.Chase:
                HandleChaseState();
                break;
            case GuardState.Attack:
                HandleAttackState();
                break;
        }

        // Transition between states based on suspicion level
        HandleStateTransitions();
    }

    private void HandlePatrolState()
    { 
        if (patrolPoints.Length == 0) return;

        // decrease suspicion over time
        currentSuspicion = Mathf.Max(0, currentSuspicion - suspicionDecreaseRate * Time.deltaTime);

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        float distance = Vector2.Distance(transform.position, targetPoint.position);

        if (distance < 0.2f)
        {
            // reached patrol point
            patrolWaitTimer += Time.deltaTime;
            rb.linearVelocity = Vector2.zero;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                patrolWaitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
        else
        {
            // move towards patrol point
            Vector2 direction = (targetPoint.position - transform.position).normalized;
            rb.linearVelocity = direction * patrolSpeed;
            RotateTowards(direction);
        }
    }

    private void HandleInvestigateState()
    {
        investigateTimer += Time.deltaTime;

        float distance = Vector2.Distance(transform.position, investigatePosition);

        if ( distance < 0.3f )
        {
            // reached investigation point
            rb.linearVelocity = Vector2.zero;

            if (investigateTimer >= investigateTime)
            { 
                // Return to patrol after investigating
                TransitionToState(GuardState.Patrol);
            }
        }
        else
        {
            // move towards investigation point
            Vector2 direction = (investigatePosition - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * investigateSpeed;
            RotateTowards(direction);
        }

        // decrease suspicion during investigation
        currentSuspicion = Mathf.Max(0, currentSuspicion - suspicionDecreaseRate * 0.5f * Time.deltaTime);
    }

    private void HandleChaseState()
    {
        if (player == null)
        {
            TransitionToState(GuardState.Investigate);
            return;
        }

        if (hasLineOfSight)
        {
            // update last known position
            lastKnownPlayerPosition = player.position;
            loseTargetTime = 0f;

            // chase player
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
            RotateTowards(direction);
        }
        else
        {
            // lost sight, move to last known position
            loseTargetTimer += Time.deltaTime;

            Vector2 direction = (lastKnownPlayerPosition - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
            RotateTowards(direction);

            // if lost target for too long, investigate 
            if (loseTargetTimer >= loseTargetTime)
            {
                investigatePosition = lastKnownPlayerPosition;
                TransitionToState(GuardState.Investigate);
            }
        }
    }

    private void HandleAttackState()
    {
        if (player == null)
        {
            TransitionToState(GuardState.Patrol);
            return;
        }

        // stop moving
        rb.linearVelocity = Vector2.zero;

        // face player
        Vector2 direction = (player.position - transform.position).normalized;
        RotateTowards(direction);

        // attack if cooldown is over
        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    private void HandleStateTransitions()
    {
        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        switch (currentState)
        {
            case GuardState.Patrol:
                if (hasLineOfSight)
                {
                    currentSuspicion += suspicionIncreaseRate * Time.deltaTime;
                    if (currentSuspicion >= suspicionThreshold)
                    {
                        TransitionToState(GuardState.Chase);
                    }
                }
                break;

            case GuardState.Investigate:
                if (hasLineOfSight)
                {
                    TransitionToState(GuardState.Chase);
                }
                break;

            case GuardState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    TransitionToState(GuardState.Attack);
                }
                else if (!hasLineOfSight && loseTargetTimer >= loseTargetTime)
                { 
                    // handled in handleChaseState
                }
                break;
            case GuardState.Attack:
                if (distanceToPlayer > attackRange * 1.2f)
                {
                    TransitionToState(GuardState.Chase);
                }
                break;

        }
    }

    private bool DetectPlayer()
    {
        if (player == null) return false;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // check if in range
        if (distanceToPlayer > visionRange) return false;

        // check if in vision cone
        float angleToPlayer = Vector2.Angle(transform.up, directionToPlayer);
        if (angleToPlayer > visionAngle) return false;
        
        // check line of sight (no obstacles)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, playerLayer | obstacleLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }

    private void PerformAttack()
    {
        Debug.Log(gameObject.name + " attacks player!");

        // deal damage to player
        if (player != null)
        { 
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    private void TransitionToState(GuardState newState)
    {
        // exit current state
        switch (currentState)
        {
            case GuardState.Investigate:
                investigateTimer = 0f;
                break;

            case GuardState.Chase:
                loseTargetTimer = 0f;
                break;
        }

        // enter new state
        currentState = newState;

        switch (newState)
        {
            case GuardState.Patrol:
                currentSuspicion = 0f;
                break;

            case GuardState.Investigate:
                investigateTimer = 0f;
                break;

            case GuardState.Chase:
                if (player != null)
                {
                    lastKnownPlayerPosition = player.position;
                }
                loseTargetTimer = 0f;
                break;
        }
    }

    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // visualization in editor
    private void OnDrawGizmosSelected()
    {
        // vision cone
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.up * visionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // patrol points
        if (patrolPoints != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }
            }

            // connect last to first
            if (patrolPoints.Length > 0 && patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
            }
        }

        // attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
