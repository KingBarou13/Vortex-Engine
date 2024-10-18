using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class RailGrindTrigger : MonoBehaviour
{
    public float grindSpeed = 5f;
    public float minGrindSpeed = 0.2f;
    private SplineContainer currentSpline;
    public bool isGrinding = false;
    private float splineProgress = 0f;
    private bool isReversing = false;
    [SerializeField] private Animator animator;

    [SerializeField] private PlayerPhysics playerPhysics;
    [SerializeField] MoveAction moveAction;

    private void Update()
    {
        if (isGrinding)
        {
            moveAction.enabled = false;

            if (isReversing)
            {
                splineProgress -= grindSpeed * Time.deltaTime;
            }
            else
            {
                splineProgress += grindSpeed * Time.deltaTime;
            }

            splineProgress = Mathf.Clamp01(splineProgress);

            Vector3 railPosition = currentSpline.EvaluatePosition(splineProgress);
            playerPhysics.RB.MovePosition(railPosition);

            Vector3 tangent = math.normalize(currentSpline.EvaluateTangent(splineProgress));
            playerPhysics.RB.MoveRotation(Quaternion.LookRotation(tangent));

            if (splineProgress <= 0f || splineProgress >= 1f)
            {
                ExitGrind();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SplineContainer spline = other.GetComponent<SplineContainer>();
        if (spline != null && other is MeshCollider)
        {
            grindSpeed = Mathf.Max(playerPhysics.horizontalVelocity.magnitude / 100, minGrindSpeed);
            StartGrind(spline);
        }
    }

    private void StartGrind(SplineContainer spline)
    {
        currentSpline = spline;
        Vector3 playerDirection = playerPhysics.RB.velocity.normalized;
        splineProgress = FindClosestPointOnSpline(currentSpline, transform.position);

        Vector3 splineTangent = math.normalize(currentSpline.EvaluateTangent(splineProgress));
        float dotProduct = Vector3.Dot(playerDirection, splineTangent);

        isReversing = dotProduct < 0;
        isGrinding = true;
    }

    private float FindClosestPointOnSpline(SplineContainer spline, Vector3 position)
    {
        float closestProgress = 0f;
        float closestDistance = float.MaxValue;

        for (float t = 0; t <= 1; t += 0.01f)
        {
            Vector3 splinePosition = spline.EvaluatePosition(t);
            float distance = Vector3.Distance(position, splinePosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestProgress = t;
            }
        }

        return closestProgress;
    }

    private void ExitGrind()
    {
        isGrinding = false;
        currentSpline = null;
        moveAction.enabled = true;
    }
}
