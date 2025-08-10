using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class NavMeshPathDrawer : MonoBehaviour
{
    public AgentController controller;   // Reference to the controller script
    private LineRenderer line;  // Reference to the LineRenderer component

    // Start is called before the first frame update
    void Start()
    {   
        // Get the LineRenderer component
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {   
        // If the controller is not set or null, do nothing
        if (controller == null) return;

        // Get the current path from the controller
        NavMeshPath path = controller.GetCurrentPath();

        // If the path is valid, set the positions of the LineRenderer
        if (path != null && path.corners.Length > 1)
        {   // Set the number of positions in the LineRenderer
            line.positionCount = path.corners.Length;
            line.SetPositions(path.corners);
        }
        else
        {   
            // If the path is invalid, clear the LineRenderer
            line.positionCount = 0;
        }
    }
}
