using UnityEngine;
using UnityEngine.AI;

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
    Vector3[] lastCorners = System.Array.Empty<Vector3>();

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;                // IMPORTANT: corners are in world space
        line.alignment = LineAlignment.View;
        line.widthMultiplier = width;
        // Set an Unlit material in the Inspector (URP/Unlit or Sprites/Default)
        // Disable shadows on the Renderer
    }

    void LateUpdate()
    {
        if (!controller) return;

        // throttle
        if (Time.time < nextUpdate) return;
        nextUpdate = Time.time + 1f / Mathf.Max(1f, updateHz);

        var path = controller.GetCurrentPath();
        if (path == null || path.corners == null || path.corners.Length < 2)
        {
            line.positionCount = 0;
            lastCorners = System.Array.Empty<Vector3>();
            return;
        }

        // copy & lift all corners to a fixed Y
        var corners = path.corners;
        float baseY = controller.transform.position.y + yOffset;
        for (int i = 0; i < corners.Length; i++)
            corners[i].y = baseY;

        if (!PathChanged(lastCorners, corners, changeEpsilon)) return;

        line.positionCount = corners.Length;
        line.SetPositions(corners);
        lastCorners = (Vector3[])corners.Clone();
    }

    bool PathChanged(Vector3[] a, Vector3[] b, float eps)
    {
        if (a == null || a.Length != b.Length) return true;
        float e2 = eps * eps;
        for (int i = 0; i < a.Length; i++)
            if ((a[i] - b[i]).sqrMagnitude > e2) return true;
        return false;
    }
}
