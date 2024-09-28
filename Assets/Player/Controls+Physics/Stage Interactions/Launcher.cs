using UnityEngine;

[ExecuteInEditMode]
public class Launcher : MonoBehaviour
{
    [SerializeField] private float upRange = 5;  // Max height of the trajectory
    [SerializeField] private int resolution = 20;  // Number of points in the curve
    [SerializeField] private bool useParabolicCurve = true;  // Toggle between curve and straight line
    [SerializeField] private LayerMask collisionMask; // Layer mask for objects that can be hit
    [SerializeField] private Transform endPoint; // Draggable end point in the scene

    void OnDrawGizmos()
    {
        if (useParabolicCurve)
        {
            DrawParabolicCurve();
        }
        else
        {
            DrawStraightLine();
        }
    }

    void DrawParabolicCurve()
    {
        Vector3 startPos = transform.position;
        Vector3 previousPos = startPos;

        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector3 point = CalculateParabolaPoint(startPos, t, endPoint.position);

            if (Physics.Raycast(previousPos, point - previousPos, out RaycastHit hit, Vector3.Distance(previousPos, point), collisionMask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(previousPos, hit.point);
                break;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(previousPos, point);
            previousPos = point;
        }
    }


    void DrawStraightLine()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = endPoint.position;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPos, endPos);
    }

    Vector3 CalculateParabolaPoint(Vector3 startPos, float t, Vector3 endPos)
    {
        Vector3 horizontalPoint = Vector3.Lerp(startPos, endPos, t);

        float parabolaHeight = 4 * upRange * t * (1 - t);

        float verticalPoint = Mathf.Lerp(startPos.y, endPos.y, t) + parabolaHeight;

        return new Vector3(horizontalPoint.x, verticalPoint, horizontalPoint.z);
    }
}