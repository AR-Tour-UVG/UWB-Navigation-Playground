using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] // Ensure the GameObject has a NavMeshAgent component
public class AgentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform target;          // Destination in the scene
    public float followSpeed = 5f;    // Smoothing speed towards UWB position
    public float sampleRadius = 0.3f; // Max search radius for nearest NavMesh point

    [Header("Path Recompute")]
    public float pathRefreshHz = 5f;     // recompute path at most 5 Hz
    public float minMoveToRepath = 0.5f; // 50 cm movement triggers repath

    private NavMeshAgent agent;       // NavMeshAgent for path calculation
    private NavMeshPath navPath;      // The current path to the target
    private Vector3 lastKnownPosition; // Last known position of the agent

    // internal repath state
    private float nextPathTime;
    private Vector3 lastPathFrom;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        // Disable automatic position and rotation updates
        agent.updatePosition = false;
        agent.updateRotation = false;

        // Initialize the NavMeshPath and last known position
        navPath = new NavMeshPath();
        lastKnownPosition = transform.position;
        lastPathFrom = transform.position;
    }

    /// <summary>
    /// Initializes the NavMeshAgent and sets up the pathfinding.
    /// </summary>
    /// <returns>
    /// An enumerator for the coroutine.
    /// </returns>
    private IEnumerator Start()
    {
        // Gate: wait until NavMesh is ready before enabling the agent
        agent.enabled = false; // Disable the NavMeshAgent to prevent movement before NavMesh is ready
        while (NavMesh.CalculateTriangulation().vertices.Length == 0)
        {
            // If the NavMesh is not ready, wait until it is
            yield return null;
            Debug.LogWarning("Waiting for NavMesh to be ready...");
        }

        // Snap starting transform to nearest on-Mesh position
        if (NavMesh.SamplePosition(transform.position, out var hit, 1f, NavMesh.AllAreas))
        {
            transform.position = hit.position; // Snap to nearest NavMesh position
            lastKnownPosition = hit.position; // Update last known position
            lastPathFrom = hit.position;      // Update last path start position
        }

        agent.enabled = true; // Enable the NavMeshAgent after confirming NavMesh is ready
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

            // Check if there's a target and the agent is enabled
            if (target != null && agent.enabled)
            {
                // Determine if the agent has moved enough
                bool movedEnough =
                (transform.position - lastPathFrom).sqrMagnitude >
                (minMoveToRepath * minMoveToRepath);
                bool timeout = Time.time >= nextPathTime;

                // Check if the agent has moved enough or if the path has timed out
                if (movedEnough || timeout)
                {
                    // Calculate the path to the target
                    NavMesh.CalculatePath(
                        transform.position,
                        target.position,
                        NavMesh.AllAreas,
                        navPath
                    );
                    lastPathFrom = transform.position; // Update last path start position
                    nextPathTime = Time.time + 1f / Mathf.Max(1f, pathRefreshHz); // Update next path time
                }
            }
            else
            {
                // No target to follow -> clear the path
                navPath.ClearCorners();
            }

        }
#if UNITY_EDITOR
        // else: No valid UWB position found this frame -> keep the last known position
        // but get calculated path to target just for debugging purposes
        NavMesh.CalculatePath(
            lastKnownPosition,
            target.position,
            NavMesh.AllAreas,
            navPath
        );
#endif
    }

    /// <summary>
    /// Gets the current NavMesh path.
    /// </summary>
    /// <returns>The current NavMesh path.</returns>
    public NavMeshPath GetCurrentPath() => navPath;
}
