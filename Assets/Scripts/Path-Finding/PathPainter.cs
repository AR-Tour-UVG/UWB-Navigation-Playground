using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NavMeshPathDrawer : MonoBehaviour
{
    public AgentController controller;  // Reference to the controller script
    public float yOffset = 0.015f;       // lift above floor to avoid z-fighting
    public float updateHz = 5f;          // redraw rate
    public float changeEpsilon = 0.01f;  // 1 cm tolerance
    public float width = 0.05f;          // meters

    LineRenderer line; // Reference to the LineRenderer component
    float nextUpdate; // Time until the next update
    Vector3[] lastCorners = System.Array.Empty<Vector3>();  // Last drawn path corners

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
