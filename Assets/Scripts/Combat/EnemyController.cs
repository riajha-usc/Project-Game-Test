using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Settings")]
    [SerializeField] private float chaseRange = 4.0f;
    [SerializeField] private float stoppingDist = 2f;
    [SerializeField] public float damageRange = 4.0f;

    [Header("CombatParams")]
    private float hp = 70f;
    private float maxHp = 0;
    public Slider enemyhealthbar;
    //public TMP_Text enemyhealthtxt;

    private NavMeshAgent agent;
    private Vector3 spawnPoint;
    public GameObject projectilePrefab;
    private float lastAttackTime;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);

    public enum EnemyState
    {
        Idle,
        Chase,
        Return
    }
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    void Start()
    {
        InitializeAgent();
        SnapToNavMesh();

        spawnPoint = transform.position;
        
        FindPlayer();

        currentState = EnemyState.Idle;
        maxHp = hp;
        agent.isStopped = true;
        if (enemyhealthbar != null)
        {
            enemyhealthbar.transform.position = transform.position + healthBarOffset;
        }
    }

    void Update()
    {
        if (!IsValidSetup()) return;
        //if (enemyhealthtxt != null) enemyhealthtxt.text = hp + " /" + maxHp;
        if (enemyhealthbar != null) enemyhealthbar.value = hp / maxHp;
        UpdateState();
        HandleState();
        AttackHandler();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            //Debug.Log("Player Object Found!");
        }
        else
        {
            Debug.LogWarning("Player Object NOT Found!");
        }
    }

    private void InitializeAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        if(agent == null)
        {
            Debug.Log("NavMeshAgent component missing on " + gameObject.name);
            return;
        }
        agent.stoppingDistance = stoppingDist;
    }

    private void SnapToNavMesh()
    {
        if (agent == null) return;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            //Debug.Log($"{gameObject.name} snapped to NavMesh at {hit.position}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} could not snap to NavMesh. Ensure NavMesh is baked.");
        }
    }

    private bool IsValidSetup()
    {
        return player != null && agent != null && agent.isOnNavMesh;
    }

    void UpdateState()
    {
        if (SafeZoneController.InSafeZone)
        {
            currentState = EnemyState.Return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if(distanceToPlayer <= chaseRange && !SafeZoneController.InSafeZone)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Return;
        }
    }

    void HandleState()
    {
        switch(currentState)
        {
            case EnemyState.Idle:
                agent.isStopped = true;
                break;

            case EnemyState.Chase:
                agent.isStopped = false;
                agent.stoppingDistance = stoppingDist;

                Vector3 direction = (player.position - spawnPoint).normalized;
                float distanceFromSpawn = Vector3.Distance(spawnPoint, player.position);

                Vector3 targetposition = player.position;

                if(distanceFromSpawn > chaseRange)
                {
                    targetposition = spawnPoint + direction * chaseRange;
                }

                agent.SetDestination(targetposition);
                break;

            case EnemyState.Return:
                agent.isStopped = false;
                agent.SetDestination(spawnPoint);
                if(Vector3.Distance(transform.position, spawnPoint) <= 0.1f)
                {
                    currentState = EnemyState.Idle;
                    agent.isStopped = true;
                }
                break;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDist);
    }
    void AttackHandler()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (currentState == EnemyState.Chase && dist <= damageRange && Time.time > lastAttackTime + fireRate)
        {
            LaunchProjectile();
            lastAttackTime = Time.time;
        }
    }

    private void LaunchProjectile()
    {
        if(projectilePrefab != null && player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            Instantiate(projectilePrefab, transform.position + new Vector3(0f, 0f, 0.5f), lookRotation);
            new WaitForSeconds(2.0f);
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;

        if (hp <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
