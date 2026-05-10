using UnityEngine;
using UnityEngine.AI;

public class BotAI : MonoBehaviour
{
    public enum BotState { Idle, Patrol, Chase, Attack }
    public BotState currentState = BotState.Idle;

    [Header("Detection Settings")]
    public float detectionRange = 20f;
    public float attackRange = 10f;
    public float fieldOfView = 120f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public Transform[] patrolWaypoints;
    private int currentWaypointIndex;

    [Header("Combat")]
    public float fireRate = 0.5f;
    private float nextFireTime;
    public Transform shootPoint;
    public float damage = 10f;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private Health health;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (patrolWaypoints.Length > 0) currentState = BotState.Patrol;
    }

    void Update()
    {
        if (health != null && health.GetCurrentHealth() <= 0) 
        {
            agent.enabled = false;
            return;
        }

        float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

        switch (currentState)
        {
            case BotState.Patrol:
                Patrol();
                if (distanceToPlayer < detectionRange && CanSeePlayer()) currentState = BotState.Chase;
                break;

            case BotState.Chase:
                Chase();
                if (distanceToPlayer <= attackRange) currentState = BotState.Attack;
                else if (distanceToPlayer > detectionRange) currentState = BotState.Patrol;
                break;

            case BotState.Attack:
                Attack();
                if (distanceToPlayer > attackRange) currentState = BotState.Chase;
                break;
        }

        UpdateAnimations();
    }

    void Patrol()
    {
        if (patrolWaypoints.Length == 0) return;

        agent.speed = patrolSpeed;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
        }
    }

    void Chase()
    {
        agent.speed = chaseSpeed;
        if (player != null) agent.SetDestination(player.position);
    }

    void Attack()
    {
        agent.SetDestination(transform.position); // Stop moving
        if (player != null)
        {
            // Rotate towards player
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

            if (Time.time >= nextFireTime)
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
        nextFireTime = Time.time + fireRate;
        if (animator != null) animator.SetTrigger("Shoot");

        Debug.Log(gameObject.name + " shooting at Player!");

        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (hit.collider.TryGetComponent(out Health playerHealth))
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < fieldOfView / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToPlayer, out hit, detectionRange))
            {
                if (hit.collider.CompareTag("Player")) return true;
            }
        }
        return false;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Forward", speed / chaseSpeed);
    }
}
