using UnityEngine;

/// <summary>
/// Follows the target from a top-down perspective.
/// </summary>
[RequireComponent(typeof(Camera))]
public class TopDownFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform of the agent or object to follow.")]
    public Transform target;

    [Header("View Settings")]
    [Tooltip("If true, the camera will use orthographic projection for a consistent top-down view.")]
    public bool useOrthographic = true;

    [Tooltip("Orthographic size (half of the camera's vertical viewing volume in world units).")]
    [Range(1f, 3f)]
    public float orthoSize = 1.6f;

    [Tooltip("Height of the camera above the ground plane.")]
    [Range(1f, 5f)]
    public float height = 2.0f;

    [Tooltip("Offset applied in the X and Z axes relative to the target.")]
    public Vector2 offsetXZ = Vector2.zero;

    [Header("Smoothing")]
    [Tooltip("Time in seconds to smooth the camera movement toward the target position.")]
    [Range(0f, 1f)]
    public float smoothTime = 0.18f;

    [Header("Room Bounds")]
    [Tooltip("World position of the bottom-left corner of the room in XZ coordinates.")]
    public Vector2 roomOrigin = Vector2.zero;

    [Tooltip("Width and depth of the room in world units (X, Z).")]
    public Vector2 roomSize = new Vector2(3.33f, 2.57f);

    [Tooltip("Padding to keep the camera from reaching the exact room edges.")]
    public float edgePadding = 0.05f;

    // Cached reference to the camera component
    private Camera cam;

    // Current velocity used by SmoothDamp for smoothing motion
    private Vector3 velocity;

    /// <summary>
    /// Initializes the camera component.
    /// </summary>
    private void Awake()
    {
        cam = GetComponent<Camera>(); // Get the Camera component

        // Configure projection mode
        if (useOrthographic)
        {   
            cam.orthographic = true; // Set camera to orthographic
            cam.orthographicSize = orthoSize; // Set orthographic size
        }

        // Ensure the camera is facing straight downward
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    /// <summary>
    /// Late update for camera follow behavior.
    /// </summary>
    private void LateUpdate()
    {
        // Skip update if there is no target to follow
        if (!target) return;

        // Determine desired position directly above the target with planar offset
        Vector3 desiredPosition = new Vector3(
            target.position.x + offsetXZ.x,
            height,
            target.position.z + offsetXZ.y
        );

        // Clamp the desired position within room boundaries
        float minX = roomOrigin.x + edgePadding;
        float maxX = roomOrigin.x + roomSize.x - edgePadding;
        float minZ = roomOrigin.y + edgePadding;
        float maxZ = roomOrigin.y + roomSize.y - edgePadding;

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX); // Clamp X position
        desiredPosition.z = Mathf.Clamp(desiredPosition.z, minZ, maxZ); // Clamp Z position

        // Smoothly interpolate camera position toward the desired position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }
}
