using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] // Ensure the GameObject has a NavMeshAgent component
public class AgentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform target;          // Destination in the scene
    public float followSpeed = 5f;    // Smoothing speed towards UWB position
    public float sampleRadius = 0.3f; // Max search radius for nearest NavMesh point

    [Header("NavMesh Settings")]
    private NavMeshAgent agent;       // NavMeshAgent for path calculation
    private NavMeshPath navPath;      // The current path to the target
    private Vector3 lastKnownPosition; // Last known position of the agent

    /// <summary>
    /// Initializes the NavMeshAgent and sets up the pathfinding.
    /// </summary>
    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component

        // Disable automatic position and rotation updates
        agent.updatePosition = false;
        agent.updateRotation = false;

        // Initialize the NavMeshPath and last known position
        navPath = new NavMeshPath(); // Initialize the NavMeshPath
        lastKnownPosition = transform.position; // Store the initial position

        // Verify NavMesh exists at runtime
        var triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogError("UWB: No NavMesh loaded. Ensure NavMeshSurface is baked and scene is included in build.");
        }
    }

    /// <summary>
    /// Updates the agent's position and pathfinding.
    /// </summary>
    void Update()
    {
        // Attempt to get UWB position from the bridge
        if (UWBBridge.GetUWBPosition(out Vector3 uwbPosition))
        {
            // Preserve current height (Y)
            uwbPosition.y = transform.position.y;

            // Snap to nearest valid point inside the NavMesh if possible
            if (NavMesh.SamplePosition(uwbPosition, out var hit, sampleRadius, NavMesh.AllAreas))
            {
                lastKnownPosition = hit.position; // Update last known position
            }

            // Smoothly move toward the last known position
            transform.position = Vector3.Lerp(
                transform.position,
                lastKnownPosition,
                Time.deltaTime * followSpeed
            );

            // Check if there's a target
            if (target != null)
            {
                // Calculate the path to the target
                NavMesh.CalculatePath(
                    transform.position,
                    target.position,
                    NavMesh.AllAreas,
                    navPath
                );
            }
        }
        // else: No valid UWB position found this frame -> keep the last known position
    }

    public NavMeshPath GetCurrentPath()
    {
        return navPath;
    }
}
