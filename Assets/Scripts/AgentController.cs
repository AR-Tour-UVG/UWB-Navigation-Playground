using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentController : MonoBehaviour
{
    public Transform target;          // Destination in the scene
    public float followSpeed = 5f;    // Smoothing speed
    private NavMeshAgent agent;       // NavMeshAgent for path calculation
    private NavMeshPath navPath;      // The current path to the target

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;

        navPath = new NavMeshPath();
    }

    void Update()
    {
        Vector3? uwbPos = UWBBridge.GetUWBPosition();

        if (uwbPos.HasValue)
        {
            // Update the position based on UWB data
            transform.position = Vector3.Lerp(transform.position, uwbPos.Value, Time.deltaTime * followSpeed);

            // Recalculate the path to the target
            if (target != null)
            {
                NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, navPath);
            }
        }
        else
        {
            Debug.Log("UWB: No valid position received.");
        }
    }

    public NavMeshPath GetCurrentPath()
    {
        return navPath;
    }
}
