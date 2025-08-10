using UnityEngine;

/// <summary>
/// Visualizes the NavMesh path using a LineRenderer component.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PathVisualizer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the AgentController that manages the NavMesh path.")]
    public NavigationAgent controller;

    [Header("Path Drawing Settings")]
    [Tooltip("Vertical offset above the floor to avoid z-fighting.")]
    [Range(0f, 0.05f)]
    public float yOffset = 0.015f;       

    [Tooltip("Number of times per second the path is redrawn.")]
    [Range(1f, 15f)]
    public float updateHz = 5f;

    [Tooltip("Minimum positional change in meters before updating the path.")]
    [Range(0.01f, 1f)]
    public float changeEpsilon = 0.01f;

    [Tooltip("Width of the drawn path in meters.")]
    [Range(0.01f, 1f)]
    public float width = 0.05f;

    // Internal references
    private LineRenderer line;                      // LineRenderer used to visualize the path
    private float nextUpdate;                       // Next allowed update time
    private Vector3[] lastCorners = System.Array.Empty<Vector3>();  // Previously drawn path corners


    /// <summary>
    /// Initializes the LineRenderer component.
    /// </summary>
    void Awake()
    {
        line = GetComponent<LineRenderer>();    // Get the LineRenderer component
        line.useWorldSpace = true;              // Corner positions are in world space
        line.alignment = LineAlignment.View;    // Align the line with the camera view
        line.widthMultiplier = width;           // Set the line width
    }

    /// <summary>
    /// Updates the line renderer with the current path.
    /// </summary>
    void LateUpdate()
    {
        // Check if the controller is assigned
        if (!controller) return;

        // throttle updates
        if (Time.time < nextUpdate) return; // If the current time is less than the next update time, return
        nextUpdate = Time.time + 1f / Mathf.Max(1f, updateHz); // Update the next allowed time

        // Get the current path from the controller
        var path = controller.GetCurrentPath();
        // Check if the path is valid
        if (path == null || path.corners == null || path.corners.Length < 2)
        {
            // if it's invalid, clear the line renderer and last corners
            line.positionCount = 0;
            lastCorners = System.Array.Empty<Vector3>();
            return;
        }

        // Copy & lift all corners to a fixed Y
        var corners = path.corners; // Get the path corners
        float baseY = controller.transform.position.y + yOffset; // Lift above floor to avoid z-fighting

        // Iterate through all corners and set their Y position
        for (int i = 0; i < corners.Length; i++)
            corners[i].y = baseY;

        // Check if the path has changed
        if (!PathChanged(lastCorners, corners, changeEpsilon)) return;

        // Update the line renderer
        line.positionCount = corners.Length;
        line.SetPositions(corners);
        lastCorners = (Vector3[])corners.Clone();
    }


    /// <summary>
    /// Checks if the path has changed significantly.
    /// </summary>
    /// <param name="a">The first path array to compare.</param>
    /// <param name="b">The second path array to compare.</param>
    /// <param name="eps">The epsilon value for determining significant changes.</param>
    /// <returns>True if the path has changed significantly; otherwise, false.</returns>
    bool PathChanged(Vector3[] a, Vector3[] b, float eps)
    {
        // Check if the arrays are null or of different lengths
        if (a == null || a.Length != b.Length) return true;
        float e2 = eps * eps; // Square the epsilon value

        // Compare each corner's position
        for (int i = 0; i < a.Length; i++)
        {
            // Compare the squared distance between corners
            if ((a[i] - b[i]).sqrMagnitude > e2) return true;
        }
        // If we reach this point, the paths are considered equal
        return false;
    }
}
