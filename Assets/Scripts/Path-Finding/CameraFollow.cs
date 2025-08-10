using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;    // The target to follow
    public Vector3 offset;      // Offset from the target's position
    public float smoothSpeed = 0.125f;  // Speed of the camera smoothing

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
