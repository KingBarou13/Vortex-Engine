using UnityEngine;

public class NearestTargetFinder : MonoBehaviour
{
    public string targetTag = "Target";
    public Transform nearestTarget;
    public Color lineColor = Color.red; // Color for the Gizmos line
    public float maxRange = 10f; // Maximum range for the target seeker
    public Camera mainCamera; // Reference to the main camera

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Automatically find the main camera if not assigned
        }
        
        FindNearestTarget();
    }

    private void Update()
    {
        FindNearestTarget();
    }

    void FindNearestTarget()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera is not assigned.");
            return;
        }

        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        if (targets.Length == 0)
        {
            nearestTarget = null;
            if (Application.isPlaying)
            {
                Debug.LogWarning("No targets found with the tag: " + targetTag);
            }
            return;
        }

        GameObject closestTarget = null;
        float minDistance = maxRange;

        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);

            // Check if the target is within the camera's view
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(target.transform.position);
            if (viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1)
            {
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = target;
                }
            }
        }

        if (closestTarget != null && minDistance <= maxRange)
        {
            nearestTarget = closestTarget.transform;
            if (Application.isPlaying)
            {
                Debug.Log("Nearest target found at: " + nearestTarget.position);
            }
        }
        else
        {
            nearestTarget = null;
            if (Application.isPlaying)
            {
                Debug.LogWarning("No target within range.");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (nearestTarget != null)
        {
            Gizmos.color = lineColor;
            Gizmos.DrawLine(transform.position, nearestTarget.position);
            if (Application.isPlaying)
            {
                Debug.Log("Drawing Gizmo line from " + transform.position + " to " + nearestTarget.position);
            }
        }
        else
        {
            if (Application.isPlaying)
            {
                Debug.Log("No nearest target to draw Gizmo line.");
            }
        }
    }
}
