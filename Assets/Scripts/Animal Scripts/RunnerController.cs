using UnityEngine;
using UnityEngine.AI;

public class RunnerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private bool playerInRange = false;
    private float wanderTimer = 0f;
    private Vector3 runDest;
    [SerializeField]
    private float wanderCooldown = 5f;
    [SerializeField]
    private float wanderSpeed = 3.5f;
    [SerializeField]
    private float runSpeed = 7f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInRange = true;
            runDest = -other.transform.position + transform.position;
            agent.speed = runSpeed;
            agent.SetDestination(runDest);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInRange = false;
            runDest = transform.position;
            agent.speed = wanderSpeed;
        }
    }

    void Update()
    {
        if (playerInRange)
        {
            runDest = transform.position - (transform.position - agent.destination).normalized * 5f;
            agent.SetDestination(runDest);
        }
        else if (wanderTimer <= 0f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 5f;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
            wanderTimer = wanderCooldown;
        }
        else
        {
            wanderTimer -= Time.deltaTime;
        }
    }
}
